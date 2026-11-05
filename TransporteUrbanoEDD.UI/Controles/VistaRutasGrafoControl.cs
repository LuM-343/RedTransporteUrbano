using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.UI.Controles
{
    /// <summary>
    /// Control de usuario independiente para el Módulo de Calculadora de Rutas,
    /// Recorridos en Grafos (BFS/DFS) y Visualización de Mapa Vial interactivo.
    /// Desarrollado cumpliendo estrictamente con la regla de no usar colecciones prohibidas
    /// (sin List, Dictionary, HashSet, Stack ni LINQ).
    /// </summary>
    public class VistaRutasGrafoControl : UserControl
    {
        // Referencias a datos del Core
        private OrquestadorTransporte? orquestador;
        private GrafoTransporte? grafo;
        private ResultadoRuta? rutaActiva;
        private ListaPropia<Parada>? recorridoActivo;
        private string tipoRecorridoActivo = string.Empty;

        /// <summary>
        /// Evento notificado cuando se registra exitosamente una nueva ruta.
        /// Permite a FormPrincipal refrescar la barra de estado y la conectividad en otras pestañas.
        /// </summary>
        public event EventHandler? RutaRegistrada;

        // Controles de interfaz (Panel Izquierdo)
        private Panel panelLateral = null!;
        private ComboBox cboOrigen = null!;
        private ComboBox cboDestino = null!;
        private RadioButton rbTiempo = null!;
        private RadioButton rbCosto = null!;
        private Button btnCalcularDijkstra = null!;
        private Label lblEstadoRuta = null!;
        private Label lblResumenCamino = null!;
        private Label lblTiempoTotal = null!;
        private Label lblCostoTotal = null!;

        // Controles de Recorridos
        private ComboBox cboInicioRecorrido = null!;
        private Button btnBFS = null!;
        private Button btnDFS = null!;
        private TextBox txtResultadoRecorrido = null!;

        // Controles para Agregar Rutas
        private ComboBox cboNuevaOrigen = null!;
        private ComboBox cboNuevaDestino = null!;
        private NumericUpDown numTiempo = null!;
        private NumericUpDown numCosto = null!;
        private CheckBox chkDobleVia = null!;
        private Button btnAgregarRuta = null!;

        // Botón de Limpieza y Acciones
        private Button btnLimpiarMapa = null!;

        // Lienzo del Mapa (Panel Derecho)
        private PanelDobleBuffer panelMapa = null!;

        // Interactividad: Zoom, Panorámica (Pan) y Hover
        private float zoom = 1.0f;
        private float panX = 0f;
        private float panY = 0f;
        private bool isDragging = false;
        private bool hasMovedSignificantly = false;
        private Point dragStartPoint;
        private Point lastMousePos;
        private Parada? paradaHover = null;

        // Barra de herramientas flotante del mapa (Esquina Superior)
        private Panel pnlToolbarMapa = null!;
        private CheckBox chkMostrarPesos = null!;
        private Button btnCentrarMapa = null!;
        private Button btnZoomOut = null!;
        private Button btnZoomIn = null!;
        private Label lblZoomIndicador = null!;

        public VistaRutasGrafoControl()
        {
            InicializarComponentes();
        }

        #region Inicialización Visual

        private void InicializarComponentes()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            // 1. Panel Lateral de Controles (Izquierda)
            panelLateral = new Panel
            {
                Dock = DockStyle.Left,
                Width = 380,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12)
            };

            // 2. Panel Derecho (Lienzo del Mapa con Doble Búfer)
            panelMapa = new PanelDobleBuffer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 255, 255),
                Cursor = Cursors.Default
            };
            panelMapa.Paint += PanelMapa_Paint;
            panelMapa.MouseClick += PanelMapa_MouseClick;
            panelMapa.MouseDoubleClick += PanelMapa_MouseDoubleClick;
            panelMapa.MouseDown += PanelMapa_MouseDown;
            panelMapa.MouseMove += PanelMapa_MouseMove;
            panelMapa.MouseUp += PanelMapa_MouseUp;
            panelMapa.MouseWheel += PanelMapa_MouseWheel;
            panelMapa.MouseEnter += (s, e) => panelMapa.Focus();
            panelMapa.MouseLeave += PanelMapa_MouseLeave;

            // Barra flotante de herramientas del mapa (esquina superior)
            ConstruirBarraHerramientasMapa();

            // Construcción modular de las secciones del panel lateral
            ConstruirEncabezado();
            ConstruirSeccionDijkstra();
            ConstruirSeccionRecorridos();
            ConstruirSeccionAgregarRuta();
            ConstruirSeccionAcciones();

            Controls.Add(panelMapa);
            Controls.Add(panelLateral);

            ResumeLayout(false);
        }

        private void ConstruirBarraHerramientasMapa()
        {
            pnlToolbarMapa = new Panel
            {
                Location = new Point(12, 12),
                Height = 36,
                Width = 430,
                BackColor = Color.FromArgb(245, 255, 255, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(6, 4, 6, 4)
            };

            // CheckBox para mostrar u ocultar pesos en las vías
            chkMostrarPesos = new CheckBox
            {
                Text = "Mostrar pesos en vías",
                Checked = true,
                Top = 7,
                Left = 10,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                Cursor = Cursors.Hand
            };
            chkMostrarPesos.CheckedChanged += (s, e) => panelMapa.Invalidate();

            Label lblSep = new Label
            {
                Text = "|",
                Top = 7,
                Left = 168,
                Width = 10,
                Height = 20,
                ForeColor = Color.FromArgb(203, 213, 225)
            };

            btnCentrarMapa = new Button
            {
                Text = "🎯 Centrar Mapa",
                Top = 4,
                Left = 184,
                Width = 100,
                Height = 26,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCentrarMapa.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCentrarMapa.Click += (s, e) => CentrarMapa();

            btnZoomOut = new Button
            {
                Text = "－",
                Top = 4,
                Left = 292,
                Width = 28,
                Height = 26,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomOut.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnZoomOut.Click += (s, e) => AplicarZoomCentrado(1f / 1.2f);

            lblZoomIndicador = new Label
            {
                Text = "100%",
                Top = 8,
                Left = 324,
                Width = 46,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            btnZoomIn = new Button
            {
                Text = "＋",
                Top = 4,
                Left = 374,
                Width = 28,
                Height = 26,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnZoomIn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnZoomIn.Click += (s, e) => AplicarZoomCentrado(1.2f);

            pnlToolbarMapa.Controls.Add(chkMostrarPesos);
            pnlToolbarMapa.Controls.Add(lblSep);
            pnlToolbarMapa.Controls.Add(btnCentrarMapa);
            pnlToolbarMapa.Controls.Add(btnZoomOut);
            pnlToolbarMapa.Controls.Add(lblZoomIndicador);
            pnlToolbarMapa.Controls.Add(btnZoomIn);

            panelMapa.Controls.Add(pnlToolbarMapa);
            pnlToolbarMapa.BringToFront();
        }

        private void ConstruirEncabezado()
        {
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10)
            };

            Label lblTitulo = new Label
            {
                Text = "Red Vial y Rutas Óptimas",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 24
            };

            Label lblSubtitulo = new Label
            {
                Text = "Dijkstra (Tiempo/Costo), BFS, DFS y Visualización",
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                Dock = DockStyle.Bottom,
                Height = 18
            };

            pnlHeader.Controls.Add(lblSubtitulo);
            pnlHeader.Controls.Add(lblTitulo);
            panelLateral.Controls.Add(pnlHeader);
        }

        private void ConstruirSeccionDijkstra()
        {
            GroupBox gb = new GroupBox
            {
                Text = " 1. Calculadora de Rutas (Dijkstra) ",
                Dock = DockStyle.Top,
                Height = 280,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10)
            };

            Label lblOrig = new Label { Text = "Parada Origen:", Top = 25, Left = 12, Width = 100, Font = new Font("Segoe UI", 8.5F) };
            cboOrigen = new ComboBox { Top = 22, Left = 115, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            cboOrigen.SelectedIndexChanged += (s, e) => panelMapa.Invalidate();

            Label lblDest = new Label { Text = "Parada Destino:", Top = 55, Left = 12, Width = 100, Font = new Font("Segoe UI", 8.5F) };
            cboDestino = new ComboBox { Top = 52, Left = 115, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };
            cboDestino.SelectedIndexChanged += (s, e) => panelMapa.Invalidate();

            // Criterio de optimización: Al cambiar, se invalida el mapa para actualizar las etiquetas de peso activas
            rbTiempo = new RadioButton { Text = "Menor Tiempo (min)", Top = 85, Left = 15, Width = 160, Checked = true, Font = new Font("Segoe UI", 8.5F) };
            rbTiempo.CheckedChanged += (s, e) => panelMapa.Invalidate();

            rbCosto = new RadioButton { Text = "Menor Costo (Q)", Top = 85, Left = 180, Width = 150, Font = new Font("Segoe UI", 8.5F) };
            rbCosto.CheckedChanged += (s, e) => panelMapa.Invalidate();

            btnCalcularDijkstra = new Button
            {
                Text = "Calcular Ruta Óptima",
                Top = 115,
                Left = 12,
                Width = 333,
                Height = 32,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCalcularDijkstra.FlatAppearance.BorderSize = 0;
            btnCalcularDijkstra.Click += BtnCalcularDijkstra_Click;

            // Panel para mostrar resultados de Dijkstra
            Panel pnlResultados = new Panel
            {
                Top = 155,
                Left = 12,
                Width = 333,
                Height = 110,
                BackColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(6)
            };

            lblEstadoRuta = new Label
            {
                Text = "Seleccione origen, destino y presione Calcular.",
                Top = 4,
                Left = 6,
                Width = 320,
                Height = 18,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            lblResumenCamino = new Label
            {
                Text = "Camino: -",
                Top = 24,
                Left = 6,
                Width = 320,
                Height = 36,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            lblTiempoTotal = new Label
            {
                Text = "Tiempo: -",
                Top = 64,
                Left = 6,
                Width = 150,
                Height = 20,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            lblCostoTotal = new Label
            {
                Text = "Costo: -",
                Top = 64,
                Left = 165,
                Width = 150,
                Height = 20,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            pnlResultados.Controls.Add(lblEstadoRuta);
            pnlResultados.Controls.Add(lblResumenCamino);
            pnlResultados.Controls.Add(lblTiempoTotal);
            pnlResultados.Controls.Add(lblCostoTotal);

            gb.Controls.Add(lblOrig);
            gb.Controls.Add(cboOrigen);
            gb.Controls.Add(lblDest);
            gb.Controls.Add(cboDestino);
            gb.Controls.Add(rbTiempo);
            gb.Controls.Add(rbCosto);
            gb.Controls.Add(btnCalcularDijkstra);
            gb.Controls.Add(pnlResultados);

            panelLateral.Controls.Add(gb);
            gb.BringToFront();
        }

        private void ConstruirSeccionRecorridos()
        {
            GroupBox gb = new GroupBox
            {
                Text = " 2. Recorridos de la Red (BFS / DFS) ",
                Dock = DockStyle.Top,
                Height = 190,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10)
            };

            Label lblInicio = new Label { Text = "Parada Inicio:", Top = 24, Left = 12, Width = 100, Font = new Font("Segoe UI", 8.5F) };
            cboInicioRecorrido = new ComboBox { Top = 21, Left = 115, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };

            btnBFS = new Button
            {
                Text = "Recorrido BFS (Anchura)",
                Top = 55,
                Left = 12,
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold)
            };
            btnBFS.FlatAppearance.BorderSize = 0;
            btnBFS.Click += BtnBFS_Click;

            btnDFS = new Button
            {
                Text = "Recorrido DFS (Profundidad)",
                Top = 55,
                Left = 185,
                Width = 160,
                Height = 30,
                BackColor = Color.FromArgb(147, 51, 234),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold)
            };
            btnDFS.FlatAppearance.BorderSize = 0;
            btnDFS.Click += BtnDFS_Click;

            txtResultadoRecorrido = new TextBox
            {
                Top = 95,
                Left = 12,
                Width = 333,
                Height = 80,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Consolas", 8.5F),
                Text = "Seleccione la parada inicial y ejecute un recorrido."
            };

            gb.Controls.Add(lblInicio);
            gb.Controls.Add(cboInicioRecorrido);
            gb.Controls.Add(btnBFS);
            gb.Controls.Add(btnDFS);
            gb.Controls.Add(txtResultadoRecorrido);

            panelLateral.Controls.Add(gb);
            gb.BringToFront();
        }

        private void ConstruirSeccionAgregarRuta()
        {
            GroupBox gb = new GroupBox
            {
                Text = " 3. Agregar Nueva Conexión Vial ",
                Dock = DockStyle.Top,
                Height = 175,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(10)
            };

            Label lblO = new Label { Text = "De:", Top = 25, Left = 12, Width = 30, Font = new Font("Segoe UI", 8.5F) };
            cboNuevaOrigen = new ComboBox { Top = 22, Left = 45, Width = 125, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8.5F) };

            Label lblD = new Label { Text = "A:", Top = 25, Left = 180, Width = 25, Font = new Font("Segoe UI", 8.5F) };
            cboNuevaDestino = new ComboBox { Top = 22, Left = 205, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 8.5F) };

            Label lblT = new Label { Text = "Tiempo (min):", Top = 58, Left = 12, Width = 85, Font = new Font("Segoe UI", 8.5F) };
            numTiempo = new NumericUpDown { Top = 55, Left = 100, Width = 70, Minimum = 1, Maximum = 300, Value = 10, DecimalPlaces = 1 };

            Label lblC = new Label { Text = "Costo (Q):", Top = 58, Left = 185, Width = 65, Font = new Font("Segoe UI", 8.5F) };
            numCosto = new NumericUpDown { Top = 55, Left = 255, Width = 90, Minimum = 0, Maximum = 100, Value = 3, DecimalPlaces = 2 };

            chkDobleVia = new CheckBox { Text = "Vía de doble sentido (Bidireccional)", Top = 90, Left = 15, Width = 260, Checked = true, Font = new Font("Segoe UI", 8.5F) };

            btnAgregarRuta = new Button
            {
                Text = "Registrar Conexión Vial",
                Top = 122,
                Left = 12,
                Width = 333,
                Height = 32,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnAgregarRuta.FlatAppearance.BorderSize = 0;
            btnAgregarRuta.Click += BtnAgregarRuta_Click;

            gb.Controls.Add(lblO);
            gb.Controls.Add(cboNuevaOrigen);
            gb.Controls.Add(lblD);
            gb.Controls.Add(cboNuevaDestino);
            gb.Controls.Add(lblT);
            gb.Controls.Add(numTiempo);
            gb.Controls.Add(lblC);
            gb.Controls.Add(numCosto);
            gb.Controls.Add(chkDobleVia);
            gb.Controls.Add(btnAgregarRuta);

            panelLateral.Controls.Add(gb);
            gb.BringToFront();
        }

        private void ConstruirSeccionAcciones()
        {
            Panel pnlAcciones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                Padding = new Padding(12, 6, 12, 6)
            };

            Button btnCentrarLateral = new Button
            {
                Text = "🎯 Centrar Mapa (Restablecer Zoom)",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnCentrarLateral.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCentrarLateral.Click += (s, e) => CentrarMapa();

            btnLimpiarMapa = new Button
            {
                Text = "Limpiar Resaltado del Mapa",
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnLimpiarMapa.FlatAppearance.BorderSize = 0;
            btnLimpiarMapa.Click += BtnLimpiarMapa_Click;

            pnlAcciones.Controls.Add(btnCentrarLateral);
            pnlAcciones.Controls.Add(btnLimpiarMapa);
            panelLateral.Controls.Add(pnlAcciones);
            pnlAcciones.BringToFront();
        }

        #endregion

        #region Enlace de Datos (ConectarDatos)

        /// <summary>
        /// Conecta el Orquestador central con el control visual y actualiza los componentes.
        /// </summary>
        public void ConectarDatos(OrquestadorTransporte orquestadorTransporte)
        {
            orquestador = orquestadorTransporte ?? throw new ArgumentNullException(nameof(orquestadorTransporte));
            grafo = orquestador.Grafo;
            ActualizarCombosParadas();
            panelMapa.Invalidate();
        }

        public void ConectarDatos(GrafoTransporte grafoTransporte)
        {
            grafo = grafoTransporte ?? throw new ArgumentNullException(nameof(grafoTransporte));
            ActualizarCombosParadas();
            panelMapa.Invalidate();
        }

        /// <summary>
        /// Sobrecarga flexible para permitir la conexión de datos proveniente de OrquestadorTransporte
        /// </summary>
        public void ConectarDatos(GrafoTransporte grafoTransporte, object? tablaHashParadas, object? unionFindZonas = null)
        {
            ConectarDatos(grafoTransporte);
        }

        /// <summary>
        /// Llena los ComboBoxes con las paradas del grafo utilizando ListaPropia (sin colecciones prohibidas).
        /// </summary>
        public void ActualizarCombosParadas()
        {
            if (grafo == null) return;

            cboOrigen.Items.Clear();
            cboDestino.Items.Clear();
            cboInicioRecorrido.Items.Clear();
            cboNuevaOrigen.Items.Clear();
            cboNuevaDestino.Items.Clear();

            // Iteración legal sobre ListaPropia<Parada>
            foreach (Parada p in grafo.ObtenerTodasLasParadas())
            {
                string item = $"{p.Codigo} - {p.Nombre}";
                cboOrigen.Items.Add(item);
                cboDestino.Items.Add(item);
                cboInicioRecorrido.Items.Add(item);
                cboNuevaOrigen.Items.Add(item);
                cboNuevaDestino.Items.Add(item);
            }

            if (cboOrigen.Items.Count > 0) cboOrigen.SelectedIndex = 0;
            if (cboDestino.Items.Count > 1) cboDestino.SelectedIndex = 1;
            if (cboInicioRecorrido.Items.Count > 0) cboInicioRecorrido.SelectedIndex = 0;
            if (cboNuevaOrigen.Items.Count > 0) cboNuevaOrigen.SelectedIndex = 0;
            if (cboNuevaDestino.Items.Count > 1) cboNuevaDestino.SelectedIndex = 1;
        }

        private string ExtraerCodigo(object? itemSeleccionado)
        {
            if (itemSeleccionado == null)
                return string.Empty;

            string texto = itemSeleccionado.ToString() ?? string.Empty;

            // Buscar el separador entre el código y el nombre
            int separador = texto.IndexOf(" - ", StringComparison.Ordinal);

            if (separador > 0)
            {
                return texto.Substring(0, separador).Trim();
            }

            return texto.Trim();
        }

        #endregion

        #region Eventos de Usuario

        private void BtnCalcularDijkstra_Click(object? sender, EventArgs e)
        {
            if (grafo == null)
            {
                MessageBox.Show("No se ha conectado la estructura del Grafo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string codOrigen = ExtraerCodigo(cboOrigen.SelectedItem);
            string codDestino = ExtraerCodigo(cboDestino.SelectedItem);

            if (string.IsNullOrEmpty(codOrigen) || string.IsNullOrEmpty(codDestino))
            {
                MessageBox.Show("Seleccione una parada de origen y una de destino.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool porTiempo = rbTiempo.Checked;
            recorridoActivo = null;
            tipoRecorridoActivo = string.Empty;

            // Ejecución de Dijkstra parametrizado
            if (orquestador != null)
            {
                rutaActiva = orquestador.BuscarRutaOptima(codOrigen, codDestino, porTiempo);
            }
            else
            {
                rutaActiva = grafo.CalcularDijkstra(codOrigen, codDestino, porTiempo);
            }

            if (rutaActiva.ExisteRuta)
            {
                lblEstadoRuta.Text = "✓ " + rutaActiva.MensajeEstado;
                lblEstadoRuta.ForeColor = Color.FromArgb(22, 163, 74);
                lblResumenCamino.Text = "Camino: " + rutaActiva.ResumenCamino;
                lblTiempoTotal.Text = $"Tiempo: {rutaActiva.TiempoTotalMinutos:F1} min";
                lblCostoTotal.Text = $"Costo: Q{rutaActiva.CostoTotal:F2}";
            }
            else
            {
                lblEstadoRuta.Text = "✗ " + rutaActiva.MensajeEstado;
                lblEstadoRuta.ForeColor = Color.FromArgb(220, 38, 38);
                lblResumenCamino.Text = "Camino: Inaccesible";
                lblTiempoTotal.Text = "Tiempo: -";
                lblCostoTotal.Text = "Costo: -";
            }

            panelMapa.Invalidate();
        }

        private void BtnBFS_Click(object? sender, EventArgs e)
        {
            EjecutarRecorrido(esBFS: true);
        }

        private void BtnDFS_Click(object? sender, EventArgs e)
        {
            EjecutarRecorrido(esBFS: false);
        }

        private void EjecutarRecorrido(bool esBFS)
        {
            if (grafo == null) return;

            string codInicio = ExtraerCodigo(cboInicioRecorrido.SelectedItem);
            if (string.IsNullOrEmpty(codInicio))
            {
                MessageBox.Show("Seleccione una parada de inicio para el recorrido.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            rutaActiva = null;
            tipoRecorridoActivo = esBFS ? "BFS (Anchura)" : "DFS (Profundidad)";

            recorridoActivo = esBFS ? grafo.RecorridoBFS(codInicio) : grafo.RecorridoDFS(codInicio);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- Recorrido {tipoRecorridoActivo} desde [{codInicio}] ---");
            sb.AppendLine($"Nodos explorados: {recorridoActivo.Count}\n");

            int paso = 1;
            foreach (Parada p in recorridoActivo)
            {
                sb.AppendLine($"{paso}. [{p.Codigo}] {p.Nombre}");
                paso++;
            }

            txtResultadoRecorrido.Text = sb.ToString();
            panelMapa.Invalidate();
        }

        private void BtnAgregarRuta_Click(object? sender, EventArgs e)
        {
            if (grafo == null) return;

            string codO = ExtraerCodigo(cboNuevaOrigen.SelectedItem);
            string codD = ExtraerCodigo(cboNuevaDestino.SelectedItem);

            if (string.IsNullOrEmpty(codO) || string.IsNullOrEmpty(codD))
            {
                MessageBox.Show("Seleccione el origen y destino de la nueva ruta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.Equals(codO, codD, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("El origen y el destino no pueden ser la misma parada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double tiempo = (double)numTiempo.Value;
            double costo = (double)numCosto.Value;
            bool dobleVia = chkDobleVia.Checked;

            try
            {
                if (orquestador != null)
                {
                    string? error = orquestador.Cargador.RegistrarRuta(codO, codD, tiempo, costo, dobleVia);
                    if (error != null)
                    {
                        MessageBox.Show(error, "Aviso al registrar ruta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    grafo.AgregarRuta(codO, codD, tiempo, costo, dobleVia);
                }

                RutaRegistrada?.Invoke(this, EventArgs.Empty);
                MessageBox.Show($"Ruta registrada exitosamente:\n{codO} {(dobleVia ? "<--->" : "--->")} {codD}\nTiempo: {tiempo:F1}m | Costo: Q{costo:F2}",
                                "Ruta Añadida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panelMapa.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar la ruta: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLimpiarMapa_Click(object? sender, EventArgs e)
        {
            rutaActiva = null;
            recorridoActivo = null;
            tipoRecorridoActivo = string.Empty;
            lblEstadoRuta.Text = "Mapa restablecido.";
            lblEstadoRuta.ForeColor = Color.FromArgb(71, 85, 105);
            lblResumenCamino.Text = "Camino: -";
            lblTiempoTotal.Text = "Tiempo: -";
            lblCostoTotal.Text = "Costo: -";
            txtResultadoRecorrido.Text = "Seleccione la parada inicial y ejecute un recorrido.";
            panelMapa.Invalidate();
        }

        private void SeleccionarEnCombo(ComboBox cbo, string codigo)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (ExtraerCodigo(cbo.Items[i]).Equals(codigo, StringComparison.OrdinalIgnoreCase))
                {
                    cbo.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Búsqueda de parada bajo coordenadas en espacio del grafo sin colecciones prohibidas ni LINQ.
        /// </summary>
        private Parada? BuscarParadaPorCoordenadas(float x, float y, float radioTolerancia)
        {
            if (grafo == null) return null;

            float rad2 = radioTolerancia * radioTolerancia;
            foreach (Parada p in grafo.ObtenerTodasLasParadas())
            {
                float dx = (float)p.X - x;
                float dy = (float)p.Y - y;
                if ((dx * dx + dy * dy) <= rad2)
                {
                    return p;
                }
            }
            return null;
        }

        #endregion

        #region Interactividad (Zoom, Pan y Hover)

        /// <summary>
        /// Restablece el zoom y posición de la cámara del mapa a su estado original.
        /// </summary>
        public void CentrarMapa()
        {
            zoom = 1.0f;
            panX = 0f;
            panY = 0f;
            paradaHover = null;
            ActualizarIndicadorZoom();
            panelMapa.Invalidate();
        }

        private void ActualizarIndicadorZoom()
        {
            if (lblZoomIndicador != null)
            {
                lblZoomIndicador.Text = $"{(int)Math.Round(zoom * 100)}%";
            }
        }

        private void AplicarZoomCentrado(float factor)
        {
            float oldZoom = zoom;
            float newZoom = Math.Max(0.4f, Math.Min(3.5f, zoom * factor));
            if (Math.Abs(newZoom - oldZoom) < 0.001f) return;

            float centerX = panelMapa.Width / 2f;
            float centerY = panelMapa.Height / 2f;

            panX = centerX - (centerX - panX) * (newZoom / oldZoom);
            panY = centerY - (centerY - panY) * (newZoom / oldZoom);
            zoom = newZoom;

            ActualizarIndicadorZoom();
            panelMapa.Invalidate();
        }

        private void PanelMapa_MouseWheel(object? sender, MouseEventArgs e)
        {
            float oldZoom = zoom;
            float factor = e.Delta > 0 ? 1.15f : (1f / 1.15f);
            float newZoom = Math.Max(0.4f, Math.Min(3.5f, zoom * factor));
            if (Math.Abs(newZoom - oldZoom) < 0.001f) return;

            // Mantener fijo el punto del mundo bajo el cursor del ratón
            float mouseX = e.X;
            float mouseY = e.Y;

            panX = mouseX - (mouseX - panX) * (newZoom / oldZoom);
            panY = mouseY - (mouseY - panY) * (newZoom / oldZoom);
            zoom = newZoom;

            ActualizarIndicadorZoom();
            panelMapa.Invalidate();
        }

        private void PanelMapa_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                isDragging = true;
                hasMovedSignificantly = false;
                dragStartPoint = e.Location;
                lastMousePos = e.Location;
            }
        }

        private void PanelMapa_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;

                if (Math.Abs(e.X - dragStartPoint.X) > 4 || Math.Abs(e.Y - dragStartPoint.Y) > 4)
                {
                    hasMovedSignificantly = true;
                }

                panX += dx;
                panY += dy;
                lastMousePos = e.Location;
                panelMapa.Cursor = Cursors.SizeAll;
                panelMapa.Invalidate();
            }
            else
            {
                // Detección de Hover en coordenadas de mundo
                float mundoX = (e.X - panX) / zoom;
                float mundoY = (e.Y - panY) / zoom;
                Parada? encontrada = BuscarParadaPorCoordenadas(mundoX, mundoY, 16f + 4f);

                if (encontrada != paradaHover)
                {
                    paradaHover = encontrada;
                    panelMapa.Cursor = paradaHover != null ? Cursors.Hand : Cursors.Default;
                    panelMapa.Invalidate();
                }
            }
        }

        private void PanelMapa_MouseUp(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                panelMapa.Cursor = paradaHover != null ? Cursors.Hand : Cursors.Default;
            }
        }

        private void PanelMapa_MouseLeave(object? sender, EventArgs e)
        {
            isDragging = false;
            if (paradaHover != null)
            {
                paradaHover = null;
                panelMapa.Invalidate();
            }
        }

        private void PanelMapa_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            // Doble clic restablece el zoom y posición original
            CentrarMapa();
        }

        private void PanelMapa_MouseClick(object? sender, MouseEventArgs e)
        {
            // Si el usuario estaba arrastrando para hacer pan, no seleccionar nodo
            if (hasMovedSignificantly)
            {
                return;
            }

            if (grafo == null) return;

            float mundoX = (e.X - panX) / zoom;
            float mundoY = (e.Y - panY) / zoom;

            Parada? seleccionada = BuscarParadaPorCoordenadas(mundoX, mundoY, 20f);
            if (seleccionada == null) return;

            // Clic izquierdo selecciona Origen; clic derecho selecciona Destino
            if (e.Button == MouseButtons.Left)
            {
                SeleccionarEnCombo(cboOrigen, seleccionada.Codigo);
            }
            else if (e.Button == MouseButtons.Right)
            {
                SeleccionarEnCombo(cboDestino, seleccionada.Codigo);
            }

            panelMapa.Invalidate();
        }

        #endregion

        #region Motor de Renderizado del Mapa Vial (GDI+)

        private void PanelMapa_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 1. Calidad de Renderizado (Anti-Aliasing estricto)
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (grafo == null || grafo.CantidadParadas == 0)
            {
                using Font fontAviso = new Font("Segoe UI", 12F, FontStyle.Regular);
                using Brush brushAviso = new SolidBrush(Color.FromArgb(148, 163, 184));
                g.DrawString("Conecte un Grafo de Transporte con paradas para visualizar el mapa.", fontAviso, brushAviso, 30, 60);
                return;
            }

            string codOrigenSel = ExtraerCodigo(cboOrigen.SelectedItem);
            string codDestinoSel = ExtraerCodigo(cboDestino.SelectedItem);

            // Guardar el estado de pantalla antes de aplicar Zoom y Pan
            GraphicsState estadoMundo = g.Save();

            // Aplicar transformaciones para el grafo
            g.TranslateTransform(panX, panY);
            g.ScaleTransform(zoom, zoom);

            // Fondo con cuadrícula tenue continua adaptada al mundo
            DibujarFondoCuadricula(g);

            // 1. Dibujar todas las aristas normales (calles regulares)
            DibujarAristasRegulares(g);

            // 2. Dibujar aristas resaltadas de la ruta óptima de Dijkstra
            DibujarAristasRutaGanadora(g);

            // 3. Dibujar todos los nodos (paradas de transporte)
            DibujarNodosParadas(g, codOrigenSel, codDestinoSel);

            // Restaurar estado a coordenadas fijas de pantalla
            g.Restore(estadoMundo);

            // 4. Dibujar leyenda gráfica fija en la esquina inferior derecha (no se mueve con el zoom)
            DibujarLeyenda(g);
        }

        private void DibujarFondoCuadricula(Graphics g)
        {
            float xMin = -panX / zoom;
            float yMin = -panY / zoom;
            float xMax = (panelMapa.Width - panX) / zoom;
            float yMax = (panelMapa.Height - panY) / zoom;

            using Pen penGrid = new Pen(Color.FromArgb(241, 245, 249), 1f);
            int paso = 40;

            int inicioX = (int)Math.Floor(xMin / paso) * paso;
            int finX = (int)Math.Ceiling(xMax / paso) * paso;
            int inicioY = (int)Math.Floor(yMin / paso) * paso;
            int finY = (int)Math.Ceiling(yMax / paso) * paso;

            for (int x = inicioX; x <= finX; x += paso)
            {
                g.DrawLine(penGrid, x, yMin, x, yMax);
            }
            for (int y = inicioY; y <= finY; y += paso)
            {
                g.DrawLine(penGrid, xMin, y, xMax, y);
            }
        }

        private void DibujarAristasRegulares(Graphics g)
        {
            if (grafo == null) return;

            bool hayRuta = rutaActiva != null && rutaActiva.ExisteRuta;
            bool hayRecorrido = recorridoActivo != null && recorridoActivo.Count > 0;
            bool spotlightActivo = hayRuta || hayRecorrido;
            bool mostrarPesos = chkMostrarPesos.Checked;
            bool porTiempo = rbTiempo.Checked;

            using Pen penCalleNormal = new Pen(Color.FromArgb(180, 190, 205), 2f);
            using Pen penCalleDimmed = new Pen(Color.FromArgb(60, 175, 185, 200), 1.2f);
            using Font fontPeso = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            using Brush brushTextoNormal = new SolidBrush(Color.FromArgb(71, 85, 105));
            using Brush brushTextoDimmed = new SolidBrush(Color.FromArgb(90, 140, 150, 165));
            using Brush brushFondoNormal = new SolidBrush(Color.FromArgb(235, 255, 255, 255));
            using Brush brushFondoDimmed = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            using Pen penBordeNormal = new Pen(Color.FromArgb(226, 232, 240), 0.8f);

            foreach (AristaRuta ruta in grafo.ObtenerTodasLasRutas())
            {
                // 1. Si la arista en este sentido forma parte de la ruta ganadora de Dijkstra, se dibuja en la capa superior
                if (hayRuta && EsAristaDeRutaGanadora(ruta.Origen.Codigo, ruta.Destino.Codigo))
                    continue;

                // 2. Si es bidireccional y el sentido inverso es la ruta ganadora, tampoco la dibujamos como regular gris
                if (hayRuta && ruta.EsBidireccional && EsAristaDeRutaGanadora(ruta.Destino.Codigo, ruta.Origen.Codigo))
                    continue;

                // 3. Para calles bidireccionales regulares, evitar dibujar la arista dos veces (evita saturación y parpadeo)
                if (ruta.EsBidireccional && string.Compare(ruta.Origen.Codigo, ruta.Destino.Codigo, StringComparison.Ordinal) > 0)
                    continue;

                float x1 = (float)ruta.Origen.X;
                float y1 = (float)ruta.Origen.Y;
                float x2 = (float)ruta.Destino.X;
                float y2 = (float)ruta.Destino.Y;

                // Determinar si esta arista pertenece al recorrido activo BFS/DFS
                bool perteneceResultado = hayRecorrido &&
                                          EsParadaEnRecorridoActivo(ruta.Origen.Codigo) &&
                                          EsParadaEnRecorridoActivo(ruta.Destino.Codigo);

                bool atenuar = spotlightActivo && !perteneceResultado;

                // Dibujar línea de la calle
                Pen penUsar = atenuar ? penCalleDimmed : penCalleNormal;
                g.DrawLine(penUsar, x1, y1, x2, y2);

                // Flecha direccional si es de un solo sentido
                if (!ruta.EsBidireccional)
                {
                    Color colorFlecha = atenuar ? Color.FromArgb(60, 175, 185, 200) : Color.FromArgb(148, 163, 184);
                    DibujarFlechaDireccion(g, x1, y1, x2, y2, colorFlecha, atenuar ? 1.5f : 2f);
                }

                // Si el usuario tiene activo "Mostrar pesos en vías"
                if (mostrarPesos)
                {
                    float mx = (x1 + x2) / 2f;
                    float my = (y1 + y2) / 2f;

                    // Descongestión: Muestra ÚNICAMENTE el peso del criterio activo según el RadioButton seleccionado
                    string etiqueta = porTiempo ? $"{ruta.Tiempo:0.#}m" : $"Q{ruta.Costo:0.00}";
                    SizeF size = g.MeasureString(etiqueta, fontPeso);

                    RectangleF rectPill = new RectangleF(mx - (size.Width / 2f) - 3, my - (size.Height / 2f) - 1.5f, size.Width + 6, size.Height + 3);

                    Brush brushFondo = atenuar ? brushFondoDimmed : brushFondoNormal;
                    Brush brushTexto = atenuar ? brushTextoDimmed : brushTextoNormal;

                    g.FillRoundedRectangle(brushFondo, rectPill, 3);
                    if (!atenuar)
                    {
                        g.DrawRoundedRectangle(penBordeNormal, rectPill, 3);
                    }
                    g.DrawString(etiqueta, fontPeso, brushTexto, rectPill.X + 3, rectPill.Y + 1.5f);
                }
            }
        }

        private void DibujarAristasRutaGanadora(Graphics g)
        {
            if (grafo == null || rutaActiva == null || !rutaActiva.ExisteRuta)
                return;

            // Halo sutil exterior para destacar el camino de inmediato (grosor 8f, verde/menta translúcido)
            using Pen penHalo = new Pen(Color.FromArgb(75, 74, 222, 128), 8f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            // Arista ganadora con 100% opacidad y grosor 3.5f según especificación estricta
            using Pen penGanadora = new Pen(Color.FromArgb(22, 163, 74), 3.5f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            using Font fontGanadora = new Font("Segoe UI", 8F, FontStyle.Bold);
            using Brush brushTextoG = new SolidBrush(Color.FromArgb(20, 83, 45));
            using Brush brushFondoG = new SolidBrush(Color.FromArgb(245, 255, 255, 255));
            using Pen penBordeG = new Pen(Color.FromArgb(34, 197, 94), 1.5f);

            foreach (AristaRuta ruta in grafo.ObtenerTodasLasRutas())
            {
                if (EsAristaDeRutaGanadora(ruta.Origen.Codigo, ruta.Destino.Codigo))
                {
                    float x1 = (float)ruta.Origen.X;
                    float y1 = (float)ruta.Origen.Y;
                    float x2 = (float)ruta.Destino.X;
                    float y2 = (float)ruta.Destino.Y;

                    // 1. Halo sutil
                    g.DrawLine(penHalo, x1, y1, x2, y2);

                    // 2. Línea principal de 3.5f de grosor
                    g.DrawLine(penGanadora, x1, y1, x2, y2);

                    // 3. Flecha direccional en el sentido de la ruta ganadora
                    DibujarFlechaDireccion(g, x1, y1, x2, y2, Color.FromArgb(21, 128, 61), grosor: 2.5f);

                    // 4. Etiqueta destacada en la ruta óptima con borde verde resaltado
                    float mx = (x1 + x2) / 2f;
                    float my = (y1 + y2) / 2f;
                    string etiqueta = $"{ruta.Tiempo:0.#} min | Q{ruta.Costo:0.00}";
                    SizeF size = g.MeasureString(etiqueta, fontGanadora);

                    RectangleF rectPill = new RectangleF(mx - (size.Width / 2f) - 4, my - (size.Height / 2f) - 2.5f, size.Width + 8, size.Height + 5);
                    g.FillRoundedRectangle(brushFondoG, rectPill, 4);
                    g.DrawRoundedRectangle(penBordeG, rectPill, 4);
                    g.DrawString(etiqueta, fontGanadora, brushTextoG, rectPill.X + 4, rectPill.Y + 2.5f);
                }
            }
        }

        private void DibujarNodosParadas(Graphics g, string codOrigenSel, string codDestinoSel)
        {
            if (grafo == null) return;

            bool hayRuta = rutaActiva != null && rutaActiva.ExisteRuta;
            bool hayRecorrido = recorridoActivo != null && recorridoActivo.Count > 0;
            bool spotlightActivo = hayRuta || hayRecorrido;

            // Radio de los círculos reducido a 16px según especificación
            float radioNormal = 16f;
            using Font fontCodigo = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            using Font fontNombre = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            using Brush brushTextoBlanco = new SolidBrush(Color.White);
            using Brush brushTextoDimmed = new SolidBrush(Color.FromArgb(170, 255, 255, 255));
            using Brush brushNombreNormal = new SolidBrush(Color.FromArgb(15, 23, 42));
            using Brush brushNombreDimmed = new SolidBrush(Color.FromArgb(90, 100, 116, 139));
            using Brush brushFondoPill = new SolidBrush(Color.FromArgb(230, 255, 255, 255)); // Semitransparente según regla
            using Brush brushFondoPillDimmed = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            using Pen penBordePill = new Pen(Color.FromArgb(203, 213, 225), 0.8f);
            using Pen penBordePillDimmed = new Pen(Color.FromArgb(60, 203, 213, 225), 0.8f);
            using Brush brushSombraPill = new SolidBrush(Color.FromArgb(25, 0, 0, 0));

            foreach (Parada p in grafo.ObtenerTodasLasParadas())
            {
                float cx = (float)p.X;
                float cy = (float)p.Y;

                bool esOrigen = string.Equals(p.Codigo, codOrigenSel, StringComparison.OrdinalIgnoreCase);
                bool esDestino = string.Equals(p.Codigo, codDestinoSel, StringComparison.OrdinalIgnoreCase);
                bool enRutaGanadora = EsParadaEnRutaGanadora(p.Codigo);
                bool enRecorrido = EsParadaEnRecorridoActivo(p.Codigo);
                bool esHover = paradaHover != null && string.Equals(p.Codigo, paradaHover.Codigo, StringComparison.OrdinalIgnoreCase);

                bool perteneceResultado = enRutaGanadora || enRecorrido || esOrigen || esDestino;
                bool atenuar = spotlightActivo && !perteneceResultado;

                float radio = esHover ? radioNormal + 2f : radioNormal;
                RectangleF rectNodo = new RectangleF(cx - radio, cy - radio, radio * 2, radio * 2);

                Color colorNodo;
                Color colorBorde;

                if (atenuar)
                {
                    // Spotlight: Baja opacidad para nodos fuera del camino
                    colorNodo = Color.FromArgb(70, 148, 163, 184);
                    colorBorde = Color.FromArgb(80, 100, 116, 139);
                }
                else if (esOrigen)
                {
                    // Nodo Origen (Azul)
                    colorNodo = Color.FromArgb(37, 99, 235);
                    colorBorde = Color.FromArgb(30, 58, 138);
                }
                else if (esDestino)
                {
                    // Nodo Destino (Rojo)
                    colorNodo = Color.FromArgb(220, 38, 38);
                    colorBorde = Color.FromArgb(153, 27, 27);
                }
                else if (enRutaGanadora)
                {
                    // Nodo en ruta ganadora de Dijkstra (Verde)
                    colorNodo = Color.FromArgb(22, 163, 74);
                    colorBorde = Color.FromArgb(20, 83, 45);
                }
                else if (enRecorrido)
                {
                    // Nodo en recorrido BFS/DFS (Ámbar)
                    colorNodo = Color.FromArgb(217, 119, 6);
                    colorBorde = Color.FromArgb(146, 64, 14);
                }
                else
                {
                    // Nodo estándar (Gris Pizarra)
                    colorNodo = Color.FromArgb(71, 85, 105);
                    colorBorde = Color.FromArgb(30, 41, 59);
                }

                // Halo sutil en hover
                if (esHover)
                {
                    using Pen penHover = new Pen(Color.FromArgb(180, 59, 130, 246), 2.5f);
                    g.DrawEllipse(penHover, cx - radio - 4, cy - radio - 4, (radio + 4) * 2, (radio + 4) * 2);
                }

                // Sombra suave del nodo
                if (!atenuar)
                {
                    using Brush brushSombra = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
                    g.FillEllipse(brushSombra, cx - radio + 1.5f, cy - radio + 1.5f, radio * 2, radio * 2);
                }

                // Relleno y borde del círculo del nodo
                using (Brush brushNodo = new SolidBrush(colorNodo))
                using (Pen penBorde = new Pen(colorBorde, atenuar ? 1.5f : 2.5f))
                {
                    g.FillEllipse(brushNodo, rectNodo);
                    g.DrawEllipse(penBorde, rectNodo);
                }

                // Código en el centro del nodo
                SizeF sizeCod = g.MeasureString(p.Codigo, fontCodigo);
                Brush brushTextoC = atenuar ? brushTextoDimmed : brushTextoBlanco;
                g.DrawString(p.Codigo, fontCodigo, brushTextoC, cx - (sizeCod.Width / 2f), cy - (sizeCod.Height / 2f));

                // Píldora compacta con sombra suave semitransparente debajo del nodo para el nombre
                SizeF sizeNom = g.MeasureString(p.Nombre, fontNombre);
                float pillY = cy + radio + 3.5f;
                RectangleF rectNom = new RectangleF(cx - (sizeNom.Width / 2f) - 4, pillY, sizeNom.Width + 8, sizeNom.Height + 2.5f);

                if (!atenuar)
                {
                    g.FillRoundedRectangle(brushSombraPill, new RectangleF(rectNom.X + 1, rectNom.Y + 1, rectNom.Width, rectNom.Height), 3);
                }

                Brush brushPill = atenuar ? brushFondoPillDimmed : brushFondoPill;
                Pen penPill = atenuar ? penBordePillDimmed : penBordePill;
                Brush brushTextoN = atenuar ? brushNombreDimmed : brushNombreNormal;

                g.FillRoundedRectangle(brushPill, rectNom, 3);
                g.DrawRoundedRectangle(penPill, rectNom, 3);
                g.DrawString(p.Nombre, fontNombre, brushTextoN, rectNom.X + 4, rectNom.Y + 1f);
            }
        }

        private void DibujarFlechaDireccion(Graphics g, float x1, float y1, float x2, float y2, Color colorFlecha, float grosor = 2f)
        {
            // Calcula punto medio avanzado hacia el destino (65% del trayecto para no tapar los nombres)
            float t = 0.65f;
            float px = x1 + (x2 - x1) * t;
            float py = y1 + (y2 - y1) * t;

            float angulo = (float)Math.Atan2(y2 - y1, x2 - x1);
            float tamaño = 8.5f;

            PointF pCentro = new PointF(px, py);
            PointF pIzq = new PointF(px - tamaño * (float)Math.Cos(angulo - Math.PI / 6),
                                     py - tamaño * (float)Math.Sin(angulo - Math.PI / 6));
            PointF pDer = new PointF(px - tamaño * (float)Math.Cos(angulo + Math.PI / 6),
                                     py - tamaño * (float)Math.Sin(angulo + Math.PI / 6));

            using Brush brushF = new SolidBrush(colorFlecha);
            using Pen penF = new Pen(colorFlecha, grosor);

            // Cabeza de flecha sólida y nítida
            PointF[] puntosFlecha = new PointF[] { pCentro, pIzq, pDer };
            g.FillPolygon(brushF, puntosFlecha);
            g.DrawPolygon(penF, puntosFlecha);
        }

        private void DibujarLeyenda(Graphics g)
        {
            int anchoLienzo = panelMapa.Width;
            int altoLienzo = panelMapa.Height;

            int anchoLeyenda = 200;
            int altoLeyenda = 135;
            int x = anchoLienzo - anchoLeyenda - 15;
            int y = altoLienzo - altoLeyenda - 15;

            if (x < 10 || y < 10) return;

            Rectangle rectBox = new Rectangle(x, y, anchoLeyenda, altoLeyenda);
            using Brush brushFondo = new SolidBrush(Color.FromArgb(240, 255, 255, 255));
            using Pen penBorde = new Pen(Color.FromArgb(203, 213, 225), 1.5f);
            using Font fontT = new Font("Segoe UI", 8F, FontStyle.Bold);
            using Font fontItem = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            using Brush brushT = new SolidBrush(Color.FromArgb(15, 23, 42));

            g.FillRoundedRectangle(brushFondo, rectBox, 6);
            g.DrawRoundedRectangle(penBorde, rectBox, 6);

            g.DrawString("Leyenda del Mapa", fontT, brushT, x + 8, y + 6);

            DibujarItemLeyenda(g, x + 8, y + 25, Color.FromArgb(37, 99, 235), "Parada Origen", fontItem, brushT);
            DibujarItemLeyenda(g, x + 8, y + 45, Color.FromArgb(220, 38, 38), "Parada Destino", fontItem, brushT);
            DibujarItemLeyenda(g, x + 8, y + 65, Color.FromArgb(22, 163, 74), "Ruta Óptima (Dijkstra)", fontItem, brushT);
            DibujarItemLeyenda(g, x + 8, y + 85, Color.FromArgb(217, 119, 6), "Visitada en Recorrido", fontItem, brushT);
            DibujarItemLeyenda(g, x + 8, y + 105, Color.FromArgb(71, 85, 105), "Parada Estándar", fontItem, brushT);
        }

        private void DibujarItemLeyenda(Graphics g, int x, int y, Color color, string texto, Font font, Brush brushTexto)
        {
            using Brush brushC = new SolidBrush(color);
            g.FillEllipse(brushC, x, y, 12, 12);
            using Pen penB = new Pen(Color.White, 1f);
            g.DrawEllipse(penB, x, y, 12, 12);
            g.DrawString(texto, font, brushTexto, x + 18, y);
        }

        #endregion

        #region Verificaciones Propias (Sin LINQ ni Colecciones Prohibidas)

        /// <summary>
        /// Comprueba si la parada forma parte de la ruta ganadora de Dijkstra
        /// iterando sobre ListaPropia de códigos (sin LINQ ni HashSet).
        /// </summary>
        private bool EsParadaEnRutaGanadora(string codigo)
        {
            if (rutaActiva == null || !rutaActiva.ExisteRuta || rutaActiva.SecuenciaCodigosParadas == null)
                return false;

            foreach (string c in rutaActiva.SecuenciaCodigosParadas)
            {
                if (string.Equals(c, codigo, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Comprueba si la parada forma parte del recorrido activo BFS o DFS.
        /// </summary>
        private bool EsParadaEnRecorridoActivo(string codigo)
        {
            if (recorridoActivo == null)
                return false;

            foreach (Parada p in recorridoActivo)
            {
                if (string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Comprueba si la arista U -> V es un tramo consecutivo en la secuencia ganadora de Dijkstra.
        /// </summary>
        private bool EsAristaDeRutaGanadora(string codOrigen, string codDestino)
        {
            if (rutaActiva == null || !rutaActiva.ExisteRuta || rutaActiva.SecuenciaCodigosParadas == null)
                return false;

            string? anterior = null;
            foreach (string actual in rutaActiva.SecuenciaCodigosParadas)
            {
                if (anterior != null)
                {
                    if (string.Equals(anterior, codOrigen, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(actual, codDestino, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                anterior = actual;
            }
            return false;
        }

        #endregion
    }

    #region Métodos de Extensión Gráfica (Bordes Redondeados)

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF bounds, float radius)
        {
            using GraphicsPath path = CrearRutaRedondeada(bounds, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF bounds, float radius)
        {
            using GraphicsPath path = CrearRutaRedondeada(bounds, radius);
            g.DrawPath(pen, path);
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, float radius)
        {
            FillRoundedRectangle(g, brush, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), radius);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, float radius)
        {
            DrawRoundedRectangle(g, pen, new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), radius);
        }

        private static GraphicsPath CrearRutaRedondeada(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2f;

            if (d > bounds.Width) d = bounds.Width;
            if (d > bounds.Height) d = bounds.Height;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    #endregion
}

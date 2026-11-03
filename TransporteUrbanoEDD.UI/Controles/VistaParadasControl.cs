// ============================================================
// I1
// Pestaña 1: gestión de paradas (CRUD), carga CSV, estado de
// los buckets de la Tabla Hash y verificador de zonas (Union-Find).
// ============================================================

using System.Text;
using TransporteUrbanoEDD.Core.Archivos;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.UI.Controles
{
    /// <summary>
    /// Pestaña 1. No crea estructuras: las recibe en ConectarDatos para compartirlas con las demás pestañas.
    /// </summary>
    public class VistaParadasControl : UserControl
    {
        // Paleta (la misma de la Pestaña 3 para que se vean parejas)
        private static readonly Color ColorFondo = Color.FromArgb(248, 250, 252);
        private static readonly Color ColorEncabezado = Color.FromArgb(30, 41, 59);
        private static readonly Color ColorTexto = Color.FromArgb(15, 23, 42);
        private static readonly Color ColorColision = Color.FromArgb(254, 243, 199); // Amarillo suave

        // Estructuras del Core (las crea el FormPrincipal, una sola vez)
        private TablaHashParadas? tablaHash;
        private UnionFindZonas? unionFind;
        private CargadorCSV? cargador;

        // Panel izquierdo: secciones apiladas de arriba hacia abajo
        private FlowLayoutPanel panelLateral = null!;

        // Sección CRUD
        private TextBox txtCodigo = null!;
        private TextBox txtNombre = null!;
        private TextBox txtZona = null!;
        private NumericUpDown numX = null!;
        private NumericUpDown numY = null!;
        private Label lblMensajeCrud = null!;

        // Sección archivos CSV
        private Label lblResumenCarga = null!;
        private ListBox lstErroresCarga = null!;

        // Sección conectividad (Union-Find)
        private ComboBox cboZonaOrigen = null!;
        private ComboBox cboZonaDestino = null!;
        private Label lblResultadoZona = null!;
        private Label lblCantidadZonas = null!;

        /// <summary>Se dispara al agregar, eliminar o cargar paradas, para que otras pestañas se refresquen.</summary>
        public event EventHandler? DatosModificados;

        // Panel derecho: estado de la Tabla Hash
        private Label lblEstadoTabla = null!;
        private DataGridView gridBuckets = null!;

        public VistaParadasControl()
        {
            InicializarComponentes();
        }

        // ---------------------------------------------------------
        // CONSTRUCCIÓN DE LA INTERFAZ
        // ---------------------------------------------------------

        private void InicializarComponentes()
        {
            SuspendLayout(); // Pausa el redibujado mientras se arma todo
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9F);
            BackColor = ColorFondo;

            panelLateral = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 400,
                FlowDirection = FlowDirection.TopDown, // Una sección debajo de otra
                WrapContents = false,
                AutoScroll = true,                     // Barra de desplazamiento si no cabe
                Padding = new Padding(12),
                BackColor = ColorFondo
            };
            panelLateral.Controls.Add(CrearEncabezado());
            panelLateral.Controls.Add(CrearSeccionCrud());
            panelLateral.Controls.Add(CrearSeccionArchivos());
            panelLateral.Controls.Add(CrearSeccionConectividad());
            // Orden importante: primero el que llena (Fill), después el acoplado a la izquierda
            Controls.Add(CrearPanelBuckets());
            Controls.Add(panelLateral);

            ResumeLayout(false);
        }

        // Franja oscura con título y subtítulo
        private Panel CrearEncabezado()
        {
            Panel encabezado = new Panel { Width = 360, Height = 60, BackColor = ColorEncabezado };

            encabezado.Controls.Add(new Label
            {
                Text = "Gestión de Paradas y Zonas",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 8),
                AutoSize = true
            });
            encabezado.Controls.Add(new Label
            {
                Text = "Tabla Hash, Union-Find y carga de archivos CSV",
                Font = new Font("Segoe UI", 8.25F),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(10, 34),
                AutoSize = true
            });

            return encabezado;
        }

        // Sección 1: campos de la parada y botones Agregar / Buscar / Eliminar / Limpiar
        private GroupBox CrearSeccionCrud()
        {
            GroupBox gb = CrearSeccion(" 1. Paradas (Agregar / Buscar / Eliminar) ", 250);

            txtCodigo = new TextBox { Location = new Point(80, 28), Width = 120, PlaceholderText = "PAR-21" };
            txtNombre = new TextBox { Location = new Point(80, 60), Width = 264 };
            numX = new NumericUpDown { Location = new Point(80, 92), Width = 90, Maximum = 2000, DecimalPlaces = 1 };
            numY = new NumericUpDown { Location = new Point(215, 92), Width = 90, Maximum = 2000, DecimalPlaces = 1 };
            txtZona = new TextBox { Location = new Point(80, 124), Width = 264, PlaceholderText = "Zona 1" };

            Button btnAgregar = CrearBoton("Agregar", Color.FromArgb(22, 163, 74), 12, 160);
            Button btnBuscar = CrearBoton("Buscar", Color.FromArgb(37, 99, 235), 98, 160);
            Button btnEliminar = CrearBoton("Eliminar", Color.FromArgb(220, 38, 38), 184, 160);
            Button btnLimpiar = CrearBoton("Limpiar", Color.FromArgb(100, 116, 139), 270, 160);
            btnAgregar.Click += BtnAgregar_Click;
            btnBuscar.Click += BtnBuscar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += (s, e) => LimpiarCampos();

            lblMensajeCrud = new Label { Location = new Point(12, 202), Size = new Size(336, 40), Font = new Font("Segoe UI", 8.5F) };

            gb.Controls.AddRange(new Control[]
            {
                CrearEtiqueta("Código:", 12, 28), txtCodigo,
                CrearEtiqueta("Nombre:", 12, 60), txtNombre,
                CrearEtiqueta("X:", 12, 92), numX,
                CrearEtiqueta("Y:", 190, 92), numY,
                CrearEtiqueta("Zona:", 12, 124), txtZona,
                btnAgregar, btnBuscar, btnEliminar, btnLimpiar,
                lblMensajeCrud
            });
            return gb;
        }

        // Sección 2: cargar y guardar paradas.csv y rutas.csv, con la lista de errores
        private GroupBox CrearSeccionArchivos()
        {
            GroupBox gb = CrearSeccion(" 2. Archivos CSV ", 236);

            Button btnCargarParadas = CrearBoton("Cargar paradas", Color.FromArgb(37, 99, 235), 12, 28, 166);
            Button btnCargarRutas = CrearBoton("Cargar rutas", Color.FromArgb(37, 99, 235), 182, 28, 166);
            Button btnGuardarParadas = CrearBoton("Guardar paradas", Color.FromArgb(100, 116, 139), 12, 66, 166);
            Button btnGuardarRutas = CrearBoton("Guardar rutas", Color.FromArgb(100, 116, 139), 182, 66, 166);
            btnCargarParadas.Click += (s, e) => CargarArchivo(esParadas: true);
            btnCargarRutas.Click += (s, e) => CargarArchivo(esParadas: false);
            btnGuardarParadas.Click += (s, e) => GuardarArchivo(esParadas: true);
            btnGuardarRutas.Click += (s, e) => GuardarArchivo(esParadas: false);

            lblResumenCarga = new Label
            {
                Location = new Point(12, 106),
                Size = new Size(336, 20),
                Font = new Font("Segoe UI", 8.5F),
                Text = "Primero las paradas, después las rutas."
            };

            lstErroresCarga = new ListBox
            {
                Location = new Point(12, 130),
                Size = new Size(336, 94),
                Font = new Font("Consolas", 8F),
                HorizontalScrollbar = true // Los mensajes largos se leen desplazando
            };

            gb.Controls.AddRange(new Control[]
            {
                btnCargarParadas, btnCargarRutas, btnGuardarParadas, btnGuardarRutas,
                lblResumenCarga, lstErroresCarga
            });
            return gb;
        }

        // Sección 3: ¿dos paradas están en la misma zona conectada? (Union-Find)
        private GroupBox CrearSeccionConectividad()
        {
            GroupBox gb = CrearSeccion(" 3. Conectividad de Zonas (Union-Find) ", 196);

            cboZonaOrigen = CrearComboParadas(80, 28);
            cboZonaDestino = CrearComboParadas(80, 60);

            Button btnVerificar = CrearBoton("Verificar conectividad", Color.FromArgb(147, 51, 234), 12, 96, 336);
            btnVerificar.Click += BtnVerificar_Click;

            lblResultadoZona = new Label { Location = new Point(12, 136), Size = new Size(336, 20), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            lblCantidadZonas = new Label { Location = new Point(12, 160), Size = new Size(336, 20), Font = new Font("Segoe UI", 8.5F), ForeColor = ColorTexto };

            gb.Controls.AddRange(new Control[]
            {
                CrearEtiqueta("Origen:", 12, 28), cboZonaOrigen,
                CrearEtiqueta("Destino:", 12, 60), cboZonaDestino,
                btnVerificar, lblResultadoZona, lblCantidadZonas
            });
            return gb;
        }

        // ComboBox que guarda objetos Parada y los muestra como "PAR-01 - Parque Central"
        private static ComboBox CrearComboParadas(int x, int y)
        {
            ComboBox combo = new ComboBox
            {
                Location = new Point(x, y),
                Width = 268,
                DropDownStyle = ComboBoxStyle.DropDownList, // Solo elegir, no escribir
                FormattingEnabled = true,                   // Necesario para que funcione el evento Format
                Font = new Font("Segoe UI", 8.5F)
            };

            // Format solo cambia cómo se ve cada elemento; el elemento sigue siendo la Parada
            combo.Format += (s, e) =>
            {
                if (e.ListItem is Parada p)
                    e.Value = $"{p.Codigo} - {p.Nombre}";
            };
            return combo;
        }

        // ---------------------------------------------------------
        // AUXILIARES VISUALES (se reutilizan en todas las secciones)
        // ---------------------------------------------------------

        // Recuadro con título para cada sección del panel izquierdo
        private GroupBox CrearSeccion(string titulo, int alto) => new GroupBox
        {
            Text = titulo,
            Width = 360,
            Height = alto,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = ColorTexto,
            Margin = new Padding(0, 12, 0, 0) // Espacio entre secciones
        };

        private static Label CrearEtiqueta(string texto, int x, int y) => new Label
        {
            Text = texto,
            Location = new Point(x, y + 3), // +3 para alinearla con el TextBox
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F)
        };

        private static Button CrearBoton(string texto, Color color, int x, int y, int ancho = 78) => new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, 32),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 }, // Sin borde, estilo plano
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        // Muestra un mensaje en verde (éxito) o rojo (error)
        private static void MostrarMensaje(Label etiqueta, string texto, bool exito)
        {
            etiqueta.Text = texto;
            etiqueta.ForeColor = exito ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);
        }

        // Lado derecho: título, resumen de la tabla y la grilla de buckets
        private Panel CrearPanelBuckets()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White };

            gridBuckets = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,                 // Solo para ver: se modifica con los botones
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            gridBuckets.Columns.Add("bucket", "Bucket");
            gridBuckets.Columns.Add("cadena", "Cadena (encadenamiento)");
            gridBuckets.Columns.Add("paradas", "Paradas");
            gridBuckets.Columns["bucket"]!.FillWeight = 15;
            gridBuckets.Columns["cadena"]!.FillWeight = 70;
            gridBuckets.Columns["paradas"]!.FillWeight = 15;

            lblEstadoTabla = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = ColorTexto,
                Text = "Sin datos conectados."
            };

            Label lblTitulo = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Estado de la Tabla Hash",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ColorTexto
            };

            // Acoplados arriba: el último agregado queda más arriba
            panel.Controls.Add(gridBuckets);
            panel.Controls.Add(lblEstadoTabla);
            panel.Controls.Add(lblTitulo);
            return panel;
        }

        // ---------------------------------------------------------
        // CONEXIÓN CON LOS DATOS
        // ---------------------------------------------------------

        /// <summary>
        /// Recibe las estructuras compartidas (creadas una sola vez en el FormPrincipal) y refresca la vista.
        /// </summary>
        public void ConectarDatos(TablaHashParadas tablaHash, UnionFindZonas unionFind, CargadorCSV cargador)
        {
            this.tablaHash = tablaHash;
            this.unionFind = unionFind;
            this.cargador = cargador;
            ActualizarVistaBuckets();
        }

        /// <summary>
        /// Redibuja la grilla: una fila por bucket con su cadena. Las colisiones se resaltan en amarillo.
        /// </summary>
        public void ActualizarVistaBuckets()
        {
            if (tablaHash == null)
                return;

            gridBuckets.Rows.Clear();
            int bucketsConColision = 0;

            for (int i = 0; i < tablaHash.Capacidad; i++)
            {
                ListaPropia<Parada> cadena = tablaHash.ObtenerBucket(i);

                // Texto de la cadena: PAR-01 → PAR-14 (o un guion si está vacío)
                StringBuilder texto = new StringBuilder();
                foreach (Parada parada in cadena)
                {
                    if (texto.Length > 0)
                        texto.Append(" → ");
                    texto.Append(parada.Codigo);
                }

                int fila = gridBuckets.Rows.Add(i, cadena.Count == 0 ? "—" : texto.ToString(), cadena.Count);

                if (cadena.Count > 1) // Más de una parada en el bucket = colisión
                {
                    gridBuckets.Rows[fila].DefaultCellStyle.BackColor = ColorColision;
                    bucketsConColision++;
                }
            }

            lblEstadoTabla.Text = $"Capacidad: {tablaHash.Capacidad}   |   Paradas: {tablaHash.Cantidad}   |   " +
                                  $"Factor de carga: {tablaHash.FactorCarga:F2}   |   Rehashes: {tablaHash.Rehashes}   |   " +
                                  $"Buckets con colisión: {bucketsConColision}";

            ActualizarSeccionZonas(); // Los combos y el conteo de zonas cambian con las paradas
        }

        // ---------------------------------------------------------
        // EVENTOS DEL CRUD
        // ---------------------------------------------------------

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (cargador == null || tablaHash == null)
                return;

            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarMensaje(lblMensajeCrud, "El código y el nombre son obligatorios.", false);
                return;
            }

            Parada parada = new Parada(txtCodigo.Text, txtNombre.Text,
                                       (double)numX.Value, (double)numY.Value, txtZona.Text);

            // Registra en Tabla Hash, Grafo, Union-Find y Trie a la vez
            string? error = cargador.RegistrarParada(parada);
            if (error != null)
            {
                MostrarMensaje(lblMensajeCrud, error, false);
                return;
            }

            int bucket = tablaHash.FuncionHash(parada.Codigo); // Después de insertar (por si hubo rehashing)
            ActualizarVistaBuckets();
            SeleccionarBucket(bucket);
            MostrarMensaje(lblMensajeCrud, $"{parada.Codigo} agregada en el bucket {bucket}.", true);
            LimpiarCampos(conservarMensaje: true);
            DatosModificados?.Invoke(this, EventArgs.Empty); // Avisar a las otras pestañas
        }

        private void BtnBuscar_Click(object? sender, EventArgs e)
        {
            if (tablaHash == null)
                return;

            Parada? parada = tablaHash.Buscar(txtCodigo.Text); // O(1) promedio
            if (parada == null)
            {
                MostrarMensaje(lblMensajeCrud, $"No existe la parada '{txtCodigo.Text.Trim()}'.", false);
                return;
            }

            // Mostrar sus datos en los campos
            txtCodigo.Text = parada.Codigo;
            txtNombre.Text = parada.Nombre;
            numX.Value = (decimal)Math.Min(parada.X, (double)numX.Maximum);
            numY.Value = (decimal)Math.Min(parada.Y, (double)numY.Maximum);
            txtZona.Text = parada.Zona;

            int bucket = tablaHash.FuncionHash(parada.Codigo);
            SeleccionarBucket(bucket);
            MostrarMensaje(lblMensajeCrud, $"Encontrada en el bucket {bucket}.", true);
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (cargador == null)
                return;

            string codigo = txtCodigo.Text.Trim().ToUpperInvariant();
            if (codigo.Length == 0)
            {
                MostrarMensaje(lblMensajeCrud, "Escribe el código de la parada a eliminar.", false);
                return;
            }

            DialogResult respuesta = MessageBox.Show($"¿Eliminar la parada {codigo}?", "Confirmar",
                                                     MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (respuesta != DialogResult.Yes)
                return;

            string? error = cargador.EliminarParada(codigo);
            if (error != null)
            {
                MostrarMensaje(lblMensajeCrud, error, false);
                return;
            }

            ActualizarVistaBuckets();
            MostrarMensaje(lblMensajeCrud, $"{codigo} eliminada.", true);
            LimpiarCampos(conservarMensaje: true);
            DatosModificados?.Invoke(this, EventArgs.Empty);
        }

        // Selecciona la fila del bucket y desplaza la tabla hasta ella
        private void SeleccionarBucket(int indice)
        {
            if (indice < 0 || indice >= gridBuckets.Rows.Count)
                return;

            gridBuckets.ClearSelection();
            gridBuckets.Rows[indice].Selected = true;
            gridBuckets.FirstDisplayedScrollingRowIndex = indice;
        }

        private void LimpiarCampos(bool conservarMensaje = false)
        {
            txtCodigo.Clear();
            txtNombre.Clear();
            txtZona.Clear();
            numX.Value = 0;
            numY.Value = 0;
            if (!conservarMensaje)
                lblMensajeCrud.Text = "";
        }

        // ---------------------------------------------------------
        // CONECTIVIDAD (UNION-FIND)
        // ---------------------------------------------------------

        private void BtnVerificar_Click(object? sender, EventArgs e)
        {
            if (unionFind == null)
                return;

            if (cboZonaOrigen.SelectedItem is not Parada origen || cboZonaDestino.SelectedItem is not Parada destino)
            {
                MostrarMensaje(lblResultadoZona, "Elige las dos paradas.", false);
                return;
            }

            // O(α(n)): prácticamente constante
            bool conectadas = unionFind.EstanEnMismaZona(origen.Codigo, destino.Codigo);

            MostrarMensaje(lblResultadoZona,
                           conectadas ? "Misma zona: se puede calcular la ruta."
                                      : "Zonas distintas: no hay conexión entre ellas.",
                           conectadas);
        }

        // Rellena los dos combos (ordenados por código) y el número de zonas
        private void ActualizarSeccionZonas()
        {
            if (tablaHash == null || unionFind == null)
                return;

            Parada[] ordenadas = ObtenerParadasOrdenadas();

            cboZonaOrigen.Items.Clear();
            cboZonaDestino.Items.Clear();
            foreach (Parada p in ordenadas)
            {
                cboZonaOrigen.Items.Add(p);
                cboZonaDestino.Items.Add(p);
            }

            lblCantidadZonas.Text = $"Zonas desconectadas en la red: {unionFind.CantidadZonas}";
        }

        // Copia las paradas a un arreglo y las ordena por código (inserción, sin LINQ). O(n²)
        private Parada[] ObtenerParadasOrdenadas()
        {
            ListaPropia<Parada> todas = tablaHash!.ObtenerTodas();
            Parada[] arreglo = new Parada[todas.Count];

            int n = 0;
            foreach (Parada p in todas)
                arreglo[n++] = p;

            // Ordenamiento por inserción: cada parada se mueve a la izquierda hasta su lugar
            for (int i = 1; i < arreglo.Length; i++)
            {
                Parada actual = arreglo[i];
                int j = i - 1;
                while (j >= 0 && string.CompareOrdinal(arreglo[j].Codigo, actual.Codigo) > 0)
                {
                    arreglo[j + 1] = arreglo[j];
                    j--;
                }
                arreglo[j + 1] = actual;
            }

            return arreglo;
        }

        // ---------------------------------------------------------
        // EVENTOS DE ARCHIVOS CSV
        // ---------------------------------------------------------

        // Abre el selector de archivos, carga el CSV y muestra el resultado con sus errores
        private void CargarArchivo(bool esParadas)
        {
            if (cargador == null)
                return;

            using OpenFileDialog dialogo = new OpenFileDialog
            {
                Title = esParadas ? "Seleccionar archivo de paradas" : "Seleccionar archivo de rutas",
                Filter = "Archivos CSV o TXT|*.csv;*.txt|Todos los archivos|*.*"
            };

            // Abrir en la carpeta Datos si existe (junto al programa)
            string carpetaDatos = Path.Combine(AppContext.BaseDirectory, "Datos");
            if (Directory.Exists(carpetaDatos))
                dialogo.InitialDirectory = carpetaDatos;

            if (dialogo.ShowDialog() != DialogResult.OK)
                return; // El usuario canceló

            ResultadoCarga resultado = esParadas
                ? cargador.CargarParadasCSV(dialogo.FileName)
                : cargador.CargarRutasCSV(dialogo.FileName);

            // Resumen en verde si no hubo errores, en rojo si hubo
            string tipo = esParadas ? "Paradas" : "Rutas";
            MostrarMensaje(lblResumenCarga, $"{tipo}: {resultado}", resultado.Errores.Count == 0);

            // Detalle de cada línea con error
            lstErroresCarga.Items.Clear();
            foreach (string error in resultado.Errores)
                lstErroresCarga.Items.Add(error);

            ActualizarVistaBuckets();
            DatosModificados?.Invoke(this, EventArgs.Empty);
        }

        // Abre el diálogo "Guardar como" y escribe las paradas o rutas actuales
        private void GuardarArchivo(bool esParadas)
        {
            if (cargador == null)
                return;

            using SaveFileDialog dialogo = new SaveFileDialog
            {
                Title = esParadas ? "Guardar paradas" : "Guardar rutas",
                Filter = "Archivo CSV|*.csv",
                FileName = esParadas ? "paradas.csv" : "rutas.csv"
            };

            if (dialogo.ShowDialog() != DialogResult.OK)
                return;

            string? error = esParadas
                ? cargador.GuardarParadasCSV(dialogo.FileName)
                : cargador.GuardarRutasCSV(dialogo.FileName);

            MostrarMensaje(lblResumenCarga,
                           error ?? $"Guardado en {Path.GetFileName(dialogo.FileName)}.",
                           error == null);
        }
    }
}
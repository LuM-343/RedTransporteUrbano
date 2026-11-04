// ============================================================
// I1
// Pestaña 2: buscador por nombre con autocompletado (Trie) y ruta
// entre paradas usando el flujo completo del Orquestador:
// Trie → Tabla Hash → Union-Find → Dijkstra.
// ============================================================

using System.Text;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.UI.Controles
{
    public class VistaBuscadorControl : UserControl
    {
        private OrquestadorTransporte? orquestador;
        private Parada? origen;   // Elegida por nombre
        private Parada? destino;

        // Sección 1: búsqueda
        private TextBox txtBuscar = null!;
        private ListBox lstSugerencias = null!;
        private Label lblCoincidencias = null!;
        private Label lblDetalle = null!;

        // Sección 2: ruta
        private Label lblOrigen = null!;
        private Label lblDestino = null!;
        private RadioButton rbTiempo = null!;
        private Label lblResultado = null!;

        // Panel derecho: paso a paso del flujo
        private TextBox txtFlujo = null!;

        public VistaBuscadorControl()
        {
            InicializarComponentes();
        }

        // ---------------------------------------------------------
        // CONSTRUCCIÓN DE LA INTERFAZ
        // ---------------------------------------------------------

        private void InicializarComponentes()
        {
            SuspendLayout();
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9F);
            BackColor = EstiloUI.Fondo;

            FlowLayoutPanel panelLateral = new FlowLayoutPanel
            {
                Dock = DockStyle.Left, Width = 400, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(12), BackColor = EstiloUI.Fondo
            };
            panelLateral.Controls.Add(EstiloUI.CrearEncabezado("Buscador de Paradas", "Autocompletado con Trie y ruta por nombre"));
            panelLateral.Controls.Add(CrearSeccionBusqueda());
            panelLateral.Controls.Add(CrearSeccionRuta());

            Controls.Add(CrearPanelFlujo()); // Fill primero
            Controls.Add(panelLateral);
            ResumeLayout(false);
        }

        // Sección 1: escribir → sugerencias del Trie → detalle desde la Tabla Hash
        private GroupBox CrearSeccionBusqueda()
        {
            GroupBox gb = EstiloUI.CrearSeccion(" 1. Buscar parada por nombre (Trie) ", 334);

            txtBuscar = new TextBox { Location = new Point(80, 28), Width = 268, PlaceholderText = "Ej. par, ter, ca..." };
            txtBuscar.TextChanged += (s, e) => ActualizarSugerencias(); // Cada letra consulta el Trie

            lblCoincidencias = new Label { Location = new Point(12, 60), Size = new Size(336, 20), Font = new Font("Segoe UI", 8.5F) };

            lstSugerencias = new ListBox { Location = new Point(12, 84), Size = new Size(336, 110), Font = new Font("Segoe UI", 9F) };
            lstSugerencias.SelectedIndexChanged += (s, e) => MostrarDetalle();

            // Botones de origen, destino y eliminar
            Button btnOrigen = EstiloUI.CrearBoton("Usar como origen", EstiloUI.Azul, 12, 204, 166);
            Button btnDestino = EstiloUI.CrearBoton("Usar como destino", EstiloUI.Rojo, 182, 204, 166);
            Button btnEliminar = EstiloUI.CrearBoton("Eliminar del Trie", Color.DarkOrange, 12, 240, 336);

            btnOrigen.Click += (s, e) => ElegirParada(esOrigen: true);
            btnDestino.Click += (s, e) => ElegirParada(esOrigen: false);
            btnEliminar.Click += (s, e) => EliminarParadaDelTrie();

            lblDetalle = new Label { Location = new Point(12, 280), Size = new Size(336, 44), Font = new Font("Segoe UI", 8.5F) };

            gb.Controls.AddRange(new Control[]
            {
                EstiloUI.CrearEtiqueta("Escribe:", 12, 28), txtBuscar,
                lblCoincidencias, lstSugerencias, btnOrigen, btnDestino, btnEliminar, lblDetalle
            });
            return gb;
        }

        private void EliminarParadaDelTrie()
        {
            if (orquestador == null) return;

            Parada? p = ParadaSeleccionada();
            if (p == null)
            {
                EstiloUI.MostrarMensaje(lblDetalle, "Primero elige una parada de la lista para eliminar.", false);
                return;
            }

            // Elimina la parada específicamente del Trie (basado en el nombre)
            orquestador.Trie.Eliminar(p.Nombre);
            
            // Actualiza visualmente el resultado
            EstiloUI.MostrarMensaje(lblDetalle, $"'{p.Nombre}' eliminada del Trie.", true);
            
            // Refresca la lista de sugerencias para que desaparezca
            ActualizarSugerencias(); 
        }
        // Sección 2: origen y destino elegidos, criterio y botón calcular
        private GroupBox CrearSeccionRuta()
        {
            GroupBox gb = EstiloUI.CrearSeccion(" 2. Ruta entre paradas (flujo completo) ", 204);

            lblOrigen = new Label { Location = new Point(12, 28), Size = new Size(336, 20), Font = new Font("Segoe UI", 8.5F), Text = "Origen: —" };
            lblDestino = new Label { Location = new Point(12, 52), Size = new Size(336, 20), Font = new Font("Segoe UI", 8.5F), Text = "Destino: —" };

            rbTiempo = new RadioButton { Location = new Point(12, 80), Width = 160, Text = "Menor tiempo", Checked = true, Font = new Font("Segoe UI", 8.5F) };
            RadioButton rbCosto = new RadioButton { Location = new Point(182, 80), Width = 160, Text = "Menor costo", Font = new Font("Segoe UI", 8.5F) };

            Button btnCalcular = EstiloUI.CrearBoton("Calcular ruta", EstiloUI.Verde, 12, 112, 336);
            btnCalcular.Click += (s, e) => CalcularRuta();

            lblResultado = new Label { Location = new Point(12, 152), Size = new Size(336, 42), Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };

            gb.Controls.AddRange(new Control[] { lblOrigen, lblDestino, rbTiempo, rbCosto, btnCalcular, lblResultado });
            return gb;
        }

        // Panel derecho: registro paso a paso de qué hizo cada estructura
        private Panel CrearPanelFlujo()
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White };

            txtFlujo = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F), BackColor = EstiloUI.Fondo, BorderStyle = BorderStyle.None,
                Text = "Elige un origen y un destino por nombre y presiona \"Calcular ruta\"."
            };

            Label lblSub = new Label { Dock = DockStyle.Top, Height = 28, ForeColor = EstiloUI.Texto, Text = "Trie  →  Tabla Hash  →  Union-Find  →  Dijkstra" };
            Label lblTitulo = new Label { Dock = DockStyle.Top, Height = 30, Text = "Flujo integrado de las 4 estructuras", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = EstiloUI.Texto };

            panel.Controls.Add(txtFlujo);
            panel.Controls.Add(lblSub);
            panel.Controls.Add(lblTitulo); // El último Top queda más arriba
            return panel;
        }

        // ---------------------------------------------------------
        // CONEXIÓN CON LOS DATOS
        // ---------------------------------------------------------

        /// <summary>Recibe el orquestador compartido por todas las pestañas.</summary>
        public void ConectarDatos(OrquestadorTransporte orquestador)
        {
            this.orquestador = orquestador;
            Refrescar();
        }

        /// <summary>Vuelve a consultar el Trie (llamar cuando cambian las paradas).</summary>
        public void Refrescar() => ActualizarSugerencias();

        // ---------------------------------------------------------
        // BÚSQUEDA (TRIE → TABLA HASH)
        // ---------------------------------------------------------

        // Se ejecuta con cada letra: pide al Trie los nombres que empiezan con el texto
        private void ActualizarSugerencias()
        {
            lstSugerencias.Items.Clear();
            string prefijo = txtBuscar.Text.Trim();

            if (orquestador == null || prefijo.Length == 0)
            {
                lblCoincidencias.Text = "";
                return;
            }

            ListaPropia<string> sugerencias = orquestador.Trie.Autocompletar(prefijo); // O(largo del prefijo + resultados)
            foreach (string s in sugerencias)
                lstSugerencias.Items.Add(s);

            lblCoincidencias.Text = $"{sugerencias.Count} coincidencia(s) para \"{prefijo}\"";
        }

        // Toma la sugerencia elegida ("Parque Central (PAR-01)") y busca la parada completa en la Tabla Hash
        private Parada? ParadaSeleccionada()
        {
            if (orquestador == null || lstSugerencias.SelectedItem is not string texto)
                return null;

            // El código está entre el último "(" y el último ")"
            int abre = texto.LastIndexOf('(');
            int cierra = texto.LastIndexOf(')');
            if (abre < 0 || cierra <= abre)
                return null;

            string codigo = texto.Substring(abre + 1, cierra - abre - 1);
            return orquestador.TablaHash.Buscar(codigo); // Trie dio el código → Hash da la parada en O(1)
        }

        private void MostrarDetalle()
        {
            Parada? p = ParadaSeleccionada();
            if (p == null)
            {
                lblDetalle.Text = lstSugerencias.SelectedItem == null ? "" : "Esa parada ya no existe en la Tabla Hash.";
                return;
            }

            int bucket = orquestador!.TablaHash.FuncionHash(p.Codigo);
            lblDetalle.ForeColor = EstiloUI.Texto;
            lblDetalle.Text = $"{p.Codigo} · {p.Nombre}\n{(p.Zona.Length > 0 ? p.Zona + " · " : "")}({p.X}, {p.Y}) · bucket {bucket}";
        }

        private void ElegirParada(bool esOrigen)
        {
            Parada? p = ParadaSeleccionada();
            if (p == null)
            {
                EstiloUI.MostrarMensaje(lblDetalle, "Primero elige una parada de la lista.", false);
                return;
            }

            if (esOrigen)
            {
                origen = p;
                lblOrigen.Text = $"Origen: {p.Codigo} - {p.Nombre}";
            }
            else
            {
                destino = p;
                lblDestino.Text = $"Destino: {p.Codigo} - {p.Nombre}";
            }
        }

        // ---------------------------------------------------------
        // RUTA (UNION-FIND → DIJKSTRA, a través del Orquestador)
        // ---------------------------------------------------------

        private void CalcularRuta()
        {
            if (orquestador == null)
                return;

            if (origen == null || destino == null)
            {
                EstiloUI.MostrarMensaje(lblResultado, "Elige el origen y el destino.", false);
                return;
            }

            bool porTiempo = rbTiempo.Checked;
            StringBuilder flujo = new StringBuilder();

            // Paso 1 y 2: ya ocurrieron al elegir por nombre
            flujo.AppendLine($"1. TRIE        Se eligieron por nombre: \"{origen.Nombre}\" y \"{destino.Nombre}\".");
            flujo.AppendLine($"2. TABLA HASH  {origen.Codigo} (bucket {orquestador.TablaHash.FuncionHash(origen.Codigo)}) y " +
                             $"{destino.Codigo} (bucket {orquestador.TablaHash.FuncionHash(destino.Codigo)}) encontrados en O(1).");

            // Paso 3: zona (solo para mostrarlo; el Orquestador también lo valida)
            bool mismaZona = orquestador.UnionFind.EstanEnMismaZona(origen.Codigo, destino.Codigo);
            flujo.AppendLine(mismaZona
                ? "3. UNION-FIND  Misma zona conectada: se puede ejecutar Dijkstra."
                : "3. UNION-FIND  Zonas distintas: no se ejecuta Dijkstra.");

            // Paso 4: el Orquestador hace las validaciones y, si pasan, corre Dijkstra
            ResultadoRuta r = orquestador.BuscarRutaOptima(origen.Codigo, destino.Codigo, porTiempo);

            if (r.ExisteRuta)
            {
                flujo.AppendLine($"4. DIJKSTRA    Optimizando por {(porTiempo ? "tiempo" : "costo")}:");
                flujo.AppendLine($"               {r.ResumenCamino}");
                flujo.AppendLine($"               Tiempo: {r.TiempoTotalMinutos:F1} min | Costo: Q{r.CostoTotal:F2}");
                EstiloUI.MostrarMensaje(lblResultado, $"{r.TiempoTotalMinutos:F1} min | Q{r.CostoTotal:F2}", true);
            }
            else
            {
                flujo.AppendLine($"4. RESULTADO   {r.MensajeEstado}");
                EstiloUI.MostrarMensaje(lblResultado, r.MensajeEstado, false);
            }

            txtFlujo.Text = flujo.ToString();
        }
    }
}
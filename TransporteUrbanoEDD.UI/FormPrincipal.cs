// ============================================================
// I1
// Ventana principal: crea el orquestador (dueño de las 4
// estructuras) y aloja las 3 pestañas sobre los mismos datos.
// ============================================================

using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.UI.Controles;

namespace TransporteUrbanoEDD.UI
{
    public class FormPrincipal : Form
    {
        private readonly OrquestadorTransporte orquestador = new OrquestadorTransporte();

        private VistaParadasControl vistaParadas = null!; // Pestaña 1 (I1)
        private VistaBuscadorControl vistaBuscador = null!; // Pestaña 2
        private VistaRutasGrafoControl vistaRutas = null!; // Pestaña 3 (I3)

        private ToolStripStatusLabel lblEstado = null!; // Texto de la barra inferior

        public FormPrincipal()
        {
            ConfigurarVentana();
            CargarDatasetInicial();
            ConstruirPestanas();
            CrearBarraEstado();   // Después de las pestañas para que quede abajo
            ConectarPestanas();
        }

        private void ConfigurarVentana()
        {
            Text = "Red de Transporte Urbano - Estructura de Datos II";
            Width = 1320;
            Height = 860;
            MinimumSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
        }

        // Si existe la carpeta Datos junto al .exe, carga el dataset de prueba al iniciar
        private void CargarDatasetInicial()
        {
            string carpeta = Path.Combine(AppContext.BaseDirectory, "Datos");
            string paradas = Path.Combine(carpeta, "paradas.csv");
            string rutas = Path.Combine(carpeta, "rutas.csv");

            if (File.Exists(paradas) && File.Exists(rutas))
            {
                orquestador.Cargador.CargarParadasCSV(paradas); // Primero paradas: las rutas las necesitan
                orquestador.Cargador.CargarRutasCSV(rutas);
            }
        }

        // TabControl con una pestaña por integrante
        private void ConstruirPestanas()
        {
            TabControl pestanas = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(16, 6) // Pestañas más anchas y fáciles de clicar
            };

            // Pestaña 1: gestión de paradas y zonas (I1)
            vistaParadas = new VistaParadasControl { Dock = DockStyle.Fill };
            TabPage tab1 = new TabPage("1. Paradas y Zonas") { BackColor = EstiloUI.Fondo };
            tab1.Controls.Add(vistaParadas);

            // Pestaña 2: buscador con autocompletado y ruta por nombre
            vistaBuscador = new VistaBuscadorControl { Dock = DockStyle.Fill };
            TabPage tab2 = new TabPage("2. Buscador") { BackColor = EstiloUI.Fondo };
            tab2.Controls.Add(vistaBuscador);

            // Pestaña 3: rutas, recorridos y mapa (I3)
            vistaRutas = new VistaRutasGrafoControl { Dock = DockStyle.Fill };
            TabPage tab3 = new TabPage("3. Rutas y Mapa") { BackColor = EstiloUI.Fondo };
            tab3.Controls.Add(vistaRutas);

            pestanas.TabPages.Add(tab1);
            pestanas.TabPages.Add(tab2);
            pestanas.TabPages.Add(tab3);
            Controls.Add(pestanas);
        }

        // Barra inferior con el estado general del sistema
        private void CrearBarraEstado()
        {
            StatusStrip barra = new StatusStrip { BackColor = EstiloUI.Encabezado, SizingGrip = false };
            lblEstado = new ToolStripStatusLabel { ForeColor = Color.White, Font = new Font("Segoe UI", 9F) };
            barra.Items.Add(lblEstado);
            Controls.Add(barra);
        }

        private void ActualizarBarraEstado()
        {
            lblEstado.Text = $"Paradas: {orquestador.TablaHash.Cantidad}     |     " +
                             $"Zonas desconectadas: {orquestador.UnionFind.CantidadZonas}     |     " +
                             $"Factor de carga: {orquestador.TablaHash.FactorCarga:F2}     |     " +
                             $"Aristas en el grafo: {orquestador.Grafo.ObtenerTodasLasRutas().Count}";
        }

        // Todas las pestañas reciben las MISMAS estructuras del orquestador
        private void ConectarPestanas()
        {
            vistaParadas.ConectarDatos(orquestador.TablaHash, orquestador.UnionFind, orquestador.Cargador);
            vistaBuscador.ConectarDatos(orquestador);
            vistaRutas.ConectarDatos(orquestador);

            // Si cambian las paradas en la Pestaña 1, se refrescan el buscador y el mapa
            vistaParadas.DatosModificados += (s, e) =>
            {
                vistaBuscador.Refrescar();
                vistaRutas.ConectarDatos(orquestador);
                ActualizarBarraEstado();
            };

            // Si se registra una ruta en la Pestaña 3, se actualiza la barra de estado y la Pestaña 1
            vistaRutas.RutaRegistrada += (s, e) =>
            {
                vistaParadas.ConectarDatos(orquestador.TablaHash, orquestador.UnionFind, orquestador.Cargador);
                ActualizarBarraEstado();
            };

            ActualizarBarraEstado();
        }
    }
}

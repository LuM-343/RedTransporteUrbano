// I1
// Ventana de prueba de la Pestaña 1 (temporal, mientras se integra el FormPrincipal)

using TransporteUrbanoEDD.Core.Archivos;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.UI.Controles;

namespace TransporteUrbanoEDD.UI
{
    public class FormPruebaDev1 : Form
    {
        public FormPruebaDev1()
        {
            Text = "Red de Transporte Urbano - Gestión de Paradas (Dev 1)";
            Width = 1280;
            Height = 820;
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;

            // Estructuras compartidas (en el FormPrincipal se crean una sola vez, igual que aquí)
            TablaHashParadas tablaHash = new TablaHashParadas();
            GrafoTransporte grafo = new GrafoTransporte();
            UnionFindZonas unionFind = new UnionFindZonas(16, tablaHash);
            TrieParadas trie = new TrieParadas();
            CargadorCSV cargador = new CargadorCSV(tablaHash, grafo, unionFind, trie);

            // Dataset de prueba (copiado junto al .exe al compilar)
            string carpeta = Path.Combine(AppContext.BaseDirectory, "Datos");
            cargador.CargarParadasCSV(Path.Combine(carpeta, "paradas.csv"));
            cargador.CargarRutasCSV(Path.Combine(carpeta, "rutas.csv"));

            VistaParadasControl vista = new VistaParadasControl { Dock = DockStyle.Fill };
            Controls.Add(vista);
            vista.ConectarDatos(tablaHash, unionFind, cargador);
        }
    }
}
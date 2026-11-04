using System;
using System.Drawing;
using System.Windows.Forms;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;
using TransporteUrbanoEDD.UI.Controles;

namespace TransporteUrbanoEDD.UI
{
    /// <summary>
    /// Ventana de prueba y ejecución interactiva para el Día 3 (Dev 3).
    /// Permite probar la calculadora de rutas (Dijkstra), los recorridos BFS/DFS
    /// y el renderizado interactivo del mapa vial antes de integrar con FormPrincipal (Dev 2).
    /// </summary>
    public class FormPruebaDev3 : Form
    {
        private VistaRutasGrafoControl vistaControl = null!;

        public FormPruebaDev3()
        {
            InicializarVentana();
            CargarDatosDePrueba();
        }

        private void InicializarVentana()
        {
            Text = "Red de Transporte Urbano - Calculadora de Rutas y Mapa Vial (Dev 3)";
            Width = 1280;
            Height = 820;
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(248, 250, 252);

            vistaControl = new VistaRutasGrafoControl
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(vistaControl);
        }

        private void CargarDatosDePrueba()
        {
            OrquestadorTransporte orquestador = new OrquestadorTransporte();

            // Paradas con coordenadas distribuidas armónicamente en el lienzo
            Parada p1 = new Parada("PAR-01", "Estación Central El Trébol", 120, 280);
            Parada p2 = new Parada("PAR-02", "Plaza España", 330, 390);
            Parada p3 = new Parada("PAR-03", "Zona Viva / Prisa", 560, 330);
            Parada p4 = new Parada("PAR-04", "Centro Cívico", 240, 140);
            Parada p5 = new Parada("PAR-05", "Paseo de la Sexta", 440, 110);
            Parada p6 = new Parada("PAR-06", "Univ. del Valle / Landívar", 740, 460);
            Parada p7 = new Parada("PAR-07", "Estación Aislada San Cristóbal", 90, 520);

            orquestador.Cargador.RegistrarParada(p1);
            orquestador.Cargador.RegistrarParada(p2);
            orquestador.Cargador.RegistrarParada(p3);
            orquestador.Cargador.RegistrarParada(p4);
            orquestador.Cargador.RegistrarParada(p5);
            orquestador.Cargador.RegistrarParada(p6);
            orquestador.Cargador.RegistrarParada(p7);

            // Rutas viales con doble ponderación (Tiempo en min, Costo en Q)
            // Vía A: Más económica (Q5.50 vs Q7.00), pero más lenta (27 min vs 23 min)
            orquestador.Cargador.RegistrarRuta("PAR-01", "PAR-02", tiempo: 12.0, costo: 2.50, bidireccional: true);
            orquestador.Cargador.RegistrarRuta("PAR-02", "PAR-03", tiempo: 15.0, costo: 3.00, bidireccional: true);

            // Vía B: Más rápida (23 min vs 27 min), pero más cara (Q7.00 vs Q5.50)
            orquestador.Cargador.RegistrarRuta("PAR-01", "PAR-04", tiempo: 8.0, costo: 2.00, bidireccional: false);
            orquestador.Cargador.RegistrarRuta("PAR-04", "PAR-05", tiempo: 5.0, costo: 1.50, bidireccional: false);
            orquestador.Cargador.RegistrarRuta("PAR-05", "PAR-03", tiempo: 10.0, costo: 3.50, bidireccional: false);

            // Conexión hacia la universidad (sentido único)
            orquestador.Cargador.RegistrarRuta("PAR-03", "PAR-06", tiempo: 20.0, costo: 4.00, bidireccional: false);

            // Conectar estructura al control visual
            vistaControl.ConectarDatos(orquestador);
        }
    }
}

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
        }
    }
}
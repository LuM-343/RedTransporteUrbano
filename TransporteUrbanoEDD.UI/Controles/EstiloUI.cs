// ============================================================
// Paleta de colores semántica unificada (estilo Tailwind Slate)
// y componentes comunes para FormPrincipal y Controles de Usuario.
// ============================================================

namespace TransporteUrbanoEDD.UI.Controles
{
    internal static class EstiloUI
    {
        // -------------------------------------------------------------
        // 1. Fondos y Contenedores (Tailwind Slate)
        // -------------------------------------------------------------
        // Fondo de las pestañas (TabPage): Gris muy claro (#F8FAFC -> 248, 250, 252)
        public static readonly Color Fondo = Color.FromArgb(248, 250, 252);

        // Banners de título superior y encabezado del DataGridView: Slate oscuro (#1E293B -> 30, 41, 59)
        public static readonly Color Encabezado = Color.FromArgb(30, 41, 59);

        // Subtítulo de banners y texto secundario (#CBD5E1 -> 203, 213, 225)
        public static readonly Color Subtitulo = Color.FromArgb(203, 213, 225);

        // Texto principal de la interfaz (#1E293B -> 30, 41, 59)
        public static readonly Color Texto = Color.FromArgb(30, 41, 59);

        // Consolas de texto / Cajas de resultados: Fondo #F1F5F9 (241, 245, 249) con texto #1E293B (30, 41, 59)
        public static readonly Color ConsolaFondo = Color.FromArgb(241, 245, 249);
        public static readonly Color ConsolaTexto = Color.FromArgb(30, 41, 59);

        // Bordes y separadores sutiles (#E2E8F0 -> 226, 232, 240)
        public static readonly Color BordeSutil = Color.FromArgb(226, 232, 240);

        // -------------------------------------------------------------
        // 2. Botones por Función Semántica
        // (FlatStyle = FlatStyle.Flat, FlatAppearance.BorderSize = 0, ForeColor = Color.White)
        // -------------------------------------------------------------
        // Acción Primaria / Cálculo / Consulta (Azul #2563EB -> 37, 99, 235)
        public static readonly Color BotonPrimario = Color.FromArgb(37, 99, 235);

        // Acción de Creación / Inserción (Verde Esmeralda #16A34A -> 22, 163, 74)
        public static readonly Color BotonCreacion = Color.FromArgb(22, 163, 74);

        // Acción Destructiva / Borrado (Rojo #DC2626 -> 220, 38, 38)
        public static readonly Color BotonDestructivo = Color.FromArgb(220, 38, 38);

        // Acción Analítica / Secundaria (Índigo #4F46E5 -> 79, 70, 229)
        public static readonly Color BotonSecundario = Color.FromArgb(79, 70, 229);

        // Acción Neutra / Limpieza / Exportación (Gris Pizarra #475569 -> 71, 85, 105)
        public static readonly Color BotonNeutro = Color.FromArgb(71, 85, 105);

        // Alias retrocompatibles
        public static readonly Color Azul = BotonPrimario;
        public static readonly Color Verde = BotonCreacion;
        public static readonly Color Rojo = BotonDestructivo;
        public static readonly Color Indigo = BotonSecundario;
        public static readonly Color Gris = BotonNeutro;

        // -------------------------------------------------------------
        // 3. Colores del Mapa Vial y su Leyenda (Pestaña 3)
        // -------------------------------------------------------------
        // Fondo del lienzo del mapa: Blanco puro (#FFFFFF)
        public static readonly Color MapaFondo = Color.FromArgb(255, 255, 255);

        // Cuadrícula muy sutil (#F1F5F9 -> 241, 245, 249)
        public static readonly Color MapaCuadricula = Color.FromArgb(241, 245, 249);

        // Calles / Aristas normales (#94A3B8 -> 148, 163, 184)
        public static readonly Color MapaAristaNormal = Color.FromArgb(148, 163, 184);

        // Nodo Parada Normal (#334155 -> 51, 65, 85)
        public static readonly Color MapaNodoNormal = Color.FromArgb(51, 65, 85);

        // Nodo Parada Origen (#2563EB -> 37, 99, 235)
        public static readonly Color MapaNodoOrigen = Color.FromArgb(37, 99, 235);

        // Nodo Parada Destino (#E11D48 -> 225, 29, 72)
        public static readonly Color MapaNodoDestino = Color.FromArgb(225, 29, 72);

        // Ruta Óptima (Dijkstra) (#16A34A -> 22, 163, 74)
        public static readonly Color MapaRutaOptima = Color.FromArgb(22, 163, 74);

        // Visitada en Recorrido (BFS/DFS) (#D97706 -> 217, 119, 6)
        public static readonly Color MapaVisitado = Color.FromArgb(217, 119, 6);

        // -------------------------------------------------------------
        // Generadores y Aplicadores de Estilos
        // -------------------------------------------------------------
        // Franja oscura con título y subtítulo
        public static Panel CrearEncabezado(string titulo, string subtitulo, int ancho = 360)
        {
            Panel encabezado = new Panel { Width = ancho, Height = 64, BackColor = Encabezado };
            encabezado.Controls.Add(new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 8),
                AutoSize = true
            });
            encabezado.Controls.Add(new Label
            {
                Text = subtitulo,
                Font = new Font("Segoe UI", 8.25F),
                ForeColor = Subtitulo,
                Location = new Point(10, 36),
                AutoSize = true
            });
            return encabezado;
        }

        // Recuadro con título para cada sección del panel lateral
        public static GroupBox CrearSeccion(string titulo, int alto, int ancho = 360) => new GroupBox
        {
            Text = titulo,
            Width = ancho,
            Height = alto,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Texto,
            Margin = new Padding(0, 12, 0, 0)
        };

        public static Label CrearEtiqueta(string texto, int x, int y) => new Label
        {
            Text = texto,
            Location = new Point(x, y + 3),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F)
        };

        // Generador de botón con estilo plano y función semántica
        public static Button CrearBoton(string texto, Color color, int x, int y, int ancho = 78, int alto = 32) => new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, alto),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        // Aplica el estilo estándar semántico a un botón existente
        public static void AplicarEstiloBoton(Button btn, Color color)
        {
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        // Muestra un mensaje en verde (éxito) o rojo (error)
        public static void MostrarMensaje(Label etiqueta, string texto, bool exito)
        {
            etiqueta.Text = texto;
            etiqueta.ForeColor = exito ? BotonCreacion : BotonDestructivo;
        }
    }
}

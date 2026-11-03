// ============================================================
// I1
// Paleta y controles comunes para que todas las pestañas se vean iguales.
// ============================================================

namespace TransporteUrbanoEDD.UI.Controles
{
    internal static class EstiloUI
    {
        // Paleta (la misma de la Pestaña 3)
        public static readonly Color Fondo = Color.FromArgb(248, 250, 252);
        public static readonly Color Encabezado = Color.FromArgb(30, 41, 59);
        public static readonly Color Texto = Color.FromArgb(15, 23, 42);
        public static readonly Color Azul = Color.FromArgb(37, 99, 235);   // Origen en el mapa
        public static readonly Color Rojo = Color.FromArgb(220, 38, 38);   // Destino en el mapa / error
        public static readonly Color Verde = Color.FromArgb(22, 163, 74);  // Éxito
        public static readonly Color Gris = Color.FromArgb(100, 116, 139);

        // Franja oscura con título y subtítulo
        public static Panel CrearEncabezado(string titulo, string subtitulo)
        {
            Panel encabezado = new Panel { Width = 360, Height = 60, BackColor = Encabezado };
            encabezado.Controls.Add(new Label
            {
                Text = titulo, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White, Location = new Point(10, 8), AutoSize = true
            });
            encabezado.Controls.Add(new Label
            {
                Text = subtitulo, Font = new Font("Segoe UI", 8.25F),
                ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(10, 34), AutoSize = true
            });
            return encabezado;
        }

        // Recuadro con título para cada sección del panel izquierdo
        public static GroupBox CrearSeccion(string titulo, int alto) => new GroupBox
        {
            Text = titulo, Width = 360, Height = alto,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Texto,
            Margin = new Padding(0, 12, 0, 0)
        };

        public static Label CrearEtiqueta(string texto, int x, int y) => new Label
        {
            Text = texto, Location = new Point(x, y + 3), AutoSize = true, Font = new Font("Segoe UI", 8.5F)
        };

        public static Button CrearBoton(string texto, Color color, int x, int y, int ancho = 78) => new Button
        {
            Text = texto, Location = new Point(x, y), Size = new Size(ancho, 32),
            BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand
        };

        // Mensaje en verde (éxito) o rojo (error)
        public static void MostrarMensaje(Label etiqueta, string texto, bool exito)
        {
            etiqueta.Text = texto;
            etiqueta.ForeColor = exito ? Verde : Rojo;
        }
    }
}
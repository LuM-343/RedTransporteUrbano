using System.Windows.Forms;

namespace TransporteUrbanoEDD.UI.Controles
{
    /// <summary>
    /// Panel especializado con doble búfer activado para evitar parpadeos
    /// durante el renderizado en tiempo real del grafo con GDI+.
    /// Desarrollado por Dev 3.
    /// </summary>
    public class PanelDobleBuffer : Panel
    {
        public PanelDobleBuffer()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }
}

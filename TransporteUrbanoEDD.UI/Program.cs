namespace TransporteUrbanoEDD.UI;

static class Program
{
    /// <summary>
    /// Punto de entrada principal para la aplicación de interfaz gráfica.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Inicia directamente en la ventana de prueba de Dev 3
        Application.Run(new FormPruebaDev3());
    }    
}
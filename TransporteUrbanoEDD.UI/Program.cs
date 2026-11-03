namespace TransporteUrbanoEDD.UI;

static class Program
{
    /// <summary>
    /// Punto de entrada principal para la aplicación de interfaz gráfica.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // "dotnet run -- dev1" abre la prueba de la Pestaña 1; sin argumento, la de Dev 3
        if (args.Length > 0 && args[0] == "dev1")
            Application.Run(new FormPruebaDev1());
        else
            Application.Run(new FormPruebaDev3());
    }    
}
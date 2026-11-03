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

        // Sin argumento: programa completo. "dev1" / "dev3": ventanas de prueba de cada pestaña
        string modo = args.Length > 0 ? args[0] : "";

        if (modo == "dev1")
            Application.Run(new FormPruebaDev1());
        else if (modo == "dev3")
            Application.Run(new FormPruebaDev3());
        else
            Application.Run(new FormPrincipal());
    }
}
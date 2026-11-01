// Pruebas rápidas de las estructuras del Core (no es parte de la entrega)
using TransporteUrbanoEDD.Core.Estructuras;

ListaPropia<string> lista = new ListaPropia<string>();
lista.Agregar("PAR-01");
lista.Agregar("PAR-02");
lista.Agregar("PAR-03");

Console.WriteLine($"Elementos: {lista.Count}"); // Debe imprimir 3

Console.WriteLine($"Posición 1: {lista.Obtener(1)}");          // PAR-02
Console.WriteLine($"¿Contiene PAR-03? {lista.Contiene("PAR-03")}"); // True
Console.WriteLine($"¿Contiene PAR-99? {lista.Contiene("PAR-99")}"); // False
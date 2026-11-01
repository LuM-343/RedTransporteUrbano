// Pruebas rápidas de las estructuras del Core (no es parte de la entrega)
using TransporteUrbanoEDD.Core.Estructuras;

ListaPropia<string> lista = new ListaPropia<string>();
lista.Agregar("PAR-01");
lista.Agregar("PAR-02");
lista.Agregar("PAR-03");

Console.WriteLine($"Elementos: {lista.Count}"); // Debe imprimir 3
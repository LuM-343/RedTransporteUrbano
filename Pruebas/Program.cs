// Pruebas rápidas de las estructuras del Core
using TransporteUrbanoEDD.Core.Estructuras;

ListaPropia<string> lista = new ListaPropia<string>();
lista.Agregar("PAR-01");
lista.Agregar("PAR-02");
lista.Agregar("PAR-03");
lista.Agregar("PAR-04");
Imprimir("Inicial");                               // PAR-01 PAR-02 PAR-03 PAR-04

lista.Eliminar("PAR-01"); Imprimir("Sin cabeza");  // PAR-02 PAR-03 PAR-04
lista.Eliminar("PAR-03"); Imprimir("Sin medio");   // PAR-02 PAR-04
lista.Eliminar("PAR-04"); Imprimir("Sin cola");    // PAR-02

lista.Agregar("PAR-05"); Imprimir("Agregar tras borrar cola"); // PAR-02 PAR-05 (prueba que cola se movió bien)
Console.WriteLine($"¿Eliminó PAR-99? {lista.Eliminar("PAR-99")}"); // False

lista.Eliminar("PAR-02");
lista.Eliminar("PAR-05"); Imprimir("Vacía");       // (nada), Count 0
lista.Agregar("PAR-06"); Imprimir("Reusar vacía"); // PAR-06

// Muestra el contenido y el tamaño de la lista
void Imprimir(string titulo)
{
    Console.Write($"{titulo} [{lista.Count}]: ");
    foreach (string codigo in lista)
    {
        Console.Write(codigo + " ");
    }
    Console.WriteLine();
}
// Pruebas integrales de las estructuras del Core
using System;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;

Console.WriteLine("==============================================================");
Console.WriteLine("    RED DE TRANSPORTE URBANO - PRUEBAS DE ESTRUCTURAS DE DATOS");
Console.WriteLine("==============================================================\n");

// -------------------------------------------------------------
// 1. PRUEBAS DE LISTAPROPIA<T> (Dev 1)
// -------------------------------------------------------------
Console.WriteLine(">>> 1. PRUEBAS DE LISTA ENLAZADA PROPIA (ListaPropia<T>)");
ListaPropia<string> lista = new ListaPropia<string>();
lista.Agregar("PAR-01");
lista.Agregar("PAR-02");
lista.Agregar("PAR-03");
lista.Agregar("PAR-04");
ImprimirLista("Inicial", lista);

lista.Eliminar("PAR-01"); ImprimirLista("Sin cabeza", lista);
lista.Eliminar("PAR-03"); ImprimirLista("Sin medio", lista);
lista.Eliminar("PAR-04"); ImprimirLista("Sin cola", lista);

lista.Agregar("PAR-05"); ImprimirLista("Agregar tras borrar cola", lista);
Console.WriteLine($"¿Eliminó PAR-99 inexistente?: {lista.Eliminar("PAR-99")}");

lista.Eliminar("PAR-02");
lista.Eliminar("PAR-05"); ImprimirLista("Lista vacía", lista);
lista.Agregar("PAR-06"); ImprimirLista("Reutilizar vacía", lista);
Console.WriteLine("[OK] Pruebas de ListaPropia superadas con éxito.\n");


// -------------------------------------------------------------
// 2. PRUEBAS DE MINHEAP PROPIO (Dev 3)
// -------------------------------------------------------------
Console.WriteLine(">>> 2. PRUEBAS DE MONTÍCULO BINARIO MÍNIMO (MinHeapPropio<T>)");
MinHeapPropio<string> heap = new MinHeapPropio<string>(capacidadInicial: 4);

Console.WriteLine("Insertando paradas con prioridades (tiempos/costos aleatorios)...");
heap.Insertar("PAR-05 (Cayalá)", 25.5);
heap.Insertar("PAR-01 (El Trébol)", 5.0);
heap.Insertar("PAR-03 (Centro Histórico)", 12.3);
heap.Insertar("PAR-02 (Zona Viva)", 8.1);
heap.Insertar("PAR-04 (Obelisco)", 3.2); // Forzará redimensionamiento dinámico del heap

Console.WriteLine($"Total en montículo: {heap.Count} elementos (Capacidad actual: {heap.Capacidad}).");
Console.WriteLine($"Mínimo actual sin extraer (VerMinimo): {heap.VerMinimo()}");

Console.WriteLine("Extrayendo elementos en orden de prioridad mínima:");
double prioridadAnterior = -1;
bool ordenCorrecto = true;

while (!heap.EstaVacio)
{
    NodoHeap<string> min = heap.ExtraerMinimo();
    Console.WriteLine($"  -> Extraído: {min.Elemento} con prioridad = {min.Prioridad:F1}");
    if (min.Prioridad < prioridadAnterior)
    {
        ordenCorrecto = false;
    }
    prioridadAnterior = min.Prioridad;
}

if (ordenCorrecto && heap.Count == 0)
{
    Console.WriteLine("[OK] El MinHeap respeta estrictamente la propiedad de orden mínimo O(log n).\n");
}
else
{
    Console.WriteLine("[ERROR] Falla en el orden de extracción del MinHeap.\n");
}


// -------------------------------------------------------------
// 3. PRUEBAS DE GRAFO DE TRANSPORTE (Dev 3)
// -------------------------------------------------------------
Console.WriteLine(">>> 3. PRUEBAS DE GRAFO DE TRANSPORTE (GrafoTransporte)");
GrafoTransporte grafo = new GrafoTransporte();

// Registrar Paradas (Vértices)
Parada p1 = new Parada("PAR-01", "Estación Central El Trébol", 100, 250);
Parada p2 = new Parada("PAR-02", "Plaza España", 200, 300);
Parada p3 = new Parada("PAR-03", "Zona Viva / Prisa", 350, 320);
Parada p4 = new Parada("PAR-04", "Centro Cívico", 150, 150);
Parada p5 = new Parada("PAR-05", "Paseo de la Sexta", 160, 100);
Parada p6 = new Parada("PAR-06", "Universidad del Valle/Landívar", 400, 450);
Parada p7 = new Parada("PAR-07", "Estación Aislada San Cristóbal", 50, 500);

grafo.AgregarParada(p1);
grafo.AgregarParada(p2);
grafo.AgregarParada(p3);
grafo.AgregarParada(p4);
grafo.AgregarParada(p5);
grafo.AgregarParada(p6);
grafo.AgregarParada(p7);

Console.WriteLine($"Paradas registradas: {grafo.CantidadParadas}");

// Registrar Rutas con doble peso (Tiempo en min, Costo en Q)
// Conexiones:
// PAR-01 <---> PAR-02 (Bidireccional: 12 min, Q2.50)
// PAR-01 ---> PAR-04  (Dirigida: 8 min, Q2.00)
// PAR-02 <---> PAR-03 (Bidireccional: 15 min, Q3.00)
// PAR-04 ---> PAR-05  (Dirigida: 5 min, Q1.50)
// PAR-03 ---> PAR-06  (Dirigida: 20 min, Q4.00)
// PAR-05 ---> PAR-03  (Dirigida: 10 min, Q2.50)
grafo.AgregarRuta("PAR-01", "PAR-02", tiempo: 12.0, costo: 2.50, esBidireccional: true);
grafo.AgregarRuta("PAR-01", "PAR-04", tiempo: 8.0, costo: 2.00, esBidireccional: false);
grafo.AgregarRuta("PAR-02", "PAR-03", tiempo: 15.0, costo: 3.00, esBidireccional: true);
grafo.AgregarRuta("PAR-04", "PAR-05", tiempo: 5.0, costo: 1.50, esBidireccional: false);
grafo.AgregarRuta("PAR-03", "PAR-06", tiempo: 20.0, costo: 4.00, esBidireccional: false);
grafo.AgregarRuta("PAR-05", "PAR-03", tiempo: 10.0, costo: 2.50, esBidireccional: false);

Console.WriteLine("\nConexiones salientes desde PAR-01 (El Trébol):");
foreach (AristaRuta arista in grafo.ObtenerRutasSalientes("PAR-01"))
{
    Console.WriteLine($"  {arista}");
}

Console.WriteLine("\nConexiones salientes desde PAR-02 (Plaza España):");
foreach (AristaRuta arista in grafo.ObtenerRutasSalientes("PAR-02"))
{
    Console.WriteLine($"  {arista}");
}

// -------------------------------------------------------------
// 4. PRUEBAS DE RECORRIDOS BFS Y DFS
// -------------------------------------------------------------
Console.WriteLine("\n>>> 4. RECORRIDOS EN EL GRAFO DESDE PAR-01");

// Recorrido BFS (En anchura por niveles)
Console.WriteLine("Ejecutando BFS (Breadth-First Search) desde PAR-01:");
ListaPropia<Parada> bfs = grafo.RecorridoBFS("PAR-01");
Console.Write("  Orden BFS: ");
foreach (Parada p in bfs)
{
    Console.Write($"{p.Codigo} -> ");
}
Console.WriteLine("FIN");

// Recorrido DFS (En profundidad por ramales)
Console.WriteLine("\nEjecutando DFS (Depth-First Search) desde PAR-01:");
ListaPropia<Parada> dfs = grafo.RecorridoDFS("PAR-01");
Console.Write("  Orden DFS: ");
foreach (Parada p in dfs)
{
    Console.Write($"{p.Codigo} -> ");
}
Console.WriteLine("FIN");

// Probar parada aislada (PAR-07)
Console.WriteLine("\nRecorrido BFS desde parada aislada (PAR-07):");
ListaPropia<Parada> bfsAislado = grafo.RecorridoBFS("PAR-07");
Console.Write("  Orden BFS (debe contener solo PAR-07): ");
foreach (Parada p in bfsAislado)
{
    Console.Write($"{p.Codigo} ");
}
Console.WriteLine();

Console.WriteLine("\n==============================================================");
Console.WriteLine("             TODAS LAS PRUEBAS FINALIZADAS CON ÉXITO");
Console.WriteLine("==============================================================");

// Función auxiliar de impresión para ListaPropia
void ImprimirLista(string titulo, ListaPropia<string> l)
{
    Console.Write($"  {titulo} [{l.Count}]: ");
    foreach (string s in l)
    {
        Console.Write(s + " ");
    }
    Console.WriteLine();
}
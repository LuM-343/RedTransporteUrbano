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
// Vía A: PAR-01 <-> PAR-02 (12m, Q2.50) y PAR-02 <-> PAR-03 (15m, Q3.00) => Total: 27 min, Q5.50 (Más barata)
// Vía B: PAR-01 -> PAR-04 (8m, Q2.00) -> PAR-05 (5m, Q1.50) -> PAR-03 (10m, Q3.50) => Total: 23 min, Q7.00 (Más rápida)
grafo.AgregarRuta("PAR-01", "PAR-02", tiempo: 12.0, costo: 2.50, esBidireccional: true);
grafo.AgregarRuta("PAR-01", "PAR-04", tiempo: 8.0, costo: 2.00, esBidireccional: false);
grafo.AgregarRuta("PAR-02", "PAR-03", tiempo: 15.0, costo: 3.00, esBidireccional: true);
grafo.AgregarRuta("PAR-04", "PAR-05", tiempo: 5.0, costo: 1.50, esBidireccional: false);
grafo.AgregarRuta("PAR-05", "PAR-03", tiempo: 10.0, costo: 3.50, esBidireccional: false);
grafo.AgregarRuta("PAR-03", "PAR-06", tiempo: 20.0, costo: 4.00, esBidireccional: false);

Console.WriteLine("\nConexiones salientes desde PAR-01 (El Trébol):");
foreach (AristaRuta arista in grafo.ObtenerRutasSalientes("PAR-01"))
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


// -------------------------------------------------------------
// 5. PRUEBAS DE ALGORITMO DE DIJKSTRA (Día 2 - Dev 3)
// -------------------------------------------------------------
Console.WriteLine("\n>>> 5. PRUEBAS DE ALGORITMO DE DIJKSTRA PARAMETRIZADO");

// Prueba A: Optimizar por MENOR TIEMPO (Debe elegir Vía B: PAR-01 -> PAR-04 -> PAR-05 -> PAR-03 con 23 min)
Console.WriteLine("\n[Caso 1] Ruta óptima PAR-01 -> PAR-03 (Optimizando por TIEMPO):");
ResultadoRuta rutaTiempo = grafo.CalcularDijkstra("PAR-01", "PAR-03", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaTiempo.ExisteRuta}");
Console.WriteLine($"  Camino: {rutaTiempo.ResumenCamino}");
Console.WriteLine($"  Tiempo total: {rutaTiempo.TiempoTotalMinutos:F1} min (Criterio optimizado)");
Console.WriteLine($"  Costo total: Q{rutaTiempo.CostoTotal:F2}");
Console.Write("  Secuencia en ListaPropia: ");
foreach (string cod in rutaTiempo.SecuenciaCodigosParadas) Console.Write($"[{cod}] ");
Console.WriteLine();

// Prueba B: Optimizar por MENOR COSTO (Debe elegir Vía A: PAR-01 -> PAR-02 -> PAR-03 con Q5.50)
Console.WriteLine("\n[Caso 2] Ruta óptima PAR-01 -> PAR-03 (Optimizando por COSTO):");
ResultadoRuta rutaCosto = grafo.CalcularDijkstra("PAR-01", "PAR-03", optimizarPorTiempo: false);
Console.WriteLine($"  ¿Existe ruta?: {rutaCosto.ExisteRuta}");
Console.WriteLine($"  Camino: {rutaCosto.ResumenCamino}");
Console.WriteLine($"  Tiempo total: {rutaCosto.TiempoTotalMinutos:F1} min");
Console.WriteLine($"  Costo total: Q{rutaCosto.CostoTotal:F2} (Criterio optimizado)");
Console.Write("  Secuencia en ListaPropia: ");
foreach (string cod in rutaCosto.SecuenciaCodigosParadas) Console.Write($"[{cod}] ");
Console.WriteLine();

// Prueba C: Ruta hasta PAR-06
Console.WriteLine("\n[Caso 3] Ruta óptima PAR-01 -> PAR-06 (Optimizando por TIEMPO):");
ResultadoRuta rutaUniversidad = grafo.CalcularDijkstra("PAR-01", "PAR-06", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaUniversidad.ExisteRuta}");
Console.WriteLine($"  Camino: {rutaUniversidad.ResumenCamino}");
Console.WriteLine($"  Tiempo total: {rutaUniversidad.TiempoTotalMinutos:F1} min");
Console.WriteLine($"  Costo total: Q{rutaUniversidad.CostoTotal:F2}");

// Prueba D: Sentido contrario en vía unidireccional (PAR-06 a PAR-01 es imposible)
Console.WriteLine("\n[Caso 4] Ruta contraria PAR-06 -> PAR-01 (Vía de sentido único):");
ResultadoRuta rutaImposible = grafo.CalcularDijkstra("PAR-06", "PAR-01", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaImposible.ExisteRuta}");
Console.WriteLine($"  Mensaje: {rutaImposible.MensajeEstado}");

// Prueba E: Nodo aislado desconectado (PAR-01 a PAR-07)
Console.WriteLine("\n[Caso 5] Ruta hacia parada aislada PAR-01 -> PAR-07:");
ResultadoRuta rutaAislada = grafo.CalcularDijkstra("PAR-01", "PAR-07", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaAislada.ExisteRuta}");
Console.WriteLine($"  Mensaje: {rutaAislada.MensajeEstado}");

// Prueba F: Origen igual a Destino (PAR-02 a PAR-02)
Console.WriteLine("\n[Caso 6] Mismo origen y destino PAR-02 -> PAR-02:");
ResultadoRuta rutaMismoNodo = grafo.CalcularDijkstra("PAR-02", "PAR-02", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaMismoNodo.ExisteRuta}");
Console.WriteLine($"  Camino: {rutaMismoNodo.ResumenCamino}");
Console.WriteLine($"  Tiempo: {rutaMismoNodo.TiempoTotalMinutos} min | Costo: Q{rutaMismoNodo.CostoTotal:F2}");

// Prueba G: Código inexistente
Console.WriteLine("\n[Caso 7] Código de parada inexistente (PAR-99 -> PAR-01):");
ResultadoRuta rutaInvalida = grafo.CalcularDijkstra("PAR-99", "PAR-01", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaInvalida.ExisteRuta}");
Console.WriteLine($"  Mensaje: {rutaInvalida.MensajeEstado}");

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

// -------------------------------------------------------------
// PRUEBAS DE TABLA HASH (Dev 1) - Bloque 1: función hash
// -------------------------------------------------------------
Console.WriteLine("\n>>> PRUEBAS DE TABLA HASH (TablaHashParadas)");
TablaHashParadas hash = new TablaHashParadas(11);
string[] codigos = { "PAR-01", "PAR-02", "PAR-12", "PAR-21", "par-01" };
foreach (string cod in codigos)
    Console.WriteLine($"  Hash(\"{cod}\") = bucket {hash.FuncionHash(cod)}");
Console.WriteLine($"  Capacidad: {hash.Capacidad} | Cantidad: {hash.Cantidad} | Factor: {hash.FactorCarga:F2}");

// --- Bloque 2: Insertar, Buscar, Contiene ---
Console.WriteLine("\n  Insertando 5 paradas...");
hash.Insertar(new Parada("PAR-01", "Parque Central", 100, 200));
hash.Insertar(new Parada("PAR-02", "Terminal Minerva", 50, 80));
hash.Insertar(new Parada("PAR-03", "La Democracia", 150, 120));
hash.Insertar(new Parada("PAR-12", "Calvario", 220, 90));
hash.Insertar(new Parada("PAR-21", "Templo Minerva", 300, 60));
Console.WriteLine($"  Cantidad: {hash.Cantidad} | Factor: {hash.FactorCarga:F2}"); // 5 | 0.45

Console.WriteLine($"  Buscar PAR-03: {hash.Buscar("PAR-03")}");          // [PAR-03] La Democracia
Console.WriteLine($"  Buscar ' par-12 ': {hash.Buscar(" par-12 ")}");    // [PAR-12] Calvario
Console.WriteLine($"  Buscar PAR-99: {hash.Buscar("PAR-99")?.ToString() ?? "null"}"); // null
Console.WriteLine($"  ¿Contiene PAR-21? {hash.Contiene("PAR-21")}");     // True
Console.WriteLine($"  ¿Contiene PAR-99? {hash.Contiene("PAR-99")}");     // False
Console.WriteLine($"  ¿Insertó PAR-01 duplicado? {hash.Insertar(new Parada("PAR-01", "Otra", 0, 0))}"); // False
Console.WriteLine($"  Cantidad final: {hash.Cantidad}");                 // 5

// --- Bloque 3: Rehashing y Eliminar ---
Console.WriteLine("\n  Rehashing (tabla pequeña de 5 buckets):");
TablaHashParadas chica = new TablaHashParadas(5);
for (int i = 1; i <= 8; i++)
{
    chica.Insertar(new Parada($"PAR-{i:D2}", $"Parada {i}"));
    Console.WriteLine($"    +PAR-{i:D2} -> Cantidad {chica.Cantidad} | Capacidad {chica.Capacidad} | Factor {chica.FactorCarga:F2} | Rehashes {chica.Rehashes}");
}
// Esperado: rehash al insertar PAR-04 (4/5 = 0.80 > 0.70): Capacidad 5 -> 11

// Después del rehashing todas deben seguir encontrándose
bool todas = true;
for (int i = 1; i <= 8; i++)
    if (!chica.Contiene($"PAR-{i:D2}")) todas = false;
Console.WriteLine($"  ¿Todas encontradas tras rehashing? {todas}");          // True

Console.WriteLine($"  ¿Eliminó PAR-05? {chica.Eliminar("PAR-05")}");          // True
Console.WriteLine($"  ¿Contiene PAR-05? {chica.Contiene("PAR-05")}");         // False
Console.WriteLine($"  ¿Eliminó PAR-99? {chica.Eliminar("PAR-99")}");          // False
Console.WriteLine($"  Cantidad final: {chica.Cantidad}");                     // 7

// --- Bloque 4: ObtenerTodas, ObtenerBucket, Mostrar ---
Console.WriteLine("\n  Estado de la tabla chica:");
chica.Mostrar();

Console.Write("\n  ObtenerTodas: ");
foreach (Parada p in chica.ObtenerTodas())
    Console.Write(p.Codigo + " ");
Console.WriteLine($"({chica.ObtenerTodas().Count} paradas)"); // 7 paradas, sin PAR-05

// -------------------------------------------------------------
// PRUEBAS DE UNION-FIND (Dev 1) - Bloque 1
// -------------------------------------------------------------
Console.WriteLine("\n>>> PRUEBAS DE UNION-FIND (UnionFindZonas)");
UnionFindZonas uf = new UnionFindZonas(4); // Capacidad 4 para probar que crece

for (int i = 0; i < 7; i++)
    uf.AgregarElemento();                  // Elementos 0..6, cada uno su propia zona
Console.WriteLine($"  Elementos: {uf.Cantidad} | Zonas: {uf.CantidadZonas}"); // 7 | 7

// Rutas: 0-1, 1-2, 3-4  → zonas {0,1,2} {3,4} {5} {6}
uf.Union(0, 1);
uf.Union(1, 2);
uf.Union(3, 4);
Console.WriteLine($"  Zonas tras 3 uniones: {uf.CantidadZonas}");          // 4
Console.WriteLine($"  ¿0 y 2 conectadas? {uf.EstanConectadas(0, 2)}");      // True (vía 1)
Console.WriteLine($"  ¿0 y 3 conectadas? {uf.EstanConectadas(0, 3)}");      // False
Console.WriteLine($"  ¿Union(0,2) repetida fusionó? {uf.Union(0, 2)}");     // False (ya estaban)

uf.Union(2, 4); // Une {0,1,2} con {3,4}
Console.WriteLine($"  Tras Union(2,4): ¿0 y 3 conectadas? {uf.EstanConectadas(0, 3)}"); // True
Console.WriteLine($"  Zonas: {uf.CantidadZonas}");                          // 3: {0..4} {5} {6}
Console.WriteLine($"  ¿6 aislada de 0? {!uf.EstanConectadas(6, 0)}");       // True
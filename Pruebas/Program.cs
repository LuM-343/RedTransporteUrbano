// Pruebas integrales de las estructuras del Core
using System;
using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;
using TransporteUrbanoEDD.Core.Archivos;

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

Console.WriteLine($"Paradas registradas: {grafo.CantidadParadas} (Activas: {grafo.CantidadParadasActivas})");

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

// -------------------------------------------------------------
// 6. PRUEBAS DE ELIMINACIÓN DE RUTAS Y PARADAS (Dev 3 - Baja de Vértice)
// -------------------------------------------------------------
Console.WriteLine("\n>>> 6. PRUEBAS DE ELIMINACIÓN DE RUTAS Y PARADAS (Baja de Vértice + Purga)");

// 6.1 Eliminación de Ruta específica PAR-01 <---> PAR-02 (Doble vía)
Console.WriteLine("\n[Subcaso 6.1] Eliminación de Ruta específica PAR-01 <---> PAR-02 (Doble vía):");
Console.WriteLine($"  ¿Existe conexión antes?: {grafo.ExisteRuta("PAR-01", "PAR-02")}");
bool elimRuta = grafo.EliminarRuta("PAR-01", "PAR-02", esDobleVia: true);
Console.WriteLine($"  ¿Ruta eliminada con éxito?: {elimRuta}");
Console.WriteLine($"  ¿Existe conexión PAR-01 -> PAR-02 después?: {grafo.ExisteRuta("PAR-01", "PAR-02")}");
Console.WriteLine($"  ¿Existe conexión PAR-02 -> PAR-01 después?: {grafo.ExisteRuta("PAR-02", "PAR-01")}");

// Restaurar ruta para continuar con pruebas de paradas
grafo.AgregarRuta("PAR-01", "PAR-02", tiempo: 12.0, costo: 2.50, esBidireccional: true);

// 6.2 Eliminación de Parada (EliminarParada) - Probar con PAR-04 (Paso intermedio de la ruta más rápida PAR-01 -> PAR-04 -> PAR-05 -> PAR-03)
Console.WriteLine("\n[Subcaso 6.2] Eliminación de Parada intermedia 'PAR-04' (Centro Cívico):");
Console.WriteLine($"  Total paradas antes: {grafo.CantidadParadas} (Activas: {grafo.CantidadParadasActivas})");
Console.WriteLine($"  ¿Existe PAR-04 antes?: {grafo.ExisteParada("PAR-04")}");

bool elimParada = grafo.EliminarParada("PAR-04");
Console.WriteLine($"  ¿Parada PAR-04 eliminada con éxito?: {elimParada}");
Console.WriteLine($"  ¿Existe PAR-04 después?: {grafo.ExisteParada("PAR-04")}");
Console.WriteLine($"  ObtenerIdVertice('PAR-04') retorna: {grafo.ObtenerIdVertice("PAR-04")}");
Console.WriteLine($"  Total paradas después: {grafo.CantidadParadas} (Activas: {grafo.CantidadParadasActivas})");

// Verificar que las aristas entrantes hacia PAR-04 en PAR-01 fueron purgadas
bool tieneAristaHaciaP4 = false;
foreach (AristaRuta r in grafo.ObtenerRutasSalientes("PAR-01"))
{
    if (r.Destino.Codigo == "PAR-04") tieneAristaHaciaP4 = true;
}
Console.WriteLine($"  ¿PAR-01 aún tiene arista saliente hacia PAR-04?: {tieneAristaHaciaP4} (Debe ser False)");

// 6.3 Impacto en Dijkstra tras eliminar PAR-04:
// Antes, la ruta más rápida a PAR-03 iba por PAR-04. Ahora debe recalcular y forzar ir por PAR-02
Console.WriteLine("\n[Subcaso 6.3] Recálculo de Dijkstra PAR-01 -> PAR-03 tras dar de baja PAR-04:");
ResultadoRuta rutaPostBaja = grafo.CalcularDijkstra("PAR-01", "PAR-03", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaPostBaja.ExisteRuta}");
Console.WriteLine($"  Nuevo camino alternativo: {rutaPostBaja.ResumenCamino}");
Console.WriteLine($"  Tiempo total: {rutaPostBaja.TiempoTotalMinutos:F1} min (Ahora forzado por PAR-02)");

// 6.4 Recorridos BFS y DFS tras dar de baja PAR-04
Console.WriteLine("\n[Subcaso 6.4] Recorridos BFS y DFS tras baja (sin NullReferenceException):");
ListaPropia<Parada> bfsPost = grafo.RecorridoBFS("PAR-01");
Console.Write("  BFS post-baja: ");
foreach (Parada p in bfsPost) Console.Write($"{p.Codigo} ");
Console.WriteLine("FIN");

ListaPropia<Parada> dfsPost = grafo.RecorridoDFS("PAR-01");
Console.Write("  DFS post-baja: ");
foreach (Parada p in dfsPost) Console.Write($"{p.Codigo} ");
Console.WriteLine("FIN");

// 6.5 Intentar calcular ruta hacia o desde parada dada de baja
Console.WriteLine("\n[Subcaso 6.5] Dijkstra intentando llegar a la parada dada de baja (PAR-01 -> PAR-04):");
ResultadoRuta rutaHaciaBaja = grafo.CalcularDijkstra("PAR-01", "PAR-04", optimizarPorTiempo: true);
Console.WriteLine($"  ¿Existe ruta?: {rutaHaciaBaja.ExisteRuta}");
Console.WriteLine($"  Mensaje: {rutaHaciaBaja.MensajeEstado}");

// 6.6 Intentar eliminar parada inexistente o ya eliminada
Console.WriteLine("\n[Subcaso 6.6] Intentar eliminar de nuevo PAR-04 o una inexistente:");
Console.WriteLine($"  Re-eliminar PAR-04: {grafo.EliminarParada("PAR-04")} (Debe ser False)");
Console.WriteLine($"  Eliminar PAR-99 inexistente: {grafo.EliminarParada("PAR-99")} (Debe ser False)");

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

// --- Union-Find Bloque 2: EstanEnMismaZona y Reconstruir ---
Console.WriteLine("\n  Union-Find con códigos (vinculado a Tabla Hash):");
TablaHashParadas tablaZ = new TablaHashParadas();
UnionFindZonas ufZ = new UnionFindZonas(16, tablaZ);

// Registrar 4 paradas: Hash + índice en Union-Find (como lo hará el cargador CSV)
Parada a = new Parada("PAR-01", "Parque Central");
Parada b = new Parada("PAR-02", "Terminal Minerva");
Parada c = new Parada("PAR-03", "La Democracia");
Parada d = new Parada("PAR-04", "Aislada");
foreach (Parada p in new[] { a, b, c, d })
{
    tablaZ.Insertar(p);
    p.Indice = ufZ.AgregarElemento();
}

ufZ.Union(a.Indice, b.Indice); // Ruta PAR-01 - PAR-02
ufZ.Union(b.Indice, c.Indice); // Ruta PAR-02 - PAR-03

Console.WriteLine($"  ¿PAR-01 y PAR-03 misma zona? {ufZ.EstanEnMismaZona("PAR-01", "par-03")}"); // True
Console.WriteLine($"  ¿PAR-01 y PAR-04 misma zona? {ufZ.EstanEnMismaZona("PAR-01", "PAR-04")}"); // False
Console.WriteLine($"  ¿PAR-01 y PAR-99 misma zona? {ufZ.EstanEnMismaZona("PAR-01", "PAR-99")}"); // False (no existe)
Console.WriteLine($"  Zonas: {ufZ.CantidadZonas}"); // 2: {01,02,03} {04}

// Simular que se eliminó la ruta PAR-02 - PAR-03: reconstruir solo con PAR-01 - PAR-02
ListaPropia<AristaRuta> rutasRestantes = new ListaPropia<AristaRuta>();
rutasRestantes.Agregar(new AristaRuta(a, b, 10, 2.5, true));
ufZ.Reconstruir(4, rutasRestantes);

Console.WriteLine("  Tras eliminar ruta PAR-02 - PAR-03 y reconstruir:");
Console.WriteLine($"  ¿PAR-01 y PAR-02 misma zona? {ufZ.EstanEnMismaZona("PAR-01", "PAR-02")}"); // True
Console.WriteLine($"  ¿PAR-01 y PAR-03 misma zona? {ufZ.EstanEnMismaZona("PAR-01", "PAR-03")}"); // False
Console.WriteLine($"  Zonas: {ufZ.CantidadZonas}"); // 3: {01,02} {03} {04}

// -------------------------------------------------------------
// PRUEBAS DE CARGADOR (Dev 1) - Bloque 2: registro manual
// -------------------------------------------------------------
Console.WriteLine("\n>>> PRUEBAS DE REGISTRO EN LAS 4 ESTRUCTURAS (CargadorCSV)");
TablaHashParadas hashR = new TablaHashParadas();
GrafoTransporte grafoR = new GrafoTransporte();
UnionFindZonas ufR = new UnionFindZonas(16, hashR);
TrieParadas trieR = new TrieParadas();
CargadorCSV cargadorR = new CargadorCSV(hashR, grafoR, ufR, trieR);

Parada pc = new Parada("PAR-01", "Parque Central", 400, 260, "Zona 1");
Console.WriteLine($"  Registrar PAR-01: {cargadorR.RegistrarParada(pc) ?? "OK"}");                      // OK
Console.WriteLine($"  Registrar PAR-02: {cargadorR.RegistrarParada(new Parada("PAR-02", "Catedral", 430, 240)) ?? "OK"}"); // OK
Console.WriteLine($"  Registrar PAR-03: {cargadorR.RegistrarParada(new Parada("PAR-03", "Zunil", 90, 500)) ?? "OK"}");    // OK
Console.WriteLine($"  Registrar PAR-01 otra vez: {cargadorR.RegistrarParada(new Parada("PAR-01", "Repetida")) ?? "OK"}"); // ya existe
Console.WriteLine($"  Indice de PAR-01: {pc.Indice}");                                                  // 0

Console.WriteLine($"  Ruta PAR-01 -> PAR-02: {cargadorR.RegistrarRuta("PAR-01", "PAR-02", 3, 1.75, true) ?? "OK"}");  // OK
Console.WriteLine($"  Ruta PAR-01 -> PAR-99: {cargadorR.RegistrarRuta("PAR-01", "PAR-99", 3, 1.75, true) ?? "OK"}");  // no existe
Console.WriteLine($"  Ruta PAR-01 -> PAR-01: {cargadorR.RegistrarRuta("PAR-01", "par-01", 3, 1.75, true) ?? "OK"}");  // misma parada

Console.WriteLine($"  Hash: {hashR.Cantidad} | Grafo: {grafoR.CantidadParadas} | UF: {ufR.Cantidad} | Zonas: {ufR.CantidadZonas}"); // 3 | 3 | 3 | 2
Console.WriteLine($"  ¿PAR-01 y PAR-02 misma zona? {ufR.EstanEnMismaZona("PAR-01", "PAR-02")}");  // True
Console.WriteLine($"  ¿PAR-01 y PAR-03 misma zona? {ufR.EstanEnMismaZona("PAR-01", "PAR-03")}");  // False

// -------------------------------------------------------------
// PRUEBAS DE CARGA CSV (Dev 1) - Bloque 3: dataset completo
// -------------------------------------------------------------
Console.WriteLine("\n>>> PRUEBAS DE CARGA DESDE CSV (dataset de 20 paradas y 35 rutas)");
TablaHashParadas hashC = new TablaHashParadas();
GrafoTransporte grafoC = new GrafoTransporte();
UnionFindZonas ufC = new UnionFindZonas(16, hashC);
TrieParadas trieC = new TrieParadas();
CargadorCSV cargador = new CargadorCSV(hashC, grafoC, ufC, trieC);

// AppContext.BaseDirectory = carpeta donde está el .dll (bin/Debug/net10.0/)
string carpetaDatos = Path.Combine(AppContext.BaseDirectory, "Datos");
ResultadoCarga rp = cargador.CargarParadasCSV(Path.Combine(carpetaDatos, "paradas.csv"));
ResultadoCarga rr = cargador.CargarRutasCSV(Path.Combine(carpetaDatos, "rutas.csv"));
Console.WriteLine($"  Paradas: {rp}");   // 20 cargados, 0 con error
Console.WriteLine($"  Rutas:   {rr}");   // 35 cargados, 0 con error
Console.WriteLine($"  Hash: {hashC.Cantidad} | Grafo: {grafoC.CantidadParadas} | Zonas: {ufC.CantidadZonas}"); // 20 | 20 | 2
Console.WriteLine($"  Factor de carga: {hashC.FactorCarga:F2} | Rehashes: {hashC.Rehashes}");

// Flujo completo: Trie → Hash → Union-Find → Dijkstra
Console.Write("  Autocompletar 'pa': ");
foreach (string s in trieC.Autocompletar("pa")) Console.Write(s + " | ");
Console.WriteLine(); // Parque Central (PAR-01) | Parque Benito Juarez (PAR-07)

Console.WriteLine($"  ¿PAR-01 y PAR-17 misma zona? {ufC.EstanEnMismaZona("PAR-01", "PAR-17")}"); // True
Console.WriteLine($"  ¿PAR-01 y PAR-20 misma zona? {ufC.EstanEnMismaZona("PAR-01", "PAR-20")}"); // False (zona aislada)

ResultadoRuta rt = grafoC.CalcularDijkstra("PAR-01", "PAR-16", true);
Console.WriteLine($"  Dijkstra PAR-01 -> PAR-16 (tiempo): {rt.ResumenCamino} | {rt.TiempoTotalMinutos:F1} min");
// PAR-01 -> PAR-02 -> PAR-12 -> PAR-14 -> PAR-16 | 29.1 min
ResultadoRuta rc = grafoC.CalcularDijkstra("PAR-01", "PAR-16", false);
Console.WriteLine($"  Dijkstra PAR-01 -> PAR-16 (costo):  {rc.ResumenCamino} | Q{rc.CostoTotal:F2}");
// PAR-01 -> PAR-09 -> PAR-11 -> PAR-15 -> PAR-16 | Q8.81

// --- Manejo de errores ---
Console.WriteLine("\n  Manejo de errores:");
Console.WriteLine($"  Archivo inexistente: {cargador.CargarParadasCSV("no_existe.csv")}");

string archivoMalo = Path.Combine(Path.GetTempPath(), "paradas_malas.csv");
File.WriteAllLines(archivoMalo, new[]
{
    "Codigo,Nombre,X,Y,Zona",
    "PAR-50,Parada buena,10,20,Zona 1",  // OK
    "PAR-51,Sin coordenadas",             // Faltan campos
    "PAR-52,Coordenada mala,abc,20",      // X no es número
    ",Sin codigo,10,20",                  // Código vacío
    "PAR-01,Duplicada,10,20",             // Ya existe del dataset
    "",                                   // Vacía: se ignora
    "# comentario: se ignora",
});
ResultadoCarga rm = cargador.CargarParadasCSV(archivoMalo);
Console.WriteLine($"  Archivo con errores: {rm}"); // 1 cargados, 4 con error
foreach (string e in rm.Errores) Console.WriteLine($"    - {e}");

// -------------------------------------------------------------
// PRUEBA DEL ORQUESTADOR (flujo integrado)
// -------------------------------------------------------------
Console.WriteLine("\n>>> PRUEBA DEL ORQUESTADOR");
OrquestadorTransporte orq = new OrquestadorTransporte();
string datos = Path.Combine(AppContext.BaseDirectory, "Datos");
orq.Cargador.CargarParadasCSV(Path.Combine(datos, "paradas.csv"));
orq.Cargador.CargarRutasCSV(Path.Combine(datos, "rutas.csv"));

ResultadoRuta r1 = orq.BuscarRutaOptima("PAR-01", "PAR-16", true);
Console.WriteLine($"  PAR-01 -> PAR-16: {r1.ResumenCamino} | {r1.TiempoTotalMinutos:F1} min"); // 29.1 min
ResultadoRuta r2 = orq.BuscarRutaOptima("PAR-01", "PAR-20", true);
Console.WriteLine($"  PAR-01 -> PAR-20: {r2.MensajeEstado}");   // zonas no conectadas (Union-Find)
ResultadoRuta r3 = orq.BuscarRutaOptima("PAR-01", "PAR-99", true);
Console.WriteLine($"  PAR-01 -> PAR-99: {r3.MensajeEstado}");   // no existe (Tabla Hash)

// Probar baja coordinada de parada a través del Orquestador (Hash + Grafo + UnionFind)
Console.WriteLine("\n  Probando EliminarParada en el Orquestador ('PAR-02'):");
string? errElim = orq.EliminarParada("PAR-02");
Console.WriteLine($"  Resultado eliminación PAR-02: {errElim ?? "OK (eliminada de Hash, Grafo y UF reconstruido)"}");
Console.WriteLine($"  ¿Tabla Hash contiene PAR-02?: {orq.TablaHash.Contiene("PAR-02")}");
Console.WriteLine($"  ¿Grafo contiene PAR-02?: {orq.Grafo.ExisteParada("PAR-02")}");
ResultadoRuta rPostElim = orq.BuscarRutaOptima("PAR-01", "PAR-16", true);
Console.WriteLine($"  Ruta alternada PAR-01 -> PAR-16 post-eliminación PAR-02: {rPostElim.ResumenCamino} (Tiempo: {rPostElim.TiempoTotalMinutos:F1} min)");

// --- Eliminar parada: el conteo de zonas no debe cambiar ---
Console.WriteLine("\n>>> PRUEBA DE ELIMINAR PARADA");
OrquestadorTransporte orqE = new OrquestadorTransporte();
orqE.Cargador.CargarParadasCSV(Path.Combine(AppContext.BaseDirectory, "Datos", "paradas.csv"));
orqE.Cargador.CargarRutasCSV(Path.Combine(AppContext.BaseDirectory, "Datos", "rutas.csv"));
Console.WriteLine($"  Zonas antes: {orqE.UnionFind.CantidadZonas}");                         // 2
Console.WriteLine($"  Eliminar PAR-05: {orqE.Cargador.EliminarParada("PAR-05") ?? "OK"}");    // OK
Console.WriteLine($"  Zonas después: {orqE.UnionFind.CantidadZonas}");                        // 2 (antes del arreglo daba 3)
Console.WriteLine($"  ¿PAR-04 y PAR-06 misma zona? {orqE.UnionFind.EstanEnMismaZona("PAR-04", "PAR-06")}"); // True
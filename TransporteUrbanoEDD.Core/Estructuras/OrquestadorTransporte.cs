// ============================================================
// I2: propuesta inicial del flujo · I1: integración con tipos reales
// Orquestador central: crea las 4 estructuras una sola vez y
// coordina el flujo Trie → Tabla Hash → Union-Find → Dijkstra.
// ============================================================

using TransporteUrbanoEDD.Core.Archivos;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Estructuras;

/// <summary>
/// Dueño de las estructuras del sistema. Se crea una vez (en el FormPrincipal)
/// y todas las pestañas usan las mismas instancias.
/// </summary>
public class OrquestadorTransporte
{
    public TablaHashParadas TablaHash { get; } // Búsqueda por código O(1)       (I1)
    public UnionFindZonas UnionFind { get; }   // Zonas conectadas O(α(n))        (I1)
    public TrieParadas Trie { get; }           // Autocompletado por nombre       (I2)
    public GrafoTransporte Grafo { get; }      // Rutas, BFS, DFS y Dijkstra      (I3)
    public CargadorCSV Cargador { get; }       // Registra en las 4 a la vez      (I1)

    public OrquestadorTransporte()
    {
        TablaHash = new TablaHashParadas();
        Grafo = new GrafoTransporte();
        UnionFind = new UnionFindZonas(16, TablaHash); // Vinculado a la Hash para consultar por código
        Trie = new TrieParadas();
        Cargador = new CargadorCSV(TablaHash, Grafo, UnionFind, Trie);
    }

    /// <summary>
    /// Flujo integrado del enunciado: valida existencia (Hash), luego zona (Union-Find)
    /// y solo entonces ejecuta Dijkstra (Grafo). Evita correr Dijkstra en rutas imposibles.
    /// </summary>
    public ResultadoRuta BuscarRutaOptima(string codOrigen, string codDestino, bool porTiempo)
    {
        // 1. Ambas paradas deben existir: O(1) promedio en la Tabla Hash
        if (!TablaHash.Contiene(codOrigen) || !TablaHash.Contiene(codDestino))
            return new ResultadoRuta { ExisteRuta = false, MensajeEstado = "Una de las paradas no existe." };

        // 2. Deben estar en la misma zona conectada: O(α(n)) en el Union-Find
        if (!UnionFind.EstanEnMismaZona(codOrigen, codDestino))
            return new ResultadoRuta { ExisteRuta = false, MensajeEstado = "Las paradas pertenecen a zonas no conectadas de la red." };

        // 3. Ruta óptima: Dijkstra O((V + E) log V)
        return Grafo.CalcularDijkstra(codOrigen, codDestino, porTiempo);
    }
}
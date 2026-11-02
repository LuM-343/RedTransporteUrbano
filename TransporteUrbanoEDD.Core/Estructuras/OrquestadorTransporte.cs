using TransporteUrbanoEDD.Core.Modelos;
namespace TransporteUrbanoEDD.Core.Estructuras;

public class OrquestadorTransporte
{
    public TrieParadas Trie { get; set; } = new TrieParadas();
    public required dynamic TablaHash { get; set; } // Reemplazar con TablaHashParadas de Dev 1
    public required dynamic UnionFind { get; set; } // Reemplazar con UnionFindZonas de Dev 1
    public required dynamic Grafo { get; set; }     // Reemplazar con GrafoTransporte de Dev 3

    public ResultadoRuta BuscarRutaOptima(string codOrigen, string codDestino, bool porTiempo)
    {
        // 1. Validar existencia en TablaHash
        if (!TablaHash.Contiene(codOrigen) || !TablaHash.Contiene(codDestino))
            return new ResultadoRuta { ExisteRuta = false, MensajeEstado = "Una de las paradas no existe." };

        // 2. Validar conectividad de zona antes de Dijkstra
        if (!UnionFind.EstanEnMismaZona(codOrigen, codDestino))
            return new ResultadoRuta { ExisteRuta = false, MensajeEstado = "Las paradas pertenecen a zonas no conectadas." };

        // 3. Ejecutar Dijkstra en el Grafo
        return Grafo.CalcularDijkstra(codOrigen, codDestino, porTiempo);
    }
}
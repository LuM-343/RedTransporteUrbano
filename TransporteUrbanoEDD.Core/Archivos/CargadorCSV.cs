// ============================================================
// I1
// Registro centralizado de paradas y rutas en las 4 estructuras
// (Tabla Hash, Grafo, Union-Find y Trie) y carga desde CSV.
// ============================================================

using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Archivos
{
    /// <summary>
    /// Resumen de una carga de archivo: cuántos registros entraron y qué líneas fallaron.
    /// </summary>
    public class ResultadoCarga
    {
        public bool ArchivoValido { get; set; } = true;                            // false si no se pudo abrir
        public int Cargados { get; set; }                                          // Registros agregados con éxito
        public ListaPropia<string> Errores { get; } = new ListaPropia<string>();   // Un mensaje por línea con error

        public override string ToString()
            => ArchivoValido
                ? $"{Cargados} cargados, {Errores.Count} con error"
                : $"Archivo no válido: {(Errores.Count > 0 ? Errores.Obtener(0) : "")}";
    }

    /// <summary>
    /// Registra paradas y rutas manteniendo sincronizadas las 4 estructuras.
    /// Lo usan la carga de CSV y el CRUD de la interfaz.
    /// </summary>
    public class CargadorCSV
    {
        private readonly TablaHashParadas tablaHash;
        private readonly GrafoTransporte grafo;
        private readonly UnionFindZonas unionFind;
        private readonly TrieParadas? trie; // Opcional: si es null, no se alimenta el autocompletado

        public CargadorCSV(TablaHashParadas tablaHash, GrafoTransporte grafo,
                           UnionFindZonas unionFind, TrieParadas? trie = null)
        {
            this.tablaHash = tablaHash;
            this.grafo = grafo;
            this.unionFind = unionFind;
            this.trie = trie;
        }

        // ---------------------------------------------------------
        // REGISTRO EN LAS 4 ESTRUCTURAS
        // ---------------------------------------------------------

        /// <summary>
        /// Registra una parada en Grafo, Tabla Hash, Union-Find y Trie, y le asigna su Indice.
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? RegistrarParada(Parada parada)
        {
            if (parada == null)
                return "La parada es nula";

            // 1. El código es clave única
            if (tablaHash.Contiene(parada.Codigo))
                return $"El código '{parada.Codigo}' ya existe";

            // 2. Grafo primero: su posición interna será el Indice de la parada
            if (!grafo.AgregarParada(parada))
                return $"El código '{parada.Codigo}' ya existe en el grafo";

            // 3. Tabla Hash
            tablaHash.Insertar(parada);

            // 4. Indice = posición en el grafo (el grafo numera 0, 1, 2… en orden de inserción)
            parada.Indice = grafo.CantidadParadas - 1;

            // 5. Union-Find: también numera en orden, así que debe dar el mismo número
            int indiceUF = unionFind.AgregarElemento();
            if (indiceUF != parada.Indice)
                throw new InvalidOperationException(
                    $"Grafo ({parada.Indice}) y Union-Find ({indiceUF}) desincronizados");

            // 6. Trie (autocompletado por nombre)
            trie?.Insertar(parada.Nombre, parada.Codigo); // ?. = solo si trie no es null

            return null; // Éxito
        }

        /// <summary>
        /// Registra una ruta en el Grafo y une sus paradas en el Union-Find (misma zona).
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? RegistrarRuta(string codOrigen, string codDestino,
                                     double tiempo, double costo, bool bidireccional)
        {
            // 1. Ambas paradas deben existir (búsqueda O(1) en la Tabla Hash)
            Parada? origen = tablaHash.Buscar(codOrigen);
            Parada? destino = tablaHash.Buscar(codDestino);

            if (origen == null)
                return $"La parada de origen '{codOrigen}' no existe";
            if (destino == null)
                return $"La parada de destino '{codDestino}' no existe";
            if (origen.Codigo == destino.Codigo)
                return "El origen y el destino no pueden ser la misma parada";
            if (tiempo < 0 || costo < 0)
                return "El tiempo y el costo no pueden ser negativos";

            // 2. Grafo: crea la arista (y la de regreso si es bidireccional)
            grafo.AgregarRuta(origen.Codigo, destino.Codigo, tiempo, costo, bidireccional);

            // 3. Union-Find: quedan en la misma zona (ignora la dirección de la ruta)
            unionFind.Union(origen.Indice, destino.Indice);

            return null; // Éxito
        }
    }
}
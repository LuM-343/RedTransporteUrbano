using System;
using System.Collections.Generic; // Exclusivamente permitido para Queue<T> en BFS
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Estructura de Grafo para la Red de Transporte Urbano.
    /// Implementado con listas de adyacencia utilizando la lista enlazada propia (ListaPropia<T>).
    /// Soporta aristas dirigidas y bidireccionales, doble ponderación (tiempo y costo),
    /// y algoritmos de recorrido BFS y DFS.
    /// Desarrollado por Dev 3.
    /// </summary>
    public class GrafoTransporte
    {
        private Parada[] vertices;
        private ListaPropia<AristaRuta>[] adyacencias;
        private int numVertices;

        public int CantidadParadas => numVertices;
        public int Capacidad => vertices.Length;

        public GrafoTransporte(int capacidadInicial = 16)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 16;

            vertices = new Parada[capacidadInicial];
            adyacencias = new ListaPropia<AristaRuta>[capacidadInicial];
            numVertices = 0;
        }

        /// <summary>
        /// Agrega una parada al grafo si no existe previamente por su código.
        /// </summary>
        /// <param name="parada">Instancia de la parada a agregar.</param>
        /// <returns>True si fue agregada; false si ya existía.</returns>
        public bool AgregarParada(Parada parada)
        {
            if (parada == null)
                throw new ArgumentNullException(nameof(parada));

            if (ObtenerIndiceParada(parada.Codigo) != -1)
                return false; // Ya existe

            if (numVertices == vertices.Length)
            {
                Redimensionar();
            }

            vertices[numVertices] = parada;
            adyacencias[numVertices] = new ListaPropia<AristaRuta>();
            numVertices++;
            return true;
        }

        /// <summary>
        /// Registra una ruta/arista entre dos paradas existentes.
        /// </summary>
        /// <param name="codigoOrigen">Código de la parada origen.</param>
        /// <param name="codigoDestino">Código de la parada destino.</param>
        /// <param name="tiempo">Tiempo de traslado en minutos.</param>
        /// <param name="costo">Costo del pasaje en quetzales/moneda.</param>
        /// <param name="esBidireccional">Indica si los buses transitan en ambos sentidos.</param>
        public void AgregarRuta(string codigoOrigen, string codigoDestino, double tiempo, double costo, bool esBidireccional = false)
        {
            int u = ObtenerIndiceParada(codigoOrigen);
            int v = ObtenerIndiceParada(codigoDestino);

            if (u == -1)
                throw new ArgumentException($"La parada de origen con código '{codigoOrigen}' no existe en el grafo.", nameof(codigoOrigen));
            if (v == -1)
                throw new ArgumentException($"La parada de destino con código '{codigoDestino}' no existe en el grafo.", nameof(codigoDestino));

            AristaRuta rutaIda = new AristaRuta(vertices[u], vertices[v], tiempo, costo, esBidireccional);
            adyacencias[u].Agregar(rutaIda);

            if (esBidireccional)
            {
                AristaRuta rutaVuelta = new AristaRuta(vertices[v], vertices[u], tiempo, costo, esBidireccional);
                adyacencias[v].Agregar(rutaVuelta);
            }
        }

        /// <summary>
        /// Sobrecarga para agregar ruta usando directamente las instancias de Parada.
        /// </summary>
        public void AgregarRuta(Parada origen, Parada destino, double tiempo, double costo, bool esBidireccional = false)
        {
            if (origen == null) throw new ArgumentNullException(nameof(origen));
            if (destino == null) throw new ArgumentNullException(nameof(destino));

            AgregarRuta(origen.Codigo, destino.Codigo, tiempo, costo, esBidireccional);
        }

        /// <summary>
        /// Obtiene el índice numérico interno de una parada por su código.
        /// </summary>
        public int ObtenerIndiceParada(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return -1;

            string buscado = codigo.Trim();
            for (int i = 0; i < numVertices; i++)
            {
                if (string.Equals(vertices[i].Codigo, buscado, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Obtiene una parada a partir de su código único.
        /// </summary>
        public Parada? ObtenerParadaPorCodigo(string codigo)
        {
            int idx = ObtenerIndiceParada(codigo);
            return idx != -1 ? vertices[idx] : null;
        }

        /// <summary>
        /// Obtiene una parada por su índice interno.
        /// </summary>
        public Parada ObtenerParadaPorIndice(int indice)
        {
            if (indice < 0 || indice >= numVertices)
                throw new ArgumentOutOfRangeException(nameof(indice), "Índice de parada fuera de rango.");

            return vertices[indice];
        }

        /// <summary>
        /// Retorna la lista enlazada propia con las rutas salientes de la parada indicada.
        /// </summary>
        public ListaPropia<AristaRuta> ObtenerRutasSalientes(string codigoParada)
        {
            int idx = ObtenerIndiceParada(codigoParada);
            if (idx == -1)
                throw new ArgumentException($"La parada '{codigoParada}' no existe en el grafo.", nameof(codigoParada));

            return adyacencias[idx];
        }

        /// <summary>
        /// Retorna la lista de adyacencias directa por índice interno (útil para algoritmos como Dijkstra).
        /// </summary>
        public ListaPropia<AristaRuta> ObtenerAdyacenciasPorIndice(int indice)
        {
            if (indice < 0 || indice >= numVertices)
                throw new ArgumentOutOfRangeException(nameof(indice), "Índice de vértice inválido.");

            return adyacencias[indice];
        }

        /// <summary>
        /// Retorna todas las paradas registradas en el grafo en una ListaPropia.
        /// </summary>
        public ListaPropia<Parada> ObtenerTodasLasParadas()
        {
            ListaPropia<Parada> lista = new ListaPropia<Parada>();
            for (int i = 0; i < numVertices; i++)
            {
                lista.Agregar(vertices[i]);
            }
            return lista;
        }

        /// <summary>
        /// Retorna todas las rutas registradas en el grafo.
        /// </summary>
        public ListaPropia<AristaRuta> ObtenerTodasLasRutas()
        {
            ListaPropia<AristaRuta> todas = new ListaPropia<AristaRuta>();
            for (int i = 0; i < numVertices; i++)
            {
                foreach (AristaRuta ruta in adyacencias[i])
                {
                    todas.Agregar(ruta);
                }
            }
            return todas;
        }

        #region Recorridos: BFS y DFS

        /// <summary>
        /// Recorrido en Anchura (Breadth-First Search).
        /// Explora la red nivel por nivel desde la parada indicada.
        /// Cumple estrictamente la regla del proyecto: usa Queue de System.Collections.Generic
        /// única y exclusivamente como estructura de control de orden de visita en BFS.
        /// </summary>
        /// <param name="codigoInicio">Código de la parada de inicio.</param>
        /// <returns>Lista enlazada propia con las paradas en orden de visita BFS.</returns>
        public ListaPropia<Parada> RecorridoBFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceParada(codigoInicio);
            if (inicio == -1)
                throw new ArgumentException($"La parada inicial '{codigoInicio}' no existe en el grafo.", nameof(codigoInicio));

            ListaPropia<Parada> ordenVisita = new ListaPropia<Parada>();
            bool[] visitados = new bool[numVertices];
            Queue<int> cola = new Queue<int>(); // Única excepción legal del proyecto

            cola.Enqueue(inicio);
            visitados[inicio] = true;

            while (cola.Count > 0)
            {
                int actual = cola.Dequeue();
                ordenVisita.Agregar(vertices[actual]);

                foreach (AristaRuta arista in adyacencias[actual])
                {
                    int vecino = ObtenerIndiceParada(arista.Destino.Codigo);
                    if (vecino != -1 && !visitados[vecino])
                    {
                        visitados[vecino] = true;
                        cola.Enqueue(vecino);
                    }
                }
            }

            return ordenVisita;
        }

        /// <summary>
        /// Recorrido en Profundidad (Depth-First Search).
        /// Explora la red profundizando en cada ramal antes de retroceder.
        /// Implementado de forma recursiva sin colecciones prohibidas.
        /// </summary>
        /// <param name="codigoInicio">Código de la parada de inicio.</param>
        /// <returns>Lista enlazada propia con las paradas en orden de visita DFS.</returns>
        public ListaPropia<Parada> RecorridoDFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceParada(codigoInicio);
            if (inicio == -1)
                throw new ArgumentException($"La parada inicial '{codigoInicio}' no existe en el grafo.", nameof(codigoInicio));

            ListaPropia<Parada> ordenVisita = new ListaPropia<Parada>();
            bool[] visitados = new bool[numVertices];

            EjecutarDfsRecursivo(inicio, visitados, ordenVisita);

            return ordenVisita;
        }

        private void EjecutarDfsRecursivo(int actual, bool[] visitados, ListaPropia<Parada> ordenVisita)
        {
            visitados[actual] = true;
            ordenVisita.Agregar(vertices[actual]);

            foreach (AristaRuta arista in adyacencias[actual])
            {
                int vecino = ObtenerIndiceParada(arista.Destino.Codigo);
                if (vecino != -1 && !visitados[vecino])
                {
                    EjecutarDfsRecursivo(vecino, visitados, ordenVisita);
                }
            }
        }

        #endregion

        /// <summary>
        /// Duplica el tamaño de los arreglos internos cuando se supera la capacidad.
        /// </summary>
        private void Redimensionar()
        {
            int nuevaCapacidad = vertices.Length * 2;
            Parada[] nuevosVertices = new Parada[nuevaCapacidad];
            ListaPropia<AristaRuta>[] nuevasAdyacencias = new ListaPropia<AristaRuta>[nuevaCapacidad];

            for (int i = 0; i < numVertices; i++)
            {
                nuevosVertices[i] = vertices[i];
                nuevasAdyacencias[i] = adyacencias[i];
            }

            vertices = nuevosVertices;
            adyacencias = nuevasAdyacencias;
        }
    }
}

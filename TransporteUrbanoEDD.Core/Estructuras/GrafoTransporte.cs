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

        #region Algoritmo de Dijkstra

        /// <summary>
        /// Calcula la ruta óptima entre dos paradas utilizando el Algoritmo de Dijkstra con MinHeapPropio.
        /// Cumple estrictamente la regla de no usar List, PriorityQueue, Stack ni LINQ.
        /// </summary>
        /// <param name="codOrigen">Código de la parada de origen.</param>
        /// <param name="codDestino">Código de la parada de destino.</param>
        /// <param name="optimizarPorTiempo">True para minimizar tiempo de viaje (minutos); False para minimizar costo de pasaje (quetzales).</param>
        /// <returns>Objeto ResultadoRuta con los datos y la secuencia de paradas de la ruta ganadora.</returns>
        public ResultadoRuta CalcularDijkstra(string codOrigen, string codDestino, bool optimizarPorTiempo)
        {
            ResultadoRuta resultado = new ResultadoRuta();

            int uOrigen = ObtenerIndiceParada(codOrigen);
            int uDestino = ObtenerIndiceParada(codDestino);

            if (uOrigen == -1)
            {
                resultado.ExisteRuta = false;
                resultado.MensajeEstado = $"La parada de origen '{codOrigen}' no existe en el sistema.";
                return resultado;
            }

            if (uDestino == -1)
            {
                resultado.ExisteRuta = false;
                resultado.MensajeEstado = $"La parada de destino '{codDestino}' no existe en el sistema.";
                return resultado;
            }

            // Caso base: El origen y el destino son idénticos
            if (uOrigen == uDestino)
            {
                resultado.ExisteRuta = true;
                resultado.MensajeEstado = "El origen y el destino corresponden a la misma parada.";
                resultado.ResumenCamino = vertices[uOrigen].Codigo;
                resultado.CostoTotal = 0.0;
                resultado.TiempoTotalMinutos = 0.0;
                resultado.SecuenciaCodigosParadas.Agregar(vertices[uOrigen].Codigo);
                return resultado;
            }

            int n = numVertices;
            double[] mejorMetrica = new double[n];
            double[] costoAcumulado = new double[n];
            double[] tiempoAcumulado = new double[n];
            int[] predecesor = new int[n];
            bool[] visitado = new bool[n];

            for (int i = 0; i < n; i++)
            {
                mejorMetrica[i] = double.MaxValue;
                costoAcumulado[i] = double.MaxValue;
                tiempoAcumulado[i] = double.MaxValue;
                predecesor[i] = -1;
                visitado[i] = false;
            }

            mejorMetrica[uOrigen] = 0.0;
            costoAcumulado[uOrigen] = 0.0;
            tiempoAcumulado[uOrigen] = 0.0;

            MinHeapPropio<int> heap = new MinHeapPropio<int>();
            heap.Insertar(uOrigen, 0.0);

            while (!heap.EstaVacio)
            {
                NodoHeap<int> nodoMin = heap.ExtraerMinimo();
                int u = nodoMin.Elemento;
                double metricaActual = nodoMin.Prioridad;

                if (metricaActual > mejorMetrica[u])
                    continue;

                visitado[u] = true;

                if (u == uDestino)
                    break; // Destino alcanzado de forma óptima

                foreach (AristaRuta arista in adyacencias[u])
                {
                    int v = ObtenerIndiceParada(arista.Destino.Codigo);
                    if (v == -1 || visitado[v])
                        continue;

                    double pesoEvaluado = optimizarPorTiempo ? arista.Tiempo : arista.Costo;
                    double nuevaMetrica = mejorMetrica[u] + pesoEvaluado;

                    if (nuevaMetrica < mejorMetrica[v])
                    {
                        mejorMetrica[v] = nuevaMetrica;
                        tiempoAcumulado[v] = tiempoAcumulado[u] + arista.Tiempo;
                        costoAcumulado[v] = costoAcumulado[u] + arista.Costo;
                        predecesor[v] = u;

                        heap.Insertar(v, nuevaMetrica);
                    }
                }
            }

            // Validar si el destino fue alcanzable
            if (mejorMetrica[uDestino] == double.MaxValue || predecesor[uDestino] == -1)
            {
                resultado.ExisteRuta = false;
                resultado.MensajeEstado = $"No existe una ruta disponible entre '{codOrigen}' y '{codDestino}'. El sentido de las vías o la falta de conectividad impiden llegar al destino.";
                return resultado;
            }

            // Reconstruir el camino desde destino hasta origen usando arreglo primitivo temporal int[]
            // (REGLA CRÍTICA: SIN usar Stack<T> ni List.Reverse())
            int[] caminoInverso = new int[n];
            int cantidadNodosCamino = 0;
            int actual = uDestino;

            while (actual != -1)
            {
                caminoInverso[cantidadNodosCamino] = actual;
                cantidadNodosCamino++;
                actual = predecesor[actual];
            }

            resultado.ExisteRuta = true;
            resultado.MensajeEstado = "Ruta óptima calculada exitosamente.";
            resultado.CostoTotal = costoAcumulado[uDestino];
            resultado.TiempoTotalMinutos = tiempoAcumulado[uDestino];

            // Recorrer el arreglo primitivo de atrás hacia adelante (de origen a destino)
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = cantidadNodosCamino - 1; i >= 0; i--)
            {
                int indiceVertice = caminoInverso[i];
                string codigoParada = vertices[indiceVertice].Codigo;

                resultado.SecuenciaCodigosParadas.Agregar(codigoParada);

                if (sb.Length > 0)
                {
                    sb.Append(" -> ");
                }
                sb.Append(codigoParada);
            }

            resultado.ResumenCamino = sb.ToString();
            return resultado;
        }

        /// <summary>
        /// Método puente para integración con OrquestadorTransporte (Dev 2).
        /// NOTA PARA DEV 2: En OrquestadorTransporte, invoque primero UnionFindZonas.EstanConectadas(origen, destino)
        /// antes de llamar a este método para cumplir con el requisito de integración coordinada entre estructuras.
        /// </summary>
        public ResultadoRuta ObtenerMejorRuta(string codOrigen, string codDestino, bool optimizarPorTiempo)
        {
            return CalcularDijkstra(codOrigen, codDestino, optimizarPorTiempo);
        }

        /// <summary>
        /// Verifica si una parada existe en el grafo mediante su código.
        /// </summary>
        public bool ExisteParada(string codigo)
        {
            return ObtenerIndiceParada(codigo) != -1;
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

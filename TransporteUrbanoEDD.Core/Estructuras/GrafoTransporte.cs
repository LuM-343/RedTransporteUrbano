using System;
using System.Collections.Generic; // Exclusivamente permitido para Queue<T> en BFS
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Estructura de Grafo para la Red de Transporte Urbano.
    /// Implementado con listas de adyacencia utilizando la lista enlazada propia (ListaPropia<T>).
    /// Soporta aristas dirigidas y bidireccionales, doble ponderación (tiempo y costo),
    /// algoritmos de recorrido BFS y DFS, y cálculo de rutas óptimas con Dijkstra.
    /// Incorpora eliminación de paradas y rutas mediante la técnica de "Baja del Vértice + Purga Total de Aristas",
    /// preservando estrictamente la estabilidad de los identificadores para sincronización con Union-Find.
    /// Desarrollado por Dev 3.
    /// </summary>
    public class GrafoTransporte
    {
        private Parada?[] vertices;
        private ListaPropia<AristaRuta>[] adyacencias;
        private int numVertices;

        /// <summary>
        /// Total de índices/posiciones reservadas en el grafo (0 .. CantidadParadas - 1).
        /// Mantiene la sincronización de identificadores con estructuras como UnionFindZonas.
        /// </summary>
        public int CantidadParadas => numVertices;

        /// <summary>
        /// Número de paradas actualmente activas en el grafo (excluyendo las dadas de baja).
        /// </summary>
        public int CantidadParadasActivas
        {
            get
            {
                int activas = 0;
                for (int i = 0; i < numVertices; i++)
                {
                    if (vertices[i] != null)
                    {
                        activas++;
                    }
                }
                return activas;
            }
        }

        public int Capacidad => vertices.Length;

        public GrafoTransporte(int capacidadInicial = 16)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 16;

            vertices = new Parada?[capacidadInicial];
            adyacencias = new ListaPropia<AristaRuta>[capacidadInicial];
            numVertices = 0;
        }

        /// <summary>
        /// Agrega una parada al grafo si no existe previamente por su código.
        /// </summary>
        /// <param name="parada">Instancia de la parada a agregar.</param>
        /// <returns>True si fue agregada; false si ya existía activa.</returns>
        public bool AgregarParada(Parada parada)
        {
            if (parada == null)
                throw new ArgumentNullException(nameof(parada));

            if (ObtenerIndiceParada(parada.Codigo) != -1)
                return false; // Ya existe activo

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

            if (u == -1 || vertices[u] == null)
                throw new ArgumentException($"La parada de origen con código '{codigoOrigen}' no existe en el grafo o está inactiva.", nameof(codigoOrigen));
            if (v == -1 || vertices[v] == null)
                throw new ArgumentException($"La parada de destino con código '{codigoDestino}' no existe en el grafo o está inactiva.", nameof(codigoDestino));

            AristaRuta rutaIda = new AristaRuta(vertices[u]!, vertices[v]!, tiempo, costo, esBidireccional);
            adyacencias[u].Agregar(rutaIda);

            if (esBidireccional)
            {
                AristaRuta rutaVuelta = new AristaRuta(vertices[v]!, vertices[u]!, tiempo, costo, esBidireccional);
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
        /// Retorna -1 si no existe o si fue dada de baja (posición null).
        /// Valida de forma segura codigosParadas[i] != null para evitar NullReferenceException.
        /// </summary>
        public int ObtenerIndiceParada(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return -1;

            string buscado = codigo.Trim();
            for (int i = 0; i < numVertices; i++)
            {
                if (vertices[i] != null && !string.IsNullOrWhiteSpace(vertices[i]!.Codigo))
                {
                    if (string.Equals(vertices[i]!.Codigo, buscado, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Alias requerido por integración y especificación técnica:
        /// Obtiene el identificador numérico interno del vértice a partir de su código.
        /// </summary>
        public int ObtenerIdVertice(string codigoParada)
        {
            return ObtenerIndiceParada(codigoParada);
        }

        /// <summary>
        /// Obtiene una parada a partir de su código único. Retorna null si no existe o fue dada de baja.
        /// </summary>
        public Parada? ObtenerParadaPorCodigo(string codigo)
        {
            int idx = ObtenerIndiceParada(codigo);
            return idx != -1 ? vertices[idx] : null;
        }

        /// <summary>
        /// Obtiene una parada por su índice interno. Retorna null si la posición fue dada de baja.
        /// </summary>
        public Parada? ObtenerParadaPorIndice(int indice)
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
            if (idx == -1 || vertices[idx] == null)
                throw new ArgumentException($"La parada '{codigoParada}' no existe en el grafo o está inactiva.", nameof(codigoParada));

            return adyacencias[idx];
        }

        /// <summary>
        /// Retorna la lista de adyacencias directa por índice interno.
        /// Retorna una lista vacía si el vértice fue dado de baja.
        /// </summary>
        public ListaPropia<AristaRuta> ObtenerAdyacenciasPorIndice(int indice)
        {
            if (indice < 0 || indice >= numVertices)
                throw new ArgumentOutOfRangeException(nameof(indice), "Índice de vértice inválido.");

            if (vertices[indice] == null || adyacencias[indice] == null)
                return new ListaPropia<AristaRuta>();

            return adyacencias[indice];
        }

        /// <summary>
        /// Retorna todas las paradas activas registradas en el grafo en una ListaPropia.
        /// </summary>
        public ListaPropia<Parada> ObtenerTodasLasParadas()
        {
            ListaPropia<Parada> lista = new ListaPropia<Parada>();
            for (int i = 0; i < numVertices; i++)
            {
                if (vertices[i] != null)
                {
                    lista.Agregar(vertices[i]!);
                }
            }
            return lista;
        }

        /// <summary>
        /// Retorna todas las rutas activas registradas en el grafo.
        /// </summary>
        public ListaPropia<AristaRuta> ObtenerTodasLasRutas()
        {
            ListaPropia<AristaRuta> todas = new ListaPropia<AristaRuta>();
            for (int i = 0; i < numVertices; i++)
            {
                if (vertices[i] != null && adyacencias[i] != null)
                {
                    foreach (AristaRuta ruta in adyacencias[i])
                    {
                        if (ruta != null && ruta.Destino != null && ObtenerIndiceParada(ruta.Destino.Codigo) != -1)
                        {
                            todas.Agregar(ruta);
                        }
                    }
                }
            }
            return todas;
        }

        #region Eliminación de Paradas y Rutas (Baja de Vértice + Purga Total)

        /// <summary>
        /// Da de baja una parada en el sistema aplicando la estrategia:
        /// "Baja del Vértice + Purga Total de Aristas", sin desplazar ni reindexar
        /// los índices del arreglo para no desfasar los identificadores sincronizados con UnionFindZonas.
        /// 
        /// Pasos ejecutados:
        /// 1. Busca idEliminar = ObtenerIdVertice(codigoParada). Si es -1, retorna false.
        /// 2. Paso A (Aristas Salientes): Vacía o reinicia con una nueva instancia vacía adyacencias[idEliminar].
        /// 3. Paso B (Aristas Entrantes): Recorre todas las listas de adyacencia de los demás vértices activos
        ///    y purga cualquier arista cuyo destino coincida con idEliminar manipulando punteros (Cabeza, Siguiente, Cola).
        /// 4. Paso C (Baja del Vértice): Marca la posición en el arreglo vertices[idEliminar] = null.
        /// </summary>
        /// <param name="codigoParada">Código único de la parada a dar de baja.</param>
        /// <returns>True si la parada existía y fue dada de baja con éxito; false si no existía o ya estaba inactiva.</returns>
        public bool EliminarParada(string codigoParada)
        {
            if (string.IsNullOrWhiteSpace(codigoParada))
                return false;

            int idEliminar = ObtenerIdVertice(codigoParada);
            if (idEliminar == -1 || vertices[idEliminar] == null)
                return false;

            string codEliminar = vertices[idEliminar]!.Codigo;

            // Paso A (Aristas Salientes): Vacía o reinicia con una nueva instancia vacía la lista de adyacencia de idEliminar
            adyacencias[idEliminar] = new ListaPropia<AristaRuta>();

            // Paso B (Aristas Entrantes): Recorre todas las listas de adyacencia de todos los demás vértices activos (0 hasta CantidadParadas - 1)
            // y elimina cualquier nodo/arista cuyo destino coincida con idEliminar manipulando punteros en Cabeza o intermedios.
            for (int i = 0; i < numVertices; i++)
            {
                if (i == idEliminar || vertices[i] == null || adyacencias[i] == null)
                    continue;

                EliminarAristasHaciaDestino(adyacencias[i], codEliminar);
            }

            // Paso C (Baja del Vértice): Marca la posición en el arreglo como null
            vertices[idEliminar] = null;

            return true;
        }

        /// <summary>
        /// Elimina de forma precisa una ruta/conexión entre dos paradas específicas sin dar de baja las paradas.
        /// Manipula directamente los punteros de la lista enlazada propia (Cabeza, Siguiente, Cola).
        /// </summary>
        /// <param name="codOrigen">Código de la parada de origen.</param>
        /// <param name="codDestino">Código de la parada de destino.</param>
        /// <param name="esDobleVia">True para eliminar la conexión en ambos sentidos; False para solo la vía de ida.</param>
        /// <returns>True si se eliminó al menos una conexión; false si alguna parada no existe o no estaban conectadas.</returns>
        public bool EliminarRuta(string codOrigen, string codDestino, bool esDobleVia = true)
        {
            if (string.IsNullOrWhiteSpace(codOrigen) || string.IsNullOrWhiteSpace(codDestino))
                return false;

            int u = ObtenerIndiceParada(codOrigen);
            int v = ObtenerIndiceParada(codDestino);

            if (u == -1 || v == -1 || vertices[u] == null || vertices[v] == null)
                return false;

            bool eliminadaIda = EliminarAristaSimple(adyacencias[u], codDestino);
            bool eliminadaVuelta = false;

            if (esDobleVia)
            {
                eliminadaVuelta = EliminarAristaSimple(adyacencias[v], codOrigen);
            }

            return eliminadaIda || eliminadaVuelta;
        }

        /// <summary>
        /// Sobrecarga para eliminar ruta utilizando instancias de Parada.
        /// </summary>
        public bool EliminarRuta(Parada origen, Parada destino, bool esDobleVia = true)
        {
            if (origen == null || destino == null)
                return false;

            return EliminarRuta(origen.Codigo, destino.Codigo, esDobleVia);
        }

        /// <summary>
        /// Verifica si existe una conexión directa desde una parada origen hacia una parada destino.
        /// </summary>
        public bool ExisteRuta(string codOrigen, string codDestino)
        {
            int u = ObtenerIndiceParada(codOrigen);
            if (u == -1 || vertices[u] == null || adyacencias[u] == null)
                return false;

            foreach (AristaRuta arista in adyacencias[u])
            {
                if (arista != null && arista.Destino != null &&
                    string.Equals(arista.Destino.Codigo, codDestino, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Purga todas las aristas de una lista enlazada de adyacencia que se dirijan hacia codigoDestino,
        /// manipulando directamente los punteros de los nodos (Cabeza, Siguiente, Cola),
        /// manejando correctamente tanto el caso en que la arista esté en la Cabeza como en nodos intermedios.
        /// </summary>
        private bool EliminarAristasHaciaDestino(ListaPropia<AristaRuta> lista, string codigoDestino)
        {
            if (lista == null || lista.Cabeza == null)
                return false;

            bool seElimino = false;
            NodoLista<AristaRuta>? anterior = null;
            NodoLista<AristaRuta>? actual = lista.Cabeza;

            while (actual != null)
            {
                bool coincide = actual.Dato != null &&
                                actual.Dato.Destino != null &&
                                string.Equals(actual.Dato.Destino.Codigo, codigoDestino, StringComparison.OrdinalIgnoreCase);

                if (coincide)
                {
                    NodoLista<AristaRuta> nodoAEliminar = actual;

                    if (anterior == null) // Caso: La arista a borrar está en la Cabeza
                    {
                        lista.Cabeza = actual.Siguiente;
                        actual = lista.Cabeza;
                    }
                    else // Caso: La arista está en un nodo intermedio o final
                    {
                        anterior.Siguiente = actual.Siguiente;
                        actual = anterior.Siguiente;
                    }

                    if (nodoAEliminar == lista.Cola)
                    {
                        lista.Cola = anterior;
                    }

                    lista.DecrementarContador();
                    seElimino = true;
                }
                else
                {
                    anterior = actual;
                    actual = actual.Siguiente;
                }
            }

            if (lista.Cabeza == null)
            {
                lista.Cola = null;
            }

            return seElimino;
        }

        /// <summary>
        /// Elimina una única arista dirigida hacia codigoDestino manipulando punteros de la lista enlazada propia.
        /// </summary>
        private bool EliminarAristaSimple(ListaPropia<AristaRuta> lista, string codigoDestino)
        {
            if (lista == null || lista.Cabeza == null)
                return false;

            NodoLista<AristaRuta>? anterior = null;
            NodoLista<AristaRuta>? actual = lista.Cabeza;

            while (actual != null)
            {
                if (actual.Dato != null &&
                    actual.Dato.Destino != null &&
                    string.Equals(actual.Dato.Destino.Codigo, codigoDestino, StringComparison.OrdinalIgnoreCase))
                {
                    NodoLista<AristaRuta> nodoAEliminar = actual;

                    if (anterior == null) // Caso: Arista en la Cabeza
                    {
                        lista.Cabeza = actual.Siguiente;
                    }
                    else // Caso: Arista en nodo intermedio o Cola
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }

                    if (nodoAEliminar == lista.Cola)
                    {
                        lista.Cola = anterior;
                    }

                    if (lista.Cabeza == null)
                    {
                        lista.Cola = null;
                    }

                    lista.DecrementarContador();
                    return true;
                }

                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        #endregion

        #region Recorridos: BFS y DFS

        /// <summary>
        /// Recorrido en Anchura (Breadth-First Search).
        /// Explora la red nivel por nivel desde la parada indicada.
        /// Ignora de forma segura cualquier vértice nulo o dado de baja.
        /// Usa Queue de System.Collections.Generic exclusivamente como estructura de control de orden en BFS.
        /// </summary>
        /// <param name="codigoInicio">Código de la parada de inicio.</param>
        /// <returns>Lista enlazada propia con las paradas en orden de visita BFS.</returns>
        public ListaPropia<Parada> RecorridoBFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceParada(codigoInicio);
            if (inicio == -1 || vertices[inicio] == null)
                throw new ArgumentException($"La parada inicial '{codigoInicio}' no existe en el grafo o está inactiva.", nameof(codigoInicio));

            ListaPropia<Parada> ordenVisita = new ListaPropia<Parada>();
            bool[] visitados = new bool[numVertices];
            Queue<int> cola = new Queue<int>(); // Única excepción legal del proyecto

            cola.Enqueue(inicio);
            visitados[inicio] = true;

            while (cola.Count > 0)
            {
                int actual = cola.Dequeue();
                if (vertices[actual] == null)
                    continue;

                ordenVisita.Agregar(vertices[actual]!);

                if (adyacencias[actual] != null)
                {
                    foreach (AristaRuta arista in adyacencias[actual])
                    {
                        if (arista == null || arista.Destino == null)
                            continue;

                        int vecino = ObtenerIndiceParada(arista.Destino.Codigo);
                        if (vecino != -1 && vertices[vecino] != null && !visitados[vecino])
                        {
                            visitados[vecino] = true;
                            cola.Enqueue(vecino);
                        }
                    }
                }
            }

            return ordenVisita;
        }

        /// <summary>
        /// Alias para el recorrido BFS cumpliendo con los estándares de nomenclatura de integración.
        /// </summary>
        public ListaPropia<Parada> EjecutarBFS(string codigoInicio) => RecorridoBFS(codigoInicio);

        /// <summary>
        /// Recorrido en Profundidad (Depth-First Search).
        /// Explora la red profundizando en cada ramal antes de retroceder.
        /// Ignora de forma segura cualquier vértice nulo o dado de baja.
        /// </summary>
        /// <param name="codigoInicio">Código de la parada de inicio.</param>
        /// <returns>Lista enlazada propia con las paradas en orden de visita DFS.</returns>
        public ListaPropia<Parada> RecorridoDFS(string codigoInicio)
        {
            int inicio = ObtenerIndiceParada(codigoInicio);
            if (inicio == -1 || vertices[inicio] == null)
                throw new ArgumentException($"La parada inicial '{codigoInicio}' no existe en el grafo o está inactiva.", nameof(codigoInicio));

            ListaPropia<Parada> ordenVisita = new ListaPropia<Parada>();
            bool[] visitados = new bool[numVertices];

            EjecutarDfsRecursivo(inicio, visitados, ordenVisita);

            return ordenVisita;
        }

        /// <summary>
        /// Alias para el recorrido DFS cumpliendo con los estándares de nomenclatura de integración.
        /// </summary>
        public ListaPropia<Parada> EjecutarDFS(string codigoInicio) => RecorridoDFS(codigoInicio);

        private void EjecutarDfsRecursivo(int actual, bool[] visitados, ListaPropia<Parada> ordenVisita)
        {
            if (actual < 0 || actual >= numVertices || vertices[actual] == null)
                return;

            visitados[actual] = true;
            ordenVisita.Agregar(vertices[actual]!);

            if (adyacencias[actual] != null)
            {
                foreach (AristaRuta arista in adyacencias[actual])
                {
                    if (arista == null || arista.Destino == null)
                        continue;

                    int vecino = ObtenerIndiceParada(arista.Destino.Codigo);
                    if (vecino != -1 && vertices[vecino] != null && !visitados[vecino])
                    {
                        EjecutarDfsRecursivo(vecino, visitados, ordenVisita);
                    }
                }
            }
        }

        #endregion

        #region Algoritmo de Dijkstra

        /// <summary>
        /// Calcula la ruta óptima entre dos paradas utilizando el Algoritmo de Dijkstra con MinHeapPropio.
        /// Cumple estrictamente la regla de no usar List, PriorityQueue, Stack ni LINQ.
        /// Ignora de forma segura cualquier vértice nulo o dado de baja.
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

            if (uOrigen == -1 || vertices[uOrigen] == null)
            {
                resultado.ExisteRuta = false;
                resultado.MensajeEstado = $"La parada de origen '{codOrigen}' no existe en el sistema o fue dada de baja.";
                return resultado;
            }

            if (uDestino == -1 || vertices[uDestino] == null)
            {
                resultado.ExisteRuta = false;
                resultado.MensajeEstado = $"La parada de destino '{codDestino}' no existe en el sistema o fue dada de baja.";
                return resultado;
            }

            // Caso base: El origen y el destino son idénticos
            if (uOrigen == uDestino)
            {
                resultado.ExisteRuta = true;
                resultado.MensajeEstado = "El origen y el destino corresponden a la misma parada.";
                resultado.ResumenCamino = vertices[uOrigen]!.Codigo;
                resultado.CostoTotal = 0.0;
                resultado.TiempoTotalMinutos = 0.0;
                resultado.SecuenciaCodigosParadas.Agregar(vertices[uOrigen]!.Codigo);
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

                if (adyacencias[u] != null)
                {
                    foreach (AristaRuta arista in adyacencias[u])
                    {
                        if (arista == null || arista.Destino == null)
                            continue;

                        int v = ObtenerIndiceParada(arista.Destino.Codigo);
                        if (v == -1 || vertices[v] == null || visitado[v])
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
                if (vertices[indiceVertice] == null)
                    continue;

                string codigoParada = vertices[indiceVertice]!.Codigo;

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
        /// Verifica si una parada existe activa en el grafo mediante su código.
        /// </summary>
        public bool ExisteParada(string codigo)
        {
            return ObtenerIndiceParada(codigo) != -1;
        }

        #endregion

        /// <summary>
        /// Duplica el tamaño de los arreglos internos cuando se supera la capacidad,
        /// preservando las posiciones e índices de los vértices existentes para no desfasar identificadores.
        /// </summary>
        private void Redimensionar()
        {
            int nuevaCapacidad = vertices.Length * 2;
            Parada?[] nuevosVertices = new Parada?[nuevaCapacidad];
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

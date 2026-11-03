// ============================================================
// I1
// Lista enlazada propia
// ============================================================

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Nodo de la lista: guarda un dato y la referencia al siguiente nodo.
    /// </summary>
    public class NodoLista<T>
    {
        public T Dato { get; set; }
        public NodoLista<T>? Siguiente { get; set; } // Enlace al siguiente (null si es el último)

        public NodoLista(T dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    /// <summary>
    /// Lista enlazada simple con referencia a cabeza y cola.
    /// Agregar: O(1) | Obtener, Contiene, Eliminar: O(n) | Recorrido con foreach: O(n)
    /// </summary>
    public class ListaPropia<T> : IEnumerable<T> // Permite usar foreach
    {
        private NodoLista<T>? cabeza; // Primer nodo
        private NodoLista<T>? cola; // Último nodo (agregar en O(1))

        /// <summary>Cantidad de elementos. Solo la lista puede modificarla.</summary>
        public int Count { get; private set; }

        /// <summary>
        /// Acceso al primer nodo de la lista enlazada para manipulación directa de punteros.
        /// </summary>
        public NodoLista<T>? Cabeza
        {
            get => cabeza;
            set
            {
                cabeza = value;
                if (cabeza == null)
                {
                    cola = null;
                }
            }
        }

        /// <summary>
        /// Acceso al último nodo de la lista enlazada para manipulación directa de punteros.
        /// </summary>
        public NodoLista<T>? Cola
        {
            get => cola;
            set => cola = value;
        }

        public ListaPropia()
        {
            cabeza = null;
            cola = null;
            Count = 0;
        }

        /// <summary>
        /// Vacía completamente la lista enlazada restableciendo punteros y contador a cero.
        /// </summary>
        public void Vaciar()
        {
            cabeza = null;
            cola = null;
            Count = 0;
        }

        /// <summary>
        /// Decrementa el contador interno tras una remoción manual de nodo.
        /// </summary>
        public void DecrementarContador()
        {
            if (Count > 0)
            {
                Count--;
            }
        }

        /// <summary> Agrega un elemento al final de la lista. O(1). </summary>
        public void Agregar(T dato)
        {
            NodoLista<T> nuevo = new NodoLista<T>(dato);

            if (cabeza == null) // Lista vacía
            {
                cabeza = nuevo;
                cola = nuevo;
            }
            else
            {
                // Lista con elementos: el último actual apunta al nuevo
                // y el nuevo pasa a ser la cola
                cola!.Siguiente = nuevo;
                cola = nuevo;
            }

            Count++;
        }

        /// <summary>
        /// Devuelve el elemento en la posición indicada (base 0). O(n): debe caminar
        /// desde la cabeza. Para recorrer toda la lista usar foreach, no un for con Obtener.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Si el índice no existe.</exception>
        public T Obtener(int indice)
        {
            // Validar que el índice esté dentro de la lista
            if (indice < 0 || indice >= Count) // Validar rango
            {
                throw new ArgumentOutOfRangeException(nameof(indice), ">> Índice fuera de rango");
            }

            NodoLista<T> actual = cabeza!;
            for (int i = 0; i < indice; i++) // Avanzar 'indice' veces
            {
                actual = actual.Siguiente!;
            }

            return actual.Dato;
        }

        /// <summary>
        /// Indica si el dato existe en la lista. O(n): búsqueda secuencial.
        /// </summary>
        public bool Contiene(T dato)
        {
            NodoLista<T>? actual = cabeza;
            while (actual != null) // Recorrer hasta el final
            {
                // EqualityComparer compara valores genéricos (== no funciona con T)
                if (EqualityComparer<T>.Default.Equals(actual.Dato, dato))
                {
                    return true; // Encontrado
                }

                actual = actual.Siguiente;
            }

            return false; // Llegó al final sin encontrarlo
        }

        /// <summary>
        /// Elimina la primera aparición del dato. O(n).
        /// Devuelve true si lo eliminó, false si no existía.
        /// No llamar dentro de un foreach sobre esta misma lista.
        /// </summary>
        public bool Eliminar(T dato)
        {
            // Se usan dos punteros: para "saltar" un nodo hay que
            // conocer el que está antes de él
            NodoLista<T>? anterior = null;
            NodoLista<T>? actual = cabeza;

            while (actual != null)
            {
                if (EqualityComparer<T>.Default.Equals(actual.Dato, dato))
                {
                    if (anterior == null) // Caso 1: era la cabeza
                    {
                        cabeza = actual.Siguiente;
                    }
                    else // Caso 2: saltar el nodo
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }

                    if (actual == cola) // Caso 3: era el último
                    {
                        cola = anterior;
                    }

                    if (cabeza == null)
                    {
                        cola = null;
                    }

                    Count--;
                    return true; // Eliminado
                }

                // Avanzar ambos punteros (sin esto el ciclo sería infinito)
                anterior = actual;
                actual = actual.Siguiente;
            }
            return false; // No existe
        }

        /// <summary>
        /// Recorre la lista nodo por nodo. Lo usa foreach automáticamente. O(n) en total.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            NodoLista<T>? actual = cabeza;
            while (actual != null)
            {
                yield return actual.Dato; // Entrega el dato y pausa
                actual = actual.Siguiente; // Continúa aquí en la siguiente vuelta
            }
        }

        // Versión no genérica que exige la interfaz; reutilizar la de arriba
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

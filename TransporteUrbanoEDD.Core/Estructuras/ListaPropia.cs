// Lista enlazada propia
using System.ComponentModel;

namespace TransporteUrbanoEDD.Core.Estructuras
{
    // Nodo: guarda un dato y el enlace al siguiente
    public class NodoLista<T>
    {
        public T Dato { get; set; }
        public NodoLista<T>? Siguiente { get; set; }

        public NodoLista(T dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public class ListaPropia<T>
    {
        private NodoLista<T>? cabeza; // Primer nodo
        private NodoLista<T>? cola; // Último nodo (agregar en O(1))
        public int Count { get; private set; }

        public ListaPropia()
        {
            cabeza = null;
            cola = null;
            Count = 0;
        }

        // Agrega al final: O(1)
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
                cola!.Siguiente = nuevo;
                cola = nuevo;
            }

            Count++;
        }

        public T Obtener(int indice)
        {
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

        // Indica si el dato existe en la lista: O(n)
        public bool Contiene(T dato)
        {
            NodoLista<T>? actual = cabeza;
            while (actual!= null) // Recorrer hasta el final
            {
                if (EqualityComparer<T>.Default.Equals(actual.Dato, dato))
                {
                    return true; // Encontrado
                }

                actual = actual.Siguiente;
            }

            return false; // Llegó al final sin encontrarlo
        }
    }
}
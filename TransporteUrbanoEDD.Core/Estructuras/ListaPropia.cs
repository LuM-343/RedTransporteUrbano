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
    }
}
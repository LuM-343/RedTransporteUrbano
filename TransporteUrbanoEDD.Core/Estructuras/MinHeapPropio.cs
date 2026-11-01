using System;
using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Implementación de un Montículo Binario Mínimo (Min-Heap) propio.
    /// Diseñado desde cero utilizando arreglos primitivos para cumplir con las reglas del curso
    /// (prohibido List, PriorityQueue, etc.).
    /// Desarrollado por Dev 3.
    /// </summary>
    /// <typeparam name="T">Tipo del elemento almacenado.</typeparam>
    public class MinHeapPropio<T>
    {
        private NodoHeap<T>[] arreglo;
        private int count;

        public int Count => count;
        public bool EstaVacio => count == 0;
        public int Capacidad => arreglo.Length;

        public MinHeapPropio(int capacidadInicial = 16)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 16;

            arreglo = new NodoHeap<T>[capacidadInicial];
            count = 0;
        }

        /// <summary>
        /// Inserta un nuevo elemento con su respectiva prioridad en el montículo.
        /// Complejidad: O(log n).
        /// </summary>
        public void Insertar(T elemento, double prioridad)
        {
            if (count == arreglo.Length)
            {
                Redimensionar();
            }

            NodoHeap<T> nuevo = new NodoHeap<T>(elemento, prioridad);
            arreglo[count] = nuevo;
            Flotar(count);
            count++;
        }

        /// <summary>
        /// Extrae y retorna el elemento con la menor prioridad (raíz del heap).
        /// Complejidad: O(log n).
        /// </summary>
        public NodoHeap<T> ExtraerMinimo()
        {
            if (EstaVacio)
            {
                throw new InvalidOperationException("No se puede extraer del montículo porque está vacío.");
            }

            NodoHeap<T> minimo = arreglo[0];

            // Reemplazar la raíz con el último elemento
            arreglo[0] = arreglo[count - 1];
            arreglo[count - 1] = null!;
            count--;

            if (count > 0)
            {
                Hundir(0);
            }

            return minimo;
        }

        /// <summary>
        /// Consulta el elemento de menor prioridad sin removerlo.
        /// Complejidad: O(1).
        /// </summary>
        public NodoHeap<T> VerMinimo()
        {
            if (EstaVacio)
            {
                throw new InvalidOperationException("El montículo está vacío.");
            }

            return arreglo[0];
        }

        /// <summary>
        /// Reestablece la propiedad de min-heap hacia arriba (hacia la raíz).
        /// </summary>
        private void Flotar(int i)
        {
            while (i > 0)
            {
                int padre = (i - 1) / 2;

                if (arreglo[i].Prioridad < arreglo[padre].Prioridad)
                {
                    Intercambiar(i, padre);
                    i = padre;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Reestablece la propiedad de min-heap hacia abajo (hacia las hojas).
        /// </summary>
        private void Hundir(int i)
        {
            while (true)
            {
                int hijoIzquierdo = 2 * i + 1;
                int hijoDerecho = 2 * i + 2;
                int menor = i;

                if (hijoIzquierdo < count && arreglo[hijoIzquierdo].Prioridad < arreglo[menor].Prioridad)
                {
                    menor = hijoIzquierdo;
                }

                if (hijoDerecho < count && arreglo[hijoDerecho].Prioridad < arreglo[menor].Prioridad)
                {
                    menor = hijoDerecho;
                }

                if (menor != i)
                {
                    Intercambiar(i, menor);
                    i = menor;
                }
                else
                {
                    break;
                }
            }
        }

        private void Intercambiar(int a, int b)
        {
            NodoHeap<T> temp = arreglo[a];
            arreglo[a] = arreglo[b];
            arreglo[b] = temp;
        }

        /// <summary>
        /// Duplica la capacidad del arreglo interno utilizando arreglos nativos.
        /// </summary>
        private void Redimensionar()
        {
            int nuevaCapacidad = arreglo.Length * 2;
            NodoHeap<T>[] nuevoArreglo = new NodoHeap<T>[nuevaCapacidad];

            for (int i = 0; i < count; i++)
            {
                nuevoArreglo[i] = arreglo[i];
            }

            arreglo = nuevoArreglo;
        }

        /// <summary>
        /// Limpia todos los elementos del montículo.
        /// </summary>
        public void Limpiar()
        {
            for (int i = 0; i < count; i++)
            {
                arreglo[i] = null!;
            }
            count = 0;
        }
    }
}

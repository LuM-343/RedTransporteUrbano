// ============================================================
// Conjuntos disjuntos (Union-Find) para zonas conectadas.
// Dos paradas están en la misma zona si existe una cadena de
// rutas que las une. Find con compresión de caminos y
// ============================================================

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Union-Find sobre índices enteros (el Indice de cada Parada).
    /// Find, Union y EstanConectadas en O(α(n)) amortizado.
    /// </summary>
    public class UnionFindZonas
    {
        private int[] padre; // padre[i] = padre de i en su árbol; si padre[i] == i, i es raíz
        private int[] rango; // Altura aproximada del árbol cuya raíz es i (solo vale en raíces)

        /// <summary>
        /// Cantidad de elementos (paradas) registrados.
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Cantidad de zonas (grupos) distintas. Empieza en una por elemento
        /// y baja en 1 con cada Union que fusiona dos grupos.
        /// </summary>
        public int CantidadZonas { get; private set; }

        public UnionFindZonas(int capacidadInicial = 16)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 16;

            padre = new int[capacidadInicial];
            rango = new int[capacidadInicial];
            Cantidad = 0;
            CantidadZonas = 0;
        }

        // ---------------------------------------------------------
        // AGREGAR ELEMENTO
        // ---------------------------------------------------------

        /// <summary>
        /// Registra un elemento nuevo como su propia zona y devuelve su índice.
        /// Los índices se asignan en orden (0, 1, 2…), igual que en el Grafo.
        /// O(1) amortizado: si el arreglo se llena, se duplica.
        /// </summary>
        public int AgregarElemento()
        {
            if (Cantidad == padre.Length)
                Crecer();

            int nuevo = Cantidad;
            padre[nuevo] = nuevo; // Es raíz de sí mismo: forma su propia zona
            rango[nuevo] = 0;     // Árbol de un solo nodo
            Cantidad++;
            CantidadZonas++;      // Una zona nueva

            return nuevo;
        }

        // ---------------------------------------------------------
        // FIND (con compresión de caminos)
        // ---------------------------------------------------------

        /// <summary>
        /// Devuelve la raíz (representante) de la zona de x.
        /// Compresión de caminos: cada nodo del recorrido queda apuntando directo a la raíz.
        /// </summary>
        public int Find(int x)
        {
            ValidarIndice(x);

            if (padre[x] != x)
                padre[x] = Find(padre[x]); // Sube recursivamente y, al volver, reconecta x a la raíz

            return padre[x];
        }

        // ---------------------------------------------------------
        // UNION (por rango)
        // ---------------------------------------------------------

        /// <summary>
        /// Fusiona las zonas de a y b. Cuelga el árbol de menor rango bajo el de mayor rango.
        /// Devuelve true si eran zonas distintas, false si ya estaban juntas.
        /// </summary>
        public bool Union(int a, int b)
        {
            int raizA = Find(a);
            int raizB = Find(b);

            if (raizA == raizB)
                return false; // Ya están en la misma zona: nada que unir

            // El árbol más bajo se cuelga debajo del más alto (así la altura no crece)
            if (rango[raizA] < rango[raizB])
            {
                padre[raizA] = raizB;
            }
            else if (rango[raizA] > rango[raizB])
            {
                padre[raizB] = raizA;
            }
            else
            {
                // Misma altura: da igual cuál va abajo, pero el de arriba crece 1 nivel
                padre[raizB] = raizA;
                rango[raizA]++;
            }

            CantidadZonas--; // Dos zonas se volvieron una
            return true;
        }

        /// <summary>
        /// Indica si a y b pertenecen a la misma zona (misma raíz). O(α(n)).
        /// </summary>
        public bool EstanConectadas(int a, int b)
            => Find(a) == Find(b);

        // ---------------------------------------------------------
        // AUXILIARES
        // ---------------------------------------------------------

        // Duplica el tamaño de los arreglos copiando los valores existentes
        private void Crecer()
        {
            int[] nuevoPadre = new int[padre.Length * 2];
            int[] nuevoRango = new int[rango.Length * 2];

            for (int i = 0; i < Cantidad; i++)
            {
                nuevoPadre[i] = padre[i];
                nuevoRango[i] = rango[i];
            }

            padre = nuevoPadre;
            rango = nuevoRango;
        }

        // Lanza error si el índice no corresponde a un elemento registrado
        private void ValidarIndice(int x)
        {
            if (x < 0 || x >= Cantidad)
                throw new ArgumentOutOfRangeException(nameof(x), $"Índice {x} no registrado en el Union-Find");
        }
    }
}
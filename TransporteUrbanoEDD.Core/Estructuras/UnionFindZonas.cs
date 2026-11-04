// ============================================================
// I1
// Conjuntos disjuntos (Union-Find) para zonas conectadas.
// Dos paradas están en la misma zona si existe una cadena de
// rutas que las une. Find con compresión de caminos y
// Union por rango: O(α(n)) amortizado (prácticamente constante).
// ============================================================

using TransporteUrbanoEDD.Core.Modelos;

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
        private TablaHashParadas? tablaHash; // Para traducir código → Indice en EstanEnMismaZona

        /// <summary>
        /// Cantidad de elementos (paradas) registrados.
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Cantidad de zonas (grupos) distintas. Empieza en una por elemento
        /// y baja en 1 con cada Union que fusiona dos grupos.
        /// </summary>
        public int CantidadZonas { get; private set; }

        /// <param name="tablaHash">Tabla donde buscar paradas por código (para EstanEnMismaZona).</param>
        public UnionFindZonas(int capacidadInicial = 16, TablaHashParadas? tablaHash = null)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 16;

            padre = new int[capacidadInicial];
            rango = new int[capacidadInicial];
            Cantidad = 0;
            CantidadZonas = 0;
            this.tablaHash = tablaHash;
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
        // CONSULTA POR CÓDIGO (contrato del Orquestador)
        // ---------------------------------------------------------

        /// <summary>
        /// Indica si dos paradas (por código) están en la misma zona conectada.
        /// Traduce cada código a su Indice con la Tabla Hash: O(1) + O(α(n)).
        /// Devuelve false si alguna no existe o no está registrada.
        /// </summary>
        public bool EstanEnMismaZona(string codigoA, string codigoB)
        {
            if (tablaHash == null)
                throw new InvalidOperationException("El Union-Find no tiene una Tabla Hash vinculada");

            // 1. Código → Parada (Tabla Hash, O(1) promedio)
            Parada? a = tablaHash.Buscar(codigoA);
            Parada? b = tablaHash.Buscar(codigoB);
            if (a == null || b == null)
                return false;

            // 2. La parada debe tener un índice válido en el Union-Find
            if (a.Indice < 0 || a.Indice >= Cantidad || b.Indice < 0 || b.Indice >= Cantidad)
                return false;

            // 3. Parada → Indice → misma raíz
            return EstanConectadas(a.Indice, b.Indice);
        }

        // ---------------------------------------------------------
        // RECONSTRUIR (tras eliminar paradas o rutas)
        // ---------------------------------------------------------

        /// <summary>
        /// Reinicia el Union-Find con n elementos separados y vuelve a unir según las rutas.
        /// Necesario porque Union-Find no puede "separar" zonas al borrar una ruta o parada.
        /// O(n + m·α(n)), n = paradas, m = rutas.
        /// </summary>
        /// <param name="paradasActivas">Paradas que siguen existiendo; las posiciones eliminadas no cuentan como zona.</param>
        public void Reconstruir(int cantidadElementos, ListaPropia<AristaRuta> rutas,
                                ListaPropia<Parada>? paradasActivas = null)
        {
            // 1. Asegurar espacio suficiente
            while (padre.Length < cantidadElementos)
                Crecer();

            // 2. Cada elemento vuelve a ser su propia zona
            for (int i = 0; i < cantidadElementos; i++)
            {
                padre[i] = i;
                rango[i] = 0;
            }
            Cantidad = cantidadElementos;
            CantidadZonas = cantidadElementos;

            // 3. Volver a unir según las rutas existentes (usa el Indice de cada parada)
            foreach (AristaRuta ruta in rutas)
                Union(ruta.Origen.Indice, ruta.Destino.Indice);

            // 4. Si hay posiciones de paradas eliminadas, solo cuentan las zonas de paradas activas
            if (paradasActivas != null)
            {
                int zonas = 0;
                foreach (Parada p in paradasActivas)
                    if (Find(p.Indice) == p.Indice) // Es raíz: representa una zona
                        zonas++;
                CantidadZonas = zonas;
            }
        }

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
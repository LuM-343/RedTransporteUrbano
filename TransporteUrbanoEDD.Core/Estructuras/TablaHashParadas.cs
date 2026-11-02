// Tabla Hash de paradas indexada por Código ("PAR-01").
// Colisiones: encadenamiento separado con ListaPropia.

using TransporteUrbanoEDD.Core.Modelos;

namespace TransporteUrbanoEDD.Core.Estructuras
{
    /// <summary>
    /// Tabla Hash de paradas. Buscar, Insertar y Eliminar en O(1) promedio.
    /// Hace rehashing automático cuando el factor de carga supera 0.70.
    /// </summary>
    public class TablaHashParadas
    {
        private const int BASE_HORNER = 31;          // Primo usado como base del polinomio
        private const double FACTOR_CARGA_MAX = 0.70; // Límite antes de hacer rehashing

        private ListaPropia<Parada>[] buckets; // Arreglo de cajones; cada uno es una lista (cadena)

        /// <summary>
        /// Cantidad de paradas almacenadas.
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Cantidad de buckets (tamaño del arreglo).
        /// </summary>
        public int Capacidad => buckets.Length;

        /// <summary>
        /// Elementos / buckets. Mide qué tan llena está la tabla.
        /// </summary>
        public double FactorCarga => (double)Cantidad / Capacidad;

        /// <param name="capacidadInicial">Conviene que sea primo para repartir mejor (ej. 11).</param>
        public TablaHashParadas(int capacidadInicial = 11)
        {
            if (capacidadInicial < 1)
                capacidadInicial = 11;

            buckets = CrearBuckets(capacidadInicial);
            Cantidad = 0;
        }

        // ---------------------------------------------------------
        // FUNCIÓN HASH
        // ---------------------------------------------------------

        /// <summary>
        /// Convierte un código en el índice de su bucket con el método de Horner:
        /// h = (h * 31 + c) % capacidad, por cada carácter. O(k), k = largo del código.
        /// </summary>
        public int FuncionHash(string codigo)
        {
            // Normalizar igual que Parada, para que "par-01" y "PAR-01" caigan en el mismo bucket
            string clave = codigo.Trim().ToUpperInvariant();

            int h = 0;
            foreach (char c in clave)
            {
                // Horner: multiplicar lo acumulado por la base y sumar el nuevo carácter.
                // El % en cada paso evita que el número se desborde.
                h = (h * BASE_HORNER + c) % Capacidad;
            }

            return h; // Siempre queda entre 0 y Capacidad - 1
        }

        // ---------------------------------------------------------
        // AUXILIARES
        // ---------------------------------------------------------

        // Crea el arreglo y una lista vacía en cada posición
        // (el arreglo solo crea los espacios; las listas hay que crearlas una por una)
        private static ListaPropia<Parada>[] CrearBuckets(int capacidad)
        {
            ListaPropia<Parada>[] nuevos = new ListaPropia<Parada>[capacidad];
            for (int i = 0; i < capacidad; i++)
                nuevos[i] = new ListaPropia<Parada>();
            return nuevos;
        }
    }
}
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

        /// <summary>Cuántas veces se ha hecho rehashing (útil para mostrar en la UI).</summary>
        public int Rehashes { get; private set; }

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
            string clave = Normalizar(codigo);

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
        // INSERTAR
        // ---------------------------------------------------------

        /// <summary>
        /// Inserta una parada en el bucket que le corresponde. O(1) promedio.
        /// Devuelve false si ya existe una parada con el mismo código (no admite duplicados).
        /// </summary>
        public bool Insertar(Parada parada)
        {
            if (parada == null)
                throw new ArgumentNullException(nameof(parada));

            // El código es clave única: no se permiten dos paradas con el mismo
            if (Contiene(parada.Codigo))
                return false;

            // Si al insertar el factor de carga superaría 0.70, primero se agranda la tabla
            double factorProyectado = (double)(Cantidad + 1) / Capacidad;
            if (factorProyectado > FACTOR_CARGA_MAX)
                Rehashing();

            InsertarEnBucket(parada);
            return true;
        }

        // Coloca la parada en su bucket sin validar duplicados.
        // Separado de Insertar para reutilizarlo en el rehashing.
        private void InsertarEnBucket(Parada parada)
        {
            int indice = FuncionHash(parada.Codigo); // 1. Calcular el bucket
            buckets[indice].Agregar(parada);         // 2. Agregar a la cadena (si hay colisión, se encadena)
            Cantidad++;
        }

        // ---------------------------------------------------------
        // BUSCAR
        // ---------------------------------------------------------

        /// <summary>
        /// Devuelve la parada con ese código, o null si no existe. O(1) promedio:
        /// solo recorre la cadena de un bucket, no toda la tabla.
        /// </summary>
        public Parada? Buscar(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            string clave = Normalizar(codigo);
            int indice = FuncionHash(clave); // 1. Ir directo al bucket

            // 2. Recorrer solo esa cadena comparando códigos
            foreach (Parada parada in buckets[indice])
            {
                if (parada.Codigo == clave)
                    return parada; // Encontrada
            }

            return null; // No está en su bucket, entonces no existe
        }

        /// <summary>
        /// Indica si existe una parada con ese código. O(1) promedio.
        /// </summary>
        public bool Contiene(string codigo)
            => Buscar(codigo) != null;

        // ---------------------------------------------------------
        // ELIMINAR
        // ---------------------------------------------------------

        /// <summary>
        /// Elimina la parada con ese código. O(1) promedio.
        /// Devuelve true si la eliminó, false si no existía.
        /// </summary>
        public bool Eliminar(string codigo)
        {
            Parada? parada = Buscar(codigo); // 1. Encontrarla en su bucket
            if (parada == null)
                return false;

            int indice = FuncionHash(parada.Codigo);
            buckets[indice].Eliminar(parada);  // 2. Quitarla de la cadena (Parada.Equals compara por código)
            Cantidad--;
            return true;
        }

        // ---------------------------------------------------------
        // CONSULTAS GENERALES (UI, CSV, impresión)
        // ---------------------------------------------------------

        /// <summary>
        /// Devuelve todas las paradas en una lista. O(n + m): recorre los m buckets
        /// y sus n paradas. Útil para llenar tablas en la UI o guardar el CSV.
        /// </summary>
        public ListaPropia<Parada> ObtenerTodas()
        {
            ListaPropia<Parada> todas = new ListaPropia<Parada>();

            foreach (ListaPropia<Parada> cadena in buckets) // Cada bucket
                foreach (Parada parada in cadena)           // Cada parada de su cadena
                    todas.Agregar(parada);

            return todas;
        }

        /// <summary>
        /// Devuelve la cadena de un bucket (para la vista visual de buckets en la UI).
        /// No modificar la lista devuelta: usar Insertar/Eliminar de la tabla.
        /// </summary>
        public ListaPropia<Parada> ObtenerBucket(int indice)
        {
            if (indice < 0 || indice >= Capacidad)
                throw new ArgumentOutOfRangeException(nameof(indice), "Bucket fuera de rango");

            return buckets[indice];
        }

        /// <summary>
        /// Imprime en consola el estado de la tabla: cada bucket con su cadena.
        /// </summary>
        public void Mostrar()
        {
            Console.WriteLine($"  Tabla Hash | Capacidad: {Capacidad} | Paradas: {Cantidad} | " +
                              $"Factor: {FactorCarga:F2} | Rehashes: {Rehashes}");

            for (int i = 0; i < Capacidad; i++)
            {
                Console.Write($"    [{i,2}] ");   // {i,2} alinea el número a 2 espacios

                if (buckets[i].Count == 0)
                {
                    Console.WriteLine("VACÍO");
                    continue;
                }

                // Mostrar la cadena: PAR-01 -> PAR-14 -> FIN (más de uno = colisión)
                foreach (Parada parada in buckets[i])
                    Console.Write($"{parada.Codigo} -> ");
                Console.WriteLine(buckets[i].Count > 1 ? "FIN  (colisión)" : "FIN");
            }
        }

        // ---------------------------------------------------------
        // REHASHING
        // ---------------------------------------------------------

        // Agranda la tabla a (capacidad * 2 + 1) y reubica todas las paradas.
        // Cada parada se vuelve a pasar por la función hash porque el
        // % ahora usa la nueva capacidad. O(n), pero ocurre pocas veces.
        private void Rehashing()
        {
            ListaPropia<Parada>[] anteriores = buckets;    // 1. Guardar la tabla vieja

            buckets = CrearBuckets(anteriores.Length * 2 + 1); // 2. Tabla nueva más grande (Capacidad cambia aquí)
            Cantidad = 0;                                  // 3. Se vuelve a contar al reinsertar

            foreach (ListaPropia<Parada> cadena in anteriores) // 4. Recorrer cada bucket viejo
                foreach (Parada parada in cadena)              //    y cada parada de su cadena
                    InsertarEnBucket(parada);                  //    recalculando su bucket nuevo

            Rehashes++;
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

        // Normaliza el código igual que Parada: sin espacios y en mayúsculas
        private static string Normalizar(string codigo)
            => codigo.Trim().ToUpperInvariant();
    }
}
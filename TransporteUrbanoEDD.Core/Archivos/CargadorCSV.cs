// ============================================================
// I1
// Registro centralizado de paradas y rutas en las 4 estructuras
// (Tabla Hash, Grafo, Union-Find y Trie) y carga desde CSV.
// ============================================================

using TransporteUrbanoEDD.Core.Estructuras;
using TransporteUrbanoEDD.Core.Modelos;
using System.Globalization; // CultureInfo para leer decimales con punto
using System.Text;          // Encoding.UTF8 para leer tildes y ñ

namespace TransporteUrbanoEDD.Core.Archivos
{
    /// <summary>
    /// Resumen de una carga de archivo: cuántos registros entraron y qué líneas fallaron.
    /// </summary>
    public class ResultadoCarga
    {
        public bool ArchivoValido { get; set; } = true;                            // false si no se pudo abrir
        public int Cargados { get; set; }                                          // Registros agregados con éxito
        public ListaPropia<string> Errores { get; } = new ListaPropia<string>();   // Un mensaje por línea con error

        public override string ToString()
            => ArchivoValido
                ? $"{Cargados} cargados, {Errores.Count} con error"
                : $"Archivo no válido: {(Errores.Count > 0 ? Errores.Obtener(0) : "")}";
    }

    /// <summary>
    /// Registra paradas y rutas manteniendo sincronizadas las 4 estructuras.
    /// Lo usan la carga de CSV y el CRUD de la interfaz.
    /// </summary>
    public class CargadorCSV
    {
        private readonly TablaHashParadas tablaHash;
        private readonly GrafoTransporte grafo;
        private readonly UnionFindZonas unionFind;
        private readonly TrieParadas? trie; // Opcional: si es null, no se alimenta el autocompletado

        public CargadorCSV(TablaHashParadas tablaHash, GrafoTransporte grafo,
                           UnionFindZonas unionFind, TrieParadas? trie = null)
        {
            this.tablaHash = tablaHash;
            this.grafo = grafo;
            this.unionFind = unionFind;
            this.trie = trie;
        }

        // ---------------------------------------------------------
        // REGISTRO EN LAS 4 ESTRUCTURAS
        // ---------------------------------------------------------

        /// <summary>
        /// Registra una parada en Grafo, Tabla Hash, Union-Find y Trie, y le asigna su Indice.
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? RegistrarParada(Parada parada)
        {
            if (parada == null)
                return "La parada es nula";

            // 1. El código es clave única
            if (tablaHash.Contiene(parada.Codigo))
                return $"El código '{parada.Codigo}' ya existe";

            // 2. Grafo primero: su posición interna será el Indice de la parada
            if (!grafo.AgregarParada(parada))
                return $"El código '{parada.Codigo}' ya existe en el grafo";

            // 3. Tabla Hash
            tablaHash.Insertar(parada);

            // 4. Indice = posición en el grafo (el grafo numera 0, 1, 2… en orden de inserción)
            parada.Indice = grafo.CantidadParadas - 1;

            // 5. Union-Find: también numera en orden, así que debe dar el mismo número
            int indiceUF = unionFind.AgregarElemento();
            if (indiceUF != parada.Indice)
                throw new InvalidOperationException(
                    $"Grafo ({parada.Indice}) y Union-Find ({indiceUF}) desincronizados");

            // 6. Trie (autocompletado por nombre)
            trie?.Insertar(parada.Nombre, parada.Codigo); // ?. = solo si trie no es null

            return null; // Éxito
        }

        /// <summary>
        /// Registra una ruta en el Grafo y une sus paradas en el Union-Find (misma zona).
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? RegistrarRuta(string codOrigen, string codDestino,
                                     double tiempo, double costo, bool bidireccional)
        {
            // 1. Ambas paradas deben existir (búsqueda O(1) en la Tabla Hash)
            Parada? origen = tablaHash.Buscar(codOrigen);
            Parada? destino = tablaHash.Buscar(codDestino);

            if (origen == null)
                return $"La parada de origen '{codOrigen}' no existe";
            if (destino == null)
                return $"La parada de destino '{codDestino}' no existe";
            if (origen.Codigo == destino.Codigo)
                return "El origen y el destino no pueden ser la misma parada";
            if (tiempo < 0 || costo < 0)
                return "El tiempo y el costo no pueden ser negativos";

            // 2. Grafo: crea la arista (y la de regreso si es bidireccional)
            grafo.AgregarRuta(origen.Codigo, destino.Codigo, tiempo, costo, bidireccional);

            // 3. Union-Find: quedan en la misma zona (ignora la dirección de la ruta)
            unionFind.Union(origen.Indice, destino.Indice);

            return null; // Éxito
        }

        /// <summary>
        /// Elimina una parada de la Tabla Hash y del Grafo (con sus rutas) y reconstruye el Union-Find.
        /// Falta quitarla del Trie cuando tenga Eliminar (Integrante 2).
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? EliminarParada(string codigo)
        {
            if (!tablaHash.Eliminar(codigo))
                return $"La parada '{codigo}' no existe";

            // Baja del vértice y purga de aristas en el grafo (Integrante 3)
            grafo.EliminarParada(codigo);
            // Reconstrucción del Union-Find para mantener zonas actualizadas
            unionFind.Reconstruir(grafo.CantidadParadas, grafo.ObtenerTodasLasRutas(), grafo.ObtenerTodasLasParadas());
            // y cuando el Trie tenga Eliminar (Integrante 2):
            //   trie?.Eliminar(nombre);

            return null; // Éxito
        }

        // ---------------------------------------------------------
        // CARGA DESDE ARCHIVOS CSV
        // ---------------------------------------------------------

        /// <summary>
        /// Carga paradas desde un CSV con formato: Codigo,Nombre,X,Y,Zona (Zona opcional).
        /// Las líneas con error se reportan y se omiten; las demás sí se cargan.
        /// </summary>
        public ResultadoCarga CargarParadasCSV(string rutaArchivo)
        {
            ResultadoCarga resultado = new ResultadoCarga();
            string[]? lineas = LeerArchivo(rutaArchivo, resultado);
            if (lineas == null)
                return resultado; // No se pudo abrir: el motivo ya está en Errores

            for (int i = 0; i < lineas.Length; i++)
            {
                int numLinea = i + 1;           // Para el usuario las líneas empiezan en 1
                string linea = lineas[i].Trim();

                if (EsLineaIgnorable(linea, i, "Codigo"))
                    continue;

                string[] campos = linea.Split(',');
                if (campos.Length < 4)
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: se esperaban al menos 4 campos (Codigo,Nombre,X,Y)");
                    continue;
                }

                if (!LeerNumero(campos[2], out double x) || !LeerNumero(campos[3], out double y))
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: coordenadas inválidas ('{campos[2]}', '{campos[3]}')");
                    continue;
                }

                string zona = campos.Length >= 5 ? campos[4] : ""; // La zona es opcional

                Parada parada;
                try
                {
                    parada = new Parada(campos[0], campos[1], x, y, zona);
                }
                catch (ArgumentException ex) // Código vacío
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: {ex.Message}");
                    continue;
                }

                string? error = RegistrarParada(parada);
                if (error == null)
                    resultado.Cargados++;
                else
                    resultado.Errores.Agregar($"Línea {numLinea}: {error}");
            }

            return resultado;
        }

        /// <summary>
        /// Carga rutas desde un CSV con formato: Origen,Destino,Tiempo,Costo,Bidireccional.
        /// Las paradas deben haberse cargado antes. Bidireccional acepta true/false, 1/0, si/no.
        /// </summary>
        public ResultadoCarga CargarRutasCSV(string rutaArchivo)
        {
            ResultadoCarga resultado = new ResultadoCarga();
            string[]? lineas = LeerArchivo(rutaArchivo, resultado);
            if (lineas == null)
                return resultado;

            for (int i = 0; i < lineas.Length; i++)
            {
                int numLinea = i + 1;
                string linea = lineas[i].Trim();

                if (EsLineaIgnorable(linea, i, "Origen"))
                    continue;

                string[] campos = linea.Split(',');
                if (campos.Length < 5)
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: se esperaban 5 campos (Origen,Destino,Tiempo,Costo,Bidireccional)");
                    continue;
                }

                if (!LeerNumero(campos[2], out double tiempo) || !LeerNumero(campos[3], out double costo))
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: tiempo o costo inválido ('{campos[2]}', '{campos[3]}')");
                    continue;
                }

                if (!LeerBooleano(campos[4], out bool bidireccional))
                {
                    resultado.Errores.Agregar($"Línea {numLinea}: valor de Bidireccional inválido ('{campos[4]}')");
                    continue;
                }

                string? error = RegistrarRuta(campos[0], campos[1], tiempo, costo, bidireccional);
                if (error == null)
                    resultado.Cargados++;
                else
                    resultado.Errores.Agregar($"Línea {numLinea}: {error}");
            }

            return resultado;
        }

        // ---------------------------------------------------------
        // ESCRITURA DE ARCHIVOS CSV
        // ---------------------------------------------------------

        /// <summary>
        /// Guarda todas las paradas en un CSV con el mismo formato que se carga. O(n + m).
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? GuardarParadasCSV(string rutaArchivo)
        {
            try
            {
                // UTF-8 sin BOM para que el encabezado se lea igual al volver a cargarlo
                using StreamWriter escritor = new StreamWriter(rutaArchivo, false, new UTF8Encoding(false));
                escritor.WriteLine("Codigo,Nombre,X,Y,Zona");

                foreach (Parada p in tablaHash.ObtenerTodas())
                    escritor.WriteLine($"{p.Codigo},{p.Nombre},{Numero(p.X)},{Numero(p.Y)},{p.Zona}");

                return null;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return $"No se pudo guardar: {ex.Message}";
            }
        }

        /// <summary>
        /// Guarda todas las rutas en un CSV. Las de doble sentido se escriben una sola vez.
        /// Devuelve null si tuvo éxito, o el mensaje de error.
        /// </summary>
        public string? GuardarRutasCSV(string rutaArchivo)
        {
            try
            {
                using StreamWriter escritor = new StreamWriter(rutaArchivo, false, new UTF8Encoding(false));
                escritor.WriteLine("Origen,Destino,Tiempo,Costo,Bidireccional");

                foreach (AristaRuta r in grafo.ObtenerTodasLasRutas())
                {
                    // El grafo guarda la ida y la vuelta: se escribe solo la de índice menor a mayor
                    if (r.EsBidireccional && r.Origen.Indice > r.Destino.Indice)
                        continue;

                    escritor.WriteLine($"{r.Origen.Codigo},{r.Destino.Codigo},{Numero(r.Tiempo)}," +
                                       $"{Numero(r.Costo)},{(r.EsBidireccional ? "true" : "false")}");
                }

                return null;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return $"No se pudo guardar: {ex.Message}";
            }
        }

        // Número con punto decimal, igual que se lee (InvariantCulture)
        private static string Numero(double valor)
            => valor.ToString(CultureInfo.InvariantCulture);

        // ---------------------------------------------------------
        // AUXILIARES DE LECTURA
        // ---------------------------------------------------------

        // Valida el archivo y devuelve sus líneas, o null si no se puede usar (y anota el motivo)
        private static string[]? LeerArchivo(string rutaArchivo, ResultadoCarga resultado)
        {
            string? motivo = null;

            if (string.IsNullOrWhiteSpace(rutaArchivo))
                motivo = "No se indicó la ruta del archivo";
            else if (!File.Exists(rutaArchivo))
                motivo = $"El archivo '{rutaArchivo}' no existe";
            else if (!rutaArchivo.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) &&
                     !rutaArchivo.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                motivo = "El archivo debe ser .csv o .txt";

            if (motivo == null)
            {
                try
                {
                    return File.ReadAllLines(rutaArchivo, Encoding.UTF8);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    motivo = $"No se pudo leer el archivo: {ex.Message}"; // En uso o sin permisos
                }
            }

            resultado.ArchivoValido = false;
            resultado.Errores.Agregar(motivo);
            return null;
        }

        // Líneas que no son datos: vacías, comentarios (#) o el encabezado en la primera línea
        private static bool EsLineaIgnorable(string linea, int indice, string primerCampoEncabezado)
            => linea.Length == 0
               || linea.StartsWith("#")
               || (indice == 0 && linea.StartsWith(primerCampoEncabezado, StringComparison.OrdinalIgnoreCase));

        // Convierte texto a número usando punto decimal, sin depender del idioma de la computadora
        private static bool LeerNumero(string texto, out double valor)
            => double.TryParse(texto.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out valor);

        // Acepta true/false, 1/0, si/no (sin importar mayúsculas)
        private static bool LeerBooleano(string texto, out bool valor)
        {
            switch (texto.Trim().ToLowerInvariant())
            {
                case "true": case "1": case "si": case "sí":
                    valor = true;
                    return true;
                case "false": case "0": case "no":
                    valor = false;
                    return true;
                default:
                    valor = false;
                    return false; // Texto no reconocido
            }
        }
    }
}
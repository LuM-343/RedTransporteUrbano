// ============================================================
// Integrante 1: Arenz Peláez
// Modelo de una parada de bus. Solo guarda datos; la usan
// la Tabla Hash, el Trie, el Union-Find, el Grafo y el mapa.
// (Versión unificada con la usada por el Grafo y la UI)
// ============================================================

namespace TransporteUrbanoEDD.Core.Modelos
{
    /// <summary>
    /// Parada de la red de transporte. Se identifica de forma única por su Código.
    /// </summary>
    public class Parada
    {
        /// <summary>Clave única (ej. "PAR-01"). Solo lectura: si cambiara,
        /// la Tabla Hash la buscaría en el bucket equivocado.</summary>
        public string Codigo { get; }

        public string Nombre { get; set; } // Nombre visible; el Trie lo usa para autocompletar
        public double X { get; set; }      // Posición horizontal en el mapa
        public double Y { get; set; }      // Posición vertical en el mapa
        public string Zona { get; set; }   // Zona descriptiva (ej. "Zona 1"); opcional

        /// <summary>Posición de la parada en los arreglos del Union-Find y del Grafo.
        /// -1 = aún no registrada en el sistema.</summary>
        public int Indice { get; set; } = -1;

        /// <param name="zona">Opcional: permite crear paradas con solo código, nombre y coordenadas.</param>
        public Parada(string codigo, string nombre, double x = 0, double y = 0, string zona = "")
        {
            // El código es obligatorio porque es la clave de búsqueda
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código de la parada no puede estar vacío.");

            // Normalizar: sin espacios y en mayúsculas (" par-01" -> "PAR-01")
            Codigo = codigo.Trim().ToUpperInvariant();
            Nombre = nombre?.Trim() ?? string.Empty; // Si viene null, queda vacío
            X = x;
            Y = y;
            Zona = zona?.Trim() ?? string.Empty;
        }

        /// <summary>Texto que se muestra en consola y en controles de WinForms.</summary>
        public override string ToString()
            => $"[{Codigo}] {Nombre} ({X}, {Y})";

        /// <summary>Dos paradas son iguales si tienen el mismo código.
        /// Lo usan ListaPropia.Contiene y ListaPropia.Eliminar.</summary>
        public override bool Equals(object? obj)
            => obj is Parada otra
               && string.Equals(Codigo, otra.Codigo, StringComparison.OrdinalIgnoreCase);

        // Obligatorio al sobrescribir Equals. No lo usa nuestra TablaHash (esa usa Horner).
        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(Codigo);
    }
}
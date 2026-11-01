namespace TransporteUrbanoEDD.Core.Modelos
{
    /// <summary>
    /// Representa un nodo/vértice de la red de transporte urbano.
    /// Desarrollado como modelo de entidad base para el sistema.
    /// </summary>
    public class Parada
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Parada(string codigo, string nombre, double x = 0, double y = 0)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ArgumentException("El código de la parada no puede ser nulo ni vacío.", nameof(codigo));
            }

            Codigo = codigo.Trim().ToUpperInvariant();
            Nombre = nombre?.Trim() ?? string.Empty;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"[{Codigo}] {Nombre} ({X}, {Y})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Parada otra)
            {
                return string.Equals(Codigo, otra.Codigo, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Codigo);
        }
    }
}

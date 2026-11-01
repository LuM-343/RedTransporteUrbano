namespace TransporteUrbanoEDD.Core.Modelos
{
    /// <summary>
    /// Representa una arista/conexión vial entre dos paradas en la red de transporte.
    /// Soporta doble peso: Tiempo de viaje (minutos) y Costo del pasaje (moneda).
    /// Desarrollado por Dev 3.
    /// </summary>
    public class AristaRuta
    {
        public Parada Origen { get; set; }
        public Parada Destino { get; set; }
        public double Tiempo { get; set; } // En minutos (>= 0)
        public double Costo { get; set; }  // En moneda/quetzales (>= 0)
        public bool EsBidireccional { get; set; }

        public AristaRuta(Parada origen, Parada destino, double tiempo, double costo, bool esBidireccional = false)
        {
            Origen = origen ?? throw new ArgumentNullException(nameof(origen));
            Destino = destino ?? throw new ArgumentNullException(nameof(destino));
            
            if (tiempo < 0)
                throw new ArgumentOutOfRangeException(nameof(tiempo), "El tiempo de viaje no puede ser negativo.");
            if (costo < 0)
                throw new ArgumentOutOfRangeException(nameof(costo), "El costo de viaje no puede ser negativo.");

            Tiempo = tiempo;
            Costo = costo;
            EsBidireccional = esBidireccional;
        }

        public override string ToString()
        {
            string flecha = EsBidireccional ? "<--->" : "--->";
            return $"{Origen.Codigo} {flecha} {Destino.Codigo} | Tiempo: {Tiempo:F1} min | Costo: Q{Costo:F2}";
        }
    }
}

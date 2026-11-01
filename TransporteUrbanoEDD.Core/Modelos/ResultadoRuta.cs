using System;
using TransporteUrbanoEDD.Core.Estructuras;

namespace TransporteUrbanoEDD.Core.Modelos
{
    /// <summary>
    /// Encapsula el resultado completo del algoritmo de Dijkstra para el cálculo
    /// de la ruta óptima entre dos paradas de la red de transporte.
    /// Desarrollado por Dev 3.
    /// </summary>
    public class ResultadoRuta
    {
        /// <summary>
        /// Indica si existe al menos un camino transitable entre el origen y destino.
        /// </summary>
        public bool ExisteRuta { get; set; }

        /// <summary>
        /// Mensaje descriptivo del estado del cálculo (éxito o causa de imposibilidad).
        /// </summary>
        public string MensajeEstado { get; set; }

        /// <summary>
        /// Cadena de texto formateada con la secuencia de paradas (ej. "PAR-01 -> PAR-05 -> PAR-09").
        /// </summary>
        public string ResumenCamino { get; set; }

        /// <summary>
        /// Costo monetario total acumulado del pasaje a lo largo de toda la ruta calculada.
        /// </summary>
        public double CostoTotal { get; set; }

        /// <summary>
        /// Tiempo total acumulado de viaje en minutos a lo largo de toda la ruta calculada.
        /// </summary>
        public double TiempoTotalMinutos { get; set; }

        /// <summary>
        /// Secuencia ordenada de códigos de paradas desde el origen hasta el destino
        /// almacenada en la lista enlazada propia (indispensable para pintar el mapa en Windows Forms).
        /// </summary>
        public ListaPropia<string> SecuenciaCodigosParadas { get; set; }

        public ResultadoRuta()
        {
            ExisteRuta = false;
            MensajeEstado = string.Empty;
            ResumenCamino = string.Empty;
            CostoTotal = 0;
            TiempoTotalMinutos = 0;
            SecuenciaCodigosParadas = new ListaPropia<string>();
        }

        public override string ToString()
        {
            if (!ExisteRuta)
            {
                return $"[Sin Ruta] {MensajeEstado}";
            }

            return $"Ruta: {ResumenCamino} | Tiempo: {TiempoTotalMinutos:F1} min | Costo: Q{CostoTotal:F2}";
        }
    }
}

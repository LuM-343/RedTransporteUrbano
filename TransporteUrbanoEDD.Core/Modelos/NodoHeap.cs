namespace TransporteUrbanoEDD.Core.Modelos
{
    /// <summary>
    /// Representa un elemento en el montículo binario mínimo, asociando
    /// el dato con un valor numérico de prioridad.
    /// Desarrollado por Dev 3.
    /// </summary>
    public class NodoHeap<T>
    {
        public T Elemento { get; set; }
        public double Prioridad { get; set; }

        public NodoHeap(T elemento, double prioridad)
        {
            Elemento = elemento;
            Prioridad = prioridad;
        }

        public override string ToString()
        {
            return $"[{Elemento} -> Prioridad: {Prioridad:F2}]";
        }
    }
}

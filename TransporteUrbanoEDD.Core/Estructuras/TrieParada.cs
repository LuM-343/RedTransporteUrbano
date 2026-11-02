namespace TransporteUrbanoEDD.Core.Estructuras;

public class ParHijoTrie
{
    public char Caracter { get; set; }
    public required NodoTrie Nodo { get; set; }
}

public class NodoTrie
{
    public ListaPropia<ParHijoTrie> Hijos { get; set; } = new ListaPropia<ParHijoTrie>();
    public bool EsFinDePalabra { get; set; } = false;
    public required string NombreOriginal { get; set; }
    public required string CodigoParada { get; set; }

    public NodoTrie? ObtenerHijo(char c)
    {
        foreach (var hijo in Hijos)
        {
            if (hijo.Caracter == c) return hijo.Nodo;
        }
        return null;
    }
}

public class TrieParadas
{
    // Inicializar propiedades required en la raíz
    private NodoTrie raiz = new NodoTrie { NombreOriginal = string.Empty, CodigoParada = string.Empty };

    public void Insertar(string nombreParada, string codigo)
    {
        NodoTrie actual = raiz;
        string nombreNormalizado = nombreParada.ToLower();

        foreach (char c in nombreNormalizado)
        {
            NodoTrie? hijo = actual.ObtenerHijo(c);
            if (hijo == null)
            {
                // Inicializar propiedades required en nodos intermedios
                hijo = new NodoTrie { NombreOriginal = string.Empty, CodigoParada = string.Empty };
                actual.Hijos.Agregar(new ParHijoTrie { Caracter = c, Nodo = hijo });
            }
            actual = hijo;
        }
        
        actual.EsFinDePalabra = true;
        actual.NombreOriginal = nombreParada;
        actual.CodigoParada = codigo;
    }

    public ListaPropia<string> Autocompletar(string prefijo)
    {
        ListaPropia<string> resultados = new ListaPropia<string>();
        NodoTrie? actual = raiz;
        string prefijoNormalizado = prefijo.ToLower();

        foreach (char c in prefijoNormalizado)
        {
            actual = actual?.ObtenerHijo(c);
            if (actual == null) return resultados; // No hay coincidencias
        }

        if (actual != null)
        {
            RecolectarPalabras(actual, resultados);
        }
        
        return resultados;
    }

    private void RecolectarPalabras(NodoTrie nodo, ListaPropia<string> resultados)
    {
        if (nodo.EsFinDePalabra)
        {
            resultados.Agregar($"{nodo.NombreOriginal} ({nodo.CodigoParada})");
        }

        foreach (var hijo in nodo.Hijos)
        {
            RecolectarPalabras(hijo.Nodo, resultados);
        }
    }
}
namespace TransporteUrbanoEDD.Core.Estructuras;

// Representa la relación entre un carácter (letra) y su nodo correspondiente.
// Es útil porque no estamos usando un diccionario o arreglo estático para los hijos.
public class ParHijoTrie
{
    public char Caracter { get; set; }
    public required NodoTrie Nodo { get; set; }
}

// Representa cada nodo (o estado) dentro del árbol Trie.
public class NodoTrie
{
    // Lista de las letras que le siguen a este nodo en diferentes palabras.
    public ListaPropia<ParHijoTrie> Hijos { get; set; } = new ListaPropia<ParHijoTrie>();
    
    // Indica si este nodo marca el final de una palabra válida (una parada registrada).
    public bool EsFinDePalabra { get; set; } = false;
    
    // Datos útiles que se guardan solo si el nodo es el final de una palabra.
    public required string NombreOriginal { get; set; }
    public required string CodigoParada { get; set; }

    // Busca entre los hijos de este nodo si ya existe un camino para el carácter dado.
    public NodoTrie? ObtenerHijo(char c)
    {
        foreach (var hijo in Hijos)
        {
            if (hijo.Caracter == c) return hijo.Nodo;
        }
        return null; // Retorna null si la letra no es hija de este nodo
    }
}

// Estructura principal del árbol Trie, optimizada para búsquedas de prefijos y autocompletado.
public class TrieParadas
{
    // El nodo raíz siempre está vacío y sirve como punto de partida.
    private NodoTrie raiz = new NodoTrie { NombreOriginal = string.Empty, CodigoParada = string.Empty };

    // Inserta una nueva parada en el árbol.
    public void Insertar(string nombreParada, string codigo)
    {
        NodoTrie actual = raiz;
        string nombreNormalizado = nombreParada.ToLower(); // Normalizamos a minúsculas para evitar duplicados por mayúsculas.

        // Recorremos letra por letra el nombre de la parada
        foreach (char c in nombreNormalizado)
        {
            NodoTrie? hijo = actual.ObtenerHijo(c);
            
            // Si la letra no existe en el camino actual, creamos un nuevo nodo para ella.
            if (hijo == null)
            {
                hijo = new NodoTrie { NombreOriginal = string.Empty, CodigoParada = string.Empty };
                actual.Hijos.Agregar(new ParHijoTrie { Caracter = c, Nodo = hijo });
            }
            // Avanzamos al siguiente nodo
            actual = hijo;
        }
        
        // Al terminar de insertar todas las letras, marcamos el último nodo como fin de palabra y guardamos los datos.
        actual.EsFinDePalabra = true;
        actual.NombreOriginal = nombreParada;
        actual.CodigoParada = codigo;
    }

    // Inicia el proceso de eliminación de una parada.
    public void Eliminar(string nombreParada)
    {
        if (string.IsNullOrEmpty(nombreParada)) return;
        EliminarRecursivo(raiz, nombreParada.ToLower(), 0);
    }

    // Método recursivo para eliminar una palabra y limpiar nodos huérfanos.
    // Retorna 'true' si el nodo actual debe ser eliminado por su padre.
    private bool EliminarRecursivo(NodoTrie nodo, string palabraNormalizada, int index)
    {
        // Caso base: llegamos al final de la palabra que queremos eliminar
        if (index == palabraNormalizada.Length)
        {
            if (!nodo.EsFinDePalabra) return false; // La palabra no existía realmente

            // "Desmarcamos" el nodo como fin de palabra y limpiamos sus datos
            nodo.EsFinDePalabra = false;
            nodo.NombreOriginal = string.Empty;
            nodo.CodigoParada = string.Empty;

            // Si no tiene hijos, significa que no es parte de otra palabra más larga, así que se puede eliminar.
            return nodo.Hijos.Count == 0; 
        }

        char c = palabraNormalizada[index];
        ParHijoTrie? parHijo = null;

        // Buscamos el nodo correspondiente a la letra actual
        foreach (var hijo in nodo.Hijos)
        {
            if (hijo.Caracter == c)
            {
                parHijo = hijo;
                break;
            }
        }

        if (parHijo == null) return false; // La palabra no se encontró en el árbol

        // Llamada recursiva para avanzar a la siguiente letra
        bool debeEliminarHijo = EliminarRecursivo(parHijo.Nodo, palabraNormalizada, index + 1);

        // Si el hijo devolvió 'true', significa que ya no sirve y debemos borrarlo de nuestra lista
        if (debeEliminarHijo)
        {
            nodo.Hijos.Eliminar(parHijo); 
            
            // Retornamos true si este nodo también quedó inútil (no es fin de otra palabra y ya no tiene hijos)
            return !nodo.EsFinDePalabra && nodo.Hijos.Count == 0;
        }

        return false;
    }
    
    // Busca todas las paradas que comiencen con el texto ingresado.
    public ListaPropia<string> Autocompletar(string prefijo)
    {
        ListaPropia<string> resultados = new ListaPropia<string>();
        NodoTrie? actual = raiz;
        string prefijoNormalizado = prefijo.ToLower();

        // 1. Navegamos hasta el nodo donde termina el prefijo ingresado
        foreach (char c in prefijoNormalizado)
        {
            actual = actual?.ObtenerHijo(c);
            if (actual == null) return resultados; // Si el camino se rompe, no hay coincidencias
        }

        // 2. A partir de ahí, recolectamos todas las palabras posibles
        if (actual != null)
        {
            RecolectarPalabras(actual, resultados);
        }
        
        return resultados;
    }

    // Navega recursivamente por todos los hijos de un nodo guardando las palabras completas que encuentre.
    private void RecolectarPalabras(NodoTrie nodo, ListaPropia<string> resultados)
    {
        // Si el nodo actual es una parada registrada, la agregamos a los resultados
        if (nodo.EsFinDePalabra)
        {
            resultados.Agregar($"{nodo.NombreOriginal} ({nodo.CodigoParada})");
        }

        // Repetimos el proceso para todos los hijos de este nodo (búsqueda en profundidad)
        foreach (var hijo in nodo.Hijos)
        {
            RecolectarPalabras(hijo.Nodo, resultados);
        }
    }
}
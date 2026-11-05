# Red de Transporte Urbano

Proyecto de Laboratorio No. 2 · Estructura de Datos II · Universidad Rafael Landívar, Campus Quetzaltenango · Ing. Manuel Gustavo Rojas

Sistema en C# (Windows Forms) que administra las paradas y rutas de buses de Quetzaltenango. Permite registrar paradas y rutas, localizar una parada por su código, sugerir nombres mientras el usuario escribe, calcular la ruta más rápida o más económica entre dos paradas y verificar si dos paradas pertenecen a la misma zona conectada antes de calcular una ruta.

Todas las estructuras son implementaciones propias. No se usa ninguna colección nativa de .NET (`List<T>`, `Dictionary`, `HashSet`, `SortedSet`, `PriorityQueue`); la única excepción permitida es `Queue<T>` como control de orden dentro del recorrido BFS.

## Integrantes

| | Integrante | Responsabilidad principal |
| --- | --- | --- |
| I1 | Arenz Peláez | `ListaPropia<T>`, `Parada`, Tabla Hash, Union-Find, carga y escritura de CSV, Pestaña 1 |
| I2 | José Noguera | Trie, Orquestador, Pestaña 2, manuales |
| I3 | Luis Velásquez | MinHeap, Grafo (BFS, DFS, Dijkstra), Pestaña 3 y mapa |

## Flujo integrado

Las cuatro estructuras trabajan en un mismo flujo, coordinado por `OrquestadorTransporte`:

1. **Trie:** el usuario escribe parte del nombre y el Trie sugiere la parada y su código.
2. **Tabla Hash:** con el código se obtiene la parada completa en O(1) promedio.
3. **Union-Find:** se verifica en O(α(n)) que origen y destino estén en la misma zona conectada. Si no lo están, se rechaza la consulta sin ejecutar Dijkstra.
4. **Grafo:** Dijkstra calcula la ruta óptima por tiempo o por costo.

Al registrar o eliminar una parada, `CargadorCSV` actualiza las cuatro estructuras a la vez para que nunca queden desincronizadas.

## Estructuras utilizadas y justificación

| Estructura | Uso en el sistema | Operación clave | Por qué se eligió |
| --- | --- | --- | --- |
| `ListaPropia<T>` | Base de las demás: buckets, hijos del Trie, adyacencia del grafo | Agregar O(1) | Reemplaza `List<T>`; la referencia a la cola evita recorrer la lista al insertar |
| Tabla Hash (`TablaHashParadas`) | Buscar paradas por código | Buscar O(1) promedio | Es la operación más frecuente; una lista requeriría O(n). Función de Horner (base 31), encadenamiento separado y rehashing a 2m + 1 cuando el factor de carga supera 0.70 |
| Union-Find (`UnionFindZonas`) | Zonas conectadas de la red | ¿Misma zona? O(α(n)) | Responde en tiempo casi constante y evita ejecutar Dijkstra en rutas imposibles. Compresión de caminos y unión por rango |
| Trie (`TrieParadas`) | Autocompletado por nombre | Autocompletar O(largo del prefijo + resultados) | Encuentra todos los nombres con un prefijo sin recorrer todas las paradas |
| Grafo (`GrafoTransporte`) | Rutas entre paradas | Dijkstra O((V + E) log V) | Lista de adyacencia propia; soporta vías de un sentido y doble sentido con dos pesos (tiempo y costo) |
| MinHeap (`MinHeapPropio`) | Cola de prioridad de Dijkstra | Extraer mínimo O(log n) | Reemplaza `PriorityQueue` |

## Organización del código

```
TransporteUrbanoEDD.slnx
├── TransporteUrbanoEDD.Core/      Lógica (C# puro, multiplataforma)
│   ├── Estructuras/               ListaPropia, TablaHash, UnionFind, Trie, Grafo, MinHeap, Orquestador
│   ├── Modelos/                   Parada, AristaRuta, NodoHeap, ResultadoRuta
│   ├── Archivos/                  CargadorCSV (lectura y escritura de CSV)
│   └── Datos/                     paradas.csv, rutas.csv
├── TransporteUrbanoEDD.UI/        Interfaz Windows Forms
│   ├── FormPrincipal.cs           Ventana con las 3 pestañas
│   └── Controles/                 Pestaña 1 (paradas y zonas), Pestaña 2 (buscador), Pestaña 3 (rutas y mapa)
└── Pruebas/                       Pruebas de consola de todas las estructuras
```

## Instrucciones de ejecución

**Requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). La interfaz gráfica requiere **Windows**; la lógica y las pruebas de consola corren también en macOS y Linux.

```bash
git clone https://github.com/LuM-343/RedTransporteUrbano.git
cd RedTransporteUrbano

# Programa completo (Windows)
dotnet run --project TransporteUrbanoEDD.UI

# Pruebas de consola (cualquier sistema)
dotnet run --project Pruebas
```

Al abrir, el programa carga automáticamente los datos de prueba. Desde la Pestaña 1 también se pueden cargar otros archivos CSV con el selector de archivos y guardar el estado actual.

## Datos de prueba

**Ubicación:** `TransporteUrbanoEDD.Core/Datos/`. Al compilar se copian junto al ejecutable, en la carpeta `Datos/`.

| Archivo | Contenido | Formato |
| --- | --- | --- |
| `paradas.csv` | 30 paradas de Quetzaltenango y alrededores | `Codigo,Nombre,X,Y,Zona` |
| `rutas.csv` | 52 rutas (39 de doble sentido, 13 de un solo sentido) | `Origen,Destino,Tiempo,Costo,Bidireccional` |

- La primera línea es el encabezado. Las líneas vacías y las que empiezan con `#` se ignoran.
- Los decimales usan punto (`3.5`) y los nombres no llevan comas.
- `X` y `Y` son la posición en el mapa; `Tiempo` está en minutos y `Costo` en quetzales.
- `Bidireccional` acepta `true`/`false`, `1`/`0` o `si`/`no`.
- Las paradas se cargan antes que las rutas. Las líneas con error se reportan con su número y no detienen la carga.

La red tiene **2 zonas conectadas**: la principal (27 paradas) y una zona rural aislada (Almolonga, Zunil y Cantel), que permite demostrar el Union-Find.

### Consultas de prueba

| Opción | Consulta | Resultado esperado |
| --- | --- | --- |
| Búsqueda (Hash) | `PAR-01` | Parque Central, bucket 13 |
| Prefijo (Trie) | `mer` | Mercado La Democracia, Mercado Las Flores |
| Recorrido (BFS) | desde `PAR-19` | PAR-19, PAR-20, PAR-29 |
| Ruta óptima (Dijkstra) | `PAR-01` → `PAR-16` por tiempo | PAR-01 → 02 → 12 → 14 → 16, 29.1 min |
| Conectividad (Union-Find) | `PAR-01` y `PAR-20` | Zonas distintas |

El listado completo de 25 consultas (5 por opción) con sus resultados esperados está en el manual técnico que se encuentra en la carpeta de Manuales y Diagramas

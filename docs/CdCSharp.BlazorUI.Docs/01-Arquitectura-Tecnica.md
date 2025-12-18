# 1. Arquitectura Técnica de CdCSharp.BlazorUI

Esta sección describe los pilares de la arquitectura de la librería, esencial para colaboradores que deseen extender o modificar el código base.

## 1.1. Bases de Componentes: Abstracción y Estilización

El diseño de componentes se basa en clases abstractas para asegurar la coherencia en la estructura, el manejo de estilos y la extensibilidad.

### 1.1.1. UIComponentBase

Es la base de todos los componentes de la librería y gestiona la composición de estilos y atributos.

* **Captura de Atributos (`AdditionalAttributes`)**:
    * Utiliza el atributo `[Parameter(CaptureUnmatchedValues = true)]` para capturar cualquier atributo HTML no definido como parámetro (`class`, `style`, `data-*`, `aria-*`, etc.).
    * Almacena estos atributos en el diccionario `AdditionalAttributes`.
* **Composición de Clases CSS (`ComputedCssClasses`)**:
    * La propiedad `ComputedCssClasses` es crucial para la estilización. Combina, en orden de prioridad:
        1.  Clases CSS específicas del componente, obtenidas de `GetAdditionalCssClasses()`.
        2.  Clases CSS proporcionadas por el usuario a través del atributo `class` en `AdditionalAttributes`.
    * **Proceso de Fusión**: Las clases proporcionadas por el usuario **se añaden** a las clases base definidas por el componente.
* **Estilos en Línea**:
    * El método `GetAdditionalInlineStyles()` permite a los componentes inyectar estilos en línea programáticamente (ej. colores dinámicos).
    * Estos estilos se fusionan con cualquier estilo en línea proporcionado por el usuario a través del atributo `style` en `AdditionalAttributes`.

### 1.1.2. UIVariantComponentBase

Extiende `UIComponentBase` e introduce el concepto de **variantes** (diseño) y el sistema de plantillas (*templates*) de renderizado.

* **Parámetro `Variant`**: Acepta un objeto de tipo `TVariant` (que hereda de `Variant`) para seleccionar una variación de presentación.
* **Prioridad de Renderizado**: El método `BuildRenderFragment` determina qué marcado renderizar:
    1. Si no se encuentra un registro personalizado, utiliza las plantillas predefinidas del componente (`BuiltInTemplates`), como `Default`.
    2.  Busca un `RenderFragment` registrado para la variante seleccionada en el `VariantRegistry`.
    
* **Inyección de Dependencias**: Depende del servicio `IVariantRegistry<TComponent, TVariant>` para el soporte de variantes externas.



### 1.1.3. Sistema de Variantes (`Variant` y `VariantRegistry`)

Este sistema permite a los usuarios **sustituir el marcado HTML interno** de un componente sin modificar la librería.

* **Clase `Variant`**: Contiene la identificación de la variación.
    * Las variantes incorporadas (ej. `Default`) son campos `static readonly`.
    * Las variantes personalizadas se crean mediante `Variant.Custom(string name)`.
* **`IVariantRegistry<TComponent, TVariant>`**: Un servicio *singleton* registrado en la DI que mapea una `TVariant` a un `RenderFragment`.
* **Prioridad de Atributos en Templates**: Por consistencia, las plantillas de variantes incorporadas de la libreria, priorizan sus atributos html frente a los que pueda declarar el usuario del componente. Es decir, si la plantilla define un atributo y el usuario declara el mismo atributo, prioriza el de la plantilla. Por esto las plantillas de la libreria no incorporan directamente el atributo class y en su lugar son calculados, para permitir al usuario definirlo si es necesario.
  Al crear plantillas personalizadas existe opción a decidir que atributos priorizar, definiendo @attributes antes o después de los atributos deseados. Esto es esencial para entender la lógica de estilos en variantes.

## 1.2. Interoperabilidad con JavaScript (JS Interop)

Para interactuar con el DOM o librerías JS, se utiliza una abstracción basada en módulos para garantizar la eficiencia y la correcta gestión de recursos.

### 1.2.1. ModuleJsInteropBase

Es la clase base para manejar la carga asíncrona y la disposición de módulos JS.

* **Carga de Módulo**: Se realiza bajo demanda a través de `Lazy<Task<IJSObjectReference>> ModuleTask`. El módulo se carga mediante `jsRuntime.InvokeAsync<IJSObjectReference>("import", jsModuleContentPath)`, lo que garantiza una carga optimizada del módulo JS en el cliente (ES Modules).
* **Disposición Asíncrona (`IAsyncDisposable`)**: La implementación de `DisposeAsync()` es obligatoria. Llama a `Module.DisposeAsync()` para liberar la referencia del objeto JS en el lado del navegador.

### 1.2.2. Ejemplo: ThemeJsInterop

Este servicio interactúa con el script de theming.
* **Métodos**: Expone métodos de alto nivel (como `SetThemeAsync(string theme)` y `ToggleThemeAsync(string[] allowedThemes)`) que abstraen las llamadas de bajo nivel a JS, gestionando el estado persistente del tema.

## 1.3. Code Generation (Generación de Código)

La carpeta `CdCSharp.BlazorUI.Core.CodeGeneration` contiene herramientas que generan código C# en tiempo de compilación.

* **`ColorCodeGenerator`**: Genera una clase `UIColor` estatica que permite acceder a todos los colores de System.Drawing.Color. Añadiendo variantes Lighten y Darken para cada color. (p.e UIColor.Aquamarine.Darken3))
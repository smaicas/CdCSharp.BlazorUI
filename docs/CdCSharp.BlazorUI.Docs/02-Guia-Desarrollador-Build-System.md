# 2. ⚙️ Guía del Desarrollador: Build System y Dependencias

El sistema de compilación de `CdCSharp.BlazorUI` utiliza una cadena de herramientas de .NET Core y Node.js para generar activos estáticos óptimos.

## 2.1. Componentes del Build System

El proceso se compone de tres partes principales:

1.  **`CdCSharp.BlazorUI.targets` (MSBuild)**: Define la secuencia de tareas que garantizan que los activos se generen *antes* de que el proyecto Blazor se compile.
2.  **`CdCSharp.BlazorUI.BuildTools` (.NET CLI)**: Una herramienta de consola que actúa como *shim* (puente) para ejecutar `npm` y `vite` de manera programática, asegurando la inicialización correcta del entorno de Node.js.
3.  **Vite/Node.js (Front-end)**: Se encarga de la agregación, minificación y *tree-shaking* de los archivos CSS y JS.



## 2.2. Flujo de Compilación (Targets de MSBuild)

El archivo `CdCSharp.BlazorUI.targets` inyecta la lógica de *build* necesaria.

| Target | Condición / Propósito | Lógica (Comando) |
| :--- | :--- | :--- |
| `CheckNodeJsInstalled` | **Pre-Requisito**: Verifica la presencia de Node.js. | `node --version` |
| `EnsureBuildToolsCompiled` | **Setup**: Asegura que el proyecto `BuildTools` esté compilado. | `MSBuild` del proyecto `BuildTools` |
| `InitializeBlazorUIConfig` | **Configuración**: Genera o actualiza los archivos de configuración de front-end (`package.json`, `vite.config.js`). | `BuildTools init` |
| **`GenerateBlazorUIAssets`** | **Principal (CSS/JS)**: Compila y minifica todos los activos estáticos y los coloca en la carpeta `wwwroot`. | `BuildTools all` |
| `GenerateBlazorUIThemes` | **Auxiliar (CSS)**: Solo genera y copia los archivos de tema. Útil en entornos de desarrollo donde solo se modifica el CSS/Theming. | `BuildTools themes` |
| `CleanBlazorUIAssets` | **Limpieza**: Elimina `node_modules`, `CssBundle`, `wwwroot\css\blazorui.css` y archivos de configuración. | `BuildTools clean` |

### 2.2.1. Inclusión de Activos

El target principal (`GenerateBlazorUIAssets`) es crítico ya que asegura que:
1.  Los archivos JS/CSS compilados se guarden en una subcarpeta bajo `wwwroot`.
2.  El archivo `.csproj` incluye estos activos generados en el paquete NuGet final (`<Content Remove=...>` y `<None Include=...>` garantizan la correcta distribución).

## 2.3. Configuración de Bundling (Vite)

La configuración de Vite está diseñada para optimizar el rendimiento de la librería.

* **Output de JS**: Utiliza `format: 'es'` y la opción `preserveEntrySignatures: 'strict'` en `vite.config.js`. Esto es esencial para que Blazor JS Interop pueda importar los módulos JavaScript individuales (ej. `ThemeJsInterop.js`) de forma dinámica con `import(...)` sin que Vite los fusione.
* **CSS Principal**: El archivo `main.css` actúa como el punto de entrada, importando:
    * `reset.css`: CSS Reset y definición de tokens de diseño como variables CSS (`--bui-font-base`, `--bui-space-1`).
    * `themes.css`: Definición de las variables de color dentro de las clases de tema (`.bui-theme-light`, `.bui-theme-dark`).
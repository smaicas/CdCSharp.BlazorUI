# 4. Guía de Uso: Instalación y Configuración

Esta sección describe cómo configurar su aplicación Blazor para consumir la librería `CdCSharp.BlazorUI`.

## 4.1. Instalación del Paquete NuGet

Añada la referencia al paquete NuGet `CdCSharp.BlazorUI` en su proyecto Blazor:

```csharp
dotnet add package CdCSharp.BlazorUI
```

## 4.2. Registro de Servicios

En el archivo de inicio de su aplicación (típicamente `Program.cs`), debe registrar los servicios de la librería llamando al método de extensión `AddBlazorUI` en la colección de servicios:

```csharp
// Program.cs
// ...
using CdCSharp.BlazorUI.Core.Extensions; // Asegúrese de importar este namespace

var builder = WebApplication.CreateBuilder(args);
// ...

// Registrar todos los servicios de la librería (Theming, Variant Registries, etc.)
builder.Services.AddBlazorUI(); 

// ...
```
> **Nota**: `AddBlazorUI` registra servicios de theming (`IThemeJsInterop`) y los registros de variantes (`IVariantRegistry`) para los componentes base.

## 4.3. Inclusión de Estilos y Scripts

Debe incluir los activos compilados de CSS y JavaScript en su archivo `App.razor` (o equivalente).

1.  **CSS Principal**: Contiene el CSS Reset, las variables de tema y los estilos base de los componentes.

**Ejemplo para Blazor Web/Server (típico en `App.razor` o `index.html`):**

```csharp
<link rel="stylesheet" href="_content/CdCSharp.BlazorUI/css/blazorui.css" />
```

## 4.4. Uso del Layout Base (Opcional)

Para una inicialización automática del sistema de temas, su layout principal puede heredar de `UIBlazorLayout`:

```csharp
@inherits CdCSharp.BlazorUI.Components.Layout.UIBlazorLayout

@Body 
```
> **Nota**: El componente `UIBlazorLayout` utiliza el servicio `IThemeJsInterop` para llamar a `InitializeAsync()` en el primer renderizado, configurando el tema inicial en el DOM.
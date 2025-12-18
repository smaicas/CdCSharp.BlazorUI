# 3. 🧩 Guía del Desarrollador: Desarrollo de Componentes

Esta guía detalla el proceso para construir nuevos componentes que se integren correctamente en la arquitectura de la librería.

## 3.1. Proceso de Creación de un Componente

### 3.1.1. Herencia y Tipado

Todo componente debe heredar de `UIVariantComponentBase<TComponent, TVariant>` (o `UIComponentBase` si no requiere variantes).

* **Tipado Genérico**: Si está creando un botón, la herencia sería:
```csharp
@inherits UIVariantComponentBase<UIButton, UIButtonVariant>
```

### 3.1.2. Definición de la API (Parámetros)

* Utilice `[Parameter]` para todas las propiedades públicas.
* Para estilos que afecten la paleta, use tipos de la librería como `CssColor` o enums de *size* (ej. `IconSize`).
* **Prioridad de Atributos**: Asegúrese de utilizar `@attributes="AdditionalAttributes"` en el elemento raíz del componente para que los atributos no mapeados (clases personalizadas, IDs, etc.) proporcionados por el usuario sean aplicados correctamente.

### 3.1.3. Lógica de Estilos

La lógica de estilización principal ocurre al sobrescribir los siguientes métodos:

| Método | Retorno | Propósito |
| :--- | :--- | :--- |
| `GetAdditionalCssClasses()` | `string` | Clases dinámicas basadas en los parámetros (ej. `ui-button--disabled` si `Disabled` es true). |
| `GetAdditionalInlineStyles()` | `string` | Estilos dinámicos inyectados por la lógica (ej. `background-color: ...`). |

### 3.1.4. Plantilla por Defecto

El marcado HTML del componente se define en el método `RenderDefault()`, que debe devolver un `RenderFragment`.

```csharp
protected override RenderFragment RenderDefault() => __builder =>
{
    // ... Lógica de renderizado ...
    <button @attributes="AdditionalAttributes"
            class="@ComputedCssClasses"
            @onclick="OnClick"
            disabled="@Disabled">
        @ChildContent
    </button>
};
```

## 3.2. Implementación de Componentes Clave

### 3.2.1. UIButton (Manejo de Contenido)

El componente `UIButton` maneja el renderizado de iconos de forma condicional, usando `UISvgIcon`.

* **Icon-Only**: Si `Text` es nulo, `GetAdditionalCssClasses()` añade la clase `ui-button--icon-only` para aplicar un tamaño y *padding* adecuados.
* **Estilos Personalizados**: Utiliza `GetAdditionalInlineStyles()` para convertir `BackgroundColor` y `Color` (`CssColor` objects) a formatos CSS (ej. `rgba(...)`) e inyectarlos en el elemento `style`.

### 3.2.2. Layout Base (`UIBlazorLayout`)

Este componente base para layouts es crucial para la inicialización del theming.

* Hereda de `LayoutComponentBase`.
* En `OnAfterRenderAsync(bool firstRender)`, utiliza el servicio `IThemeJsInterop` para llamar a `InitializeAsync()`.
* **`InitializeAsync()`**: Lee el tema preferido del *Local Storage* o del sistema operativo y lo aplica como clase CSS al cuerpo del documento (`<body>`), asegurando la persistencia del tema.
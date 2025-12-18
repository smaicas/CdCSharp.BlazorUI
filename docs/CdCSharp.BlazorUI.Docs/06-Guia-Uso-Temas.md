# 6. Guía de Uso: Theming y Personalización

El sistema de theming de `CdCSharp.BlazorUI` se basa en la definición de variables CSS para la paleta de colores. Esto permite cambiar el aspecto de la aplicación de forma global.

## 6.1. Paletas de Temas

La librería define dos temas principales: `light` y `dark`.

**Temas Predefinidos**

| Tema | ID | Background | Foreground (Texto) | Primary |
| :--- | :--- | :--- | :--- | :--- |
| LightTheme | `light` | `#FFFFFF` | `#0F172A` | Paleta definida |
| DarkTheme | `dark` | `#0F172A` | `#F1F5F9` | Paleta definida |

## 6.2. Variables CSS de la Paleta

Todos los estilos de la librería utilizan variables CSS para los colores de la paleta.

| Variable CSS | Propósito |
| :--- | :--- |
| `--palette-primary` | Color principal de la aplicación. |
| `--palette-secondary` | Color secundario/acento. |
| `--palette-background` | Color de fondo principal (cuerpo de la app). |
| `--palette-surface` | Color de fondo de contenedores y superficies. |
| `--palette-foreground` | Color del texto y elementos en primer plano. |

Para aplicar un tema, el componente `UIBlazorLayout` o la lógica JS Interop inserta el ID del tema como una clase en el `<body>` o en el elemento raíz.

## 6.3. Personalización de Paletas (Avanzado)

Para personalizar la paleta de colores o crear un nuevo tema:

1. **Personalizar Temas Existentes:**
Cree un nuevo archivo CSS (o SCSS) en su proyecto y sobrescriba las variables CSS de los temas incorporados. Por defecto se usa el tema 'dark' si no se especifica otro.

```css
/* CustomTheme.css */
:root {
  /* === Theme palettes === */
  --dark-primary: rgba(96,165,250,1);
  --dark-primarycontrast: rgba(15,23,42,1);
  --dark-secondary: rgba(167,139,250,1);
  --dark-secondarycontrast: rgba(15,23,42,1);
  --dark-background: rgba(15,23,42,1);
  --dark-surface: rgba(30,41,59,1);
  --dark-foreground: rgba(241,245,249,1);
  --dark-error: rgba(239,68,68,1);
  --dark-success: rgba(16,185,129,1);
  --dark-warning: rgba(245,158,11,1);
  --dark-info: rgba(59,130,246,1);
  --light-primary: rgba(59,130,246,1);
  --light-primarycontrast: rgba(255,255,255,1);
  --light-secondary: rgba(139,92,246,1);
  --light-secondarycontrast: rgba(255,255,255,1);
  --light-background: rgba(255,255,255,1);
  --light-surface: rgba(248,250,252,1);
  --light-foreground: rgba(30,41,59,1);
  --light-error: rgba(239,68,68,1);
  --light-success: rgba(16,185,129,1);
  --light-warning: rgba(245,158,11,1);
  --light-info: rgba(59,130,246,1);
}
```

2. **Crear un nuevo tema:**
Si desea añadir un tema extra (por ejemplo, `custom`), defina las variables CSS correspondientes:

```css
  --custom-primary: rgba(96,165,250,1);
  --custom-primarycontrast: rgba(15,23,42,1);
  --custom-secondary: rgba(167,139,250,1);
  --custom-secondarycontrast: rgba(15,23,42,1);
  --custom-background: rgba(15,23,42,1);
  --custom-surface: rgba(30,41,59,1);
  --custom-foreground: rgba(241,245,249,1);
  --custom-error: rgba(239,68,68,1);
  --custom-success: rgba(16,185,129,1);
  --custom-warning: rgba(245,158,11,1);
  --custom-info: rgba(59,130,246,1);

html[data-theme="custom"] {
  --palette-primary: var(--custom-primary);
  --palette-primarycontrast: var(--custom-primarycontrast);
  --palette-secondary: var(--custom-secondary);
  --palette-secondarycontrast: var(--custom-secondarycontrast);
  --palette-background: var(--custom-background);
  --palette-surface: var(--custom-surface);
  --palette-foreground: var(--custom-foreground);
  --palette-error: var(--custom-error);
  --palette-success: var(--custom-success);
  --palette-warning: var(--custom-warning);
  --palette-info: var(--custom-info);
}
```
**Utilice el Interop JS** para activar su nuevo tema, inyectando el ID de tema (`custom` en este ejemplo):

```csharp
@inject IThemeJsInterop ThemeInterop

<UIButton OnClick="@(() => ThemeInterop.SetThemeAsync("custom"))" 
          Text="Activar Tema Custom" />
```

Puede también hacer switch entre varios temas
```csharp
@inject IThemeJsInterop ThemeInterop

<UIButton OnClick="SwitchTheme" 
          Text="Switch Theme />

@code{
    private async Task SwitchTheme()
    {
        await ThemeInterop.ToggleThemeAsync(new[] { "light", "dark", "custom" });
    }
}
```

## 6.4. Personalización del Marcado (Variantes Custom)

Si necesita cambiar completamente el HTML que renderiza un componente para un caso de uso específico, puede registrar una plantilla de variante.

1.  **Defina una Variante Custom en una clase (p.e: MyCustomVariants.cs)**:

    ```csharp
    public static UIButtonVariant Glass => UIButtonVariant.Custom("Glass");
    ```

2.  **Registre el Template en `Program.cs`**:

    Utilice el método de extensión `AddBlazorUIVariants` para vincular su plantilla personalizada (`RenderGlassButton`) a la variante y el componente.

    ```csharp
    // Program.cs
    builder.Services.AddBlazorUIVariants(builder => 
        builder.For<UIButton, UIButtonVariant>()
               .Register(UIButtonVariant.Glass, RenderGlassButton));
    // ...
    ```

3.  **Defina el `RenderFragment` (Template)**:
    El fragmento de renderizado debe recibir el componente como argumento para acceder a sus parámetros (`Text`, `OnClick`, `AdditionalAttributes`, etc.).

    ```csharp
    // En una clase de templates o en el componente si es simple
    private RenderFragment RenderGlassButton(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                class="@(component.ComputedCssClasses) btn-glass"
                @onclick="component.OnClick"
                disabled="component.Disabled">
            <span class="effect-glass"></span>
            @component.Text
        </button>
    };
    ```

4.  **Uso**:

    ```csharp
    <UIButton Text="Botón Glass" Variant="MyCustomVariants.Glass" />
    ```
    > **Es importante entender la priorización de atributos al definir plantillas personalizadas**:
    De forma sencilla: "Los atributos que van después prevalecen". Es decir, si definimos atributos antes de @attributes="component.AdditionalAttributes", los atributos que se especifiquen al usar el componente(que estarán en AdditionalAttributes) prevalecerán sobre los definidos en la plantilla. Si se definen después, prevalecerán los de la plantilla.
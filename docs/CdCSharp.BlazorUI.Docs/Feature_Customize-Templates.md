# Guía de Creación de Templates Personalizadas - BlazorUI

## Introducción

El sistema de variantes de BlazorUI permite personalizar completamente la apariencia y estructura de cualquier componente mediante templates personalizadas. Las variantes son versiones alternativas de un componente que mantienen su funcionalidad pero cambian su presentación visual.

## Métodos de Registro de Variantes

BlazorUI ofrece tres métodos complementarios para registrar variantes personalizadas:

### 1. Builder Pattern (Configuración Fluida)

El método más directo y recomendado para la mayoría de casos:
```csharp
// Program.cs
builder.Services.AddBlazorUI(options =>
{
    options.ConfigureButton()
        .AddVariant("Glass", ButtonTemplates.GlassTemplate)
        .AddVariant("Outline", ButtonTemplates.OutlineTemplate)
        .AddVariant(MyCustomButtonVariants.Gradient, ButtonTemplates.GradientTemplate);
        
    options.ConfigureIcon()
        .AddVariant("Spinning", IconTemplates.SpinningTemplate);
});
```

**Ventajas:**
- Sintaxis fluida e intuitiva
- IntelliSense completo
- Registro centralizado
- Fácil de entender y mantener

### 2. Descubrimiento por Atributos

Ideal para templates distribuidas en múltiples archivos:
```csharp
// Templates/ButtonTemplates.cs
using CdCSharp.BlazorUI.Components.Attributes;

public static class ButtonTemplates
{
    [ButtonVariant("Glass")]
    public static RenderFragment GlassTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                class="@($"{component.ComputedCssClasses} btn-glass")"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };

    [ButtonVariant("Outline")]
    public static RenderFragment OutlineTemplate(UIButton component) => __builder =>
    {
        // Template implementation
    };
}

// Program.cs
builder.Services
    .AddBlazorUI()
    .AddVariantsFromAssembly(typeof(Program).Assembly);
```

**Ventajas:**
- Templates auto-descubiertas
- Menos configuración manual
- Ideal para librerías de componentes

### 3. Provider Pattern

Para escenarios complejos con múltiples variantes relacionadas:
```csharp
// Providers/CustomButtonProvider.cs
public class CustomButtonProvider : IVariantProvider<UIButton, UIButtonVariant>
{
    public IEnumerable<(UIButtonVariant variant, Func<UIButton, RenderFragment> template)> GetVariants()
    {
        yield return (UIButtonVariant.Custom("Primary"), PrimaryTemplate);
        yield return (UIButtonVariant.Custom("Secondary"), SecondaryTemplate);
        yield return (UIButtonVariant.Custom("Danger"), DangerTemplate);
    }

    private RenderFragment PrimaryTemplate(UIButton component) => __builder => { /* ... */ };
    private RenderFragment SecondaryTemplate(UIButton component) => __builder => { /* ... */ };
    private RenderFragment DangerTemplate(UIButton component) => __builder => { /* ... */ };
}

// Program.cs
builder.Services
    .AddBlazorUI()
    .AddVariantsFromType<CustomButtonProvider>();
```

**Ventajas:**
- Agrupa variantes relacionadas
- Lógica compartida entre variantes
- Testeable independientemente

## Combinación de Métodos

Puedes combinar los tres métodos según tus necesidades:
```csharp
builder.Services
    .AddBlazorUI(options =>
    {
        // Registro manual para casos específicos
        options.ConfigureButton()
            .AddVariant("Special", SpecialButtonTemplate);
    })
    .AddVariantsFromAssembly(typeof(Program).Assembly)  // Descubrimiento automático
    .AddVariantsFromType<ThemeButtonProvider>();        // Providers específicos
```

**Orden de prioridad:**
1. Registro manual (Builder) - máxima prioridad
2. Attributes
3. Providers
4. Assembly scanning

## Creación de Variantes Personalizadas

### Opción 1: Usar variantes dinámicas
```csharp
// Uso directo sin crear una clase
<UIButton Variant="@UIButtonVariant.Custom("Glass")" Text="Click me" />
```

### Opción 2: Crear una clase de variantes (RECOMENDADO)
```csharp
// Variants/MyCustomButtonVariants.cs
public class MyCustomButtonVariants : UIButtonVariant
{
    public MyCustomButtonVariants(string name) : base(name) { }
    
    public static readonly UIButtonVariant Glass = Custom("Glass");
    public static readonly UIButtonVariant Outline = Custom("Outline");
    public static readonly UIButtonVariant Gradient = Custom("Gradient");
}
```

## Creación de Templates

### Conceptos clave

#### ComputedCssClasses
`ComputedCssClasses` contiene todas las clases CSS calculadas:
- Clases base del componente (ej: `ui-button`)
- Clases de la variante (ej: `ui-button--glass`)  
- Clases del usuario pasadas via `AdditionalAttributes`

#### @attributes y el orden importa
```csharp
// Templates/ButtonTemplates.cs
public static class ButtonTemplates
{
    // Añade clases preservando las existentes (RECOMENDADO)
    [ButtonVariant("Glass")]
    public static RenderFragment GlassTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                class="@($"{component.ComputedCssClasses} btn-glass")"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            <span class="glass-effect"></span>
            @component.Text
        </button>
    };

    // Template con estructura compleja
    [ButtonVariant("Gradient")]
    public static RenderFragment GradientTemplate(UIButton component) => __builder =>
    {
        <div class="gradient-button-wrapper">
            <button @attributes="component.AdditionalAttributes"
                    class="@($"{component.ComputedCssClasses} btn-gradient")"
                    @onclick="component.OnClick"
                    disabled="@component.Disabled">
                
                @if (!string.IsNullOrEmpty(component.LeadingIcon))
                {
                    <svg class="icon-leading" viewBox="0 0 24 24">
                        @((MarkupString)component.LeadingIcon)
                    </svg>
                }
                
                <span>@component.Text</span>
                
                @if (!string.IsNullOrEmpty(component.TrailingIcon))
                {
                    <svg class="icon-trailing" viewBox="0 0 24 24">
                        @((MarkupString)component.TrailingIcon)
                    </svg>
                }
            </button>
            
            <div class="gradient-shadow"></div>
        </div>
    };
}
```

## Uso Final
```razor
@page "/demo"
@using YourProject.Variants

<h3>Botones con Variantes Personalizadas</h3>

<!-- Con variantes tipadas -->
<UIButton Variant="@MyCustomButtonVariants.Glass" 
          Text="Glass Button"
          OnClick="@HandleClick" />

<!-- Con string (funciona si está registrada) -->
<UIButton Variant="@UIButtonVariant.Custom("Outline")" 
          Text="Outline Button" />

<!-- Con atributos adicionales -->
<UIButton Variant="@MyCustomButtonVariants.Gradient"
          Text="Save Changes"
          LeadingIcon="@Icons.Save"
          AdditionalAttributes="@(new() { 
              ["class"] = "mt-4",
              ["data-test-id"] = "save-btn" 
          })" />

@code {
    private async Task HandleClick(MouseEventArgs e)
    {
        // Handle click
    }
}
```

## Migración desde el Sistema Anterior

Si tienes código usando el sistema anterior de registro manual:
```csharp
// Antes
using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider
        .GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
    registry.Register(variant, template);
}

// Ahora (opción 1 - recomendada)
builder.Services.AddBlazorUI(options =>
{
    options.ConfigureButton()
        .AddVariant(variant, template);
});

// Ahora (opción 2 - con atributos)
[ButtonVariant("VariantName")]
public static RenderFragment Template(UIButton component) => ...
```

## Mejores Prácticas

1. **Usa variantes tipadas** para evitar errores con strings
2. **Preserva ComputedCssClasses** para mantener las clases del componente
3. **Organiza las templates** en archivos separados por componente
4. **Combina métodos** según la complejidad de tu proyecto
5. **Documenta tus variantes** para facilitar su uso
6. **Testea las variantes** independientemente

## Resumen

El nuevo sistema de variantes ofrece múltiples formas de personalizar componentes:
- **Builder Pattern**: Simple y directo
- **Attributes**: Descubrimiento automático
- **Providers**: Agrupación lógica
- **Combinación**: Flexibilidad total

Elige el método que mejor se adapte a tu proyecto y escala según tus necesidades.
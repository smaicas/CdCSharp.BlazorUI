# Guía de Creación de Templates Personalizadas - BlazorUI

## Introducción

El sistema de variantes de BlazorUI permite personalizar completamente la apariencia y estructura de cualquier componente mediante templates personalizadas. Las variantes son versiones alternativas de un componente que mantienen su funcionalidad pero cambian su presentación visual.

## Creación de Variantes Personalizadas

### Opción 1: Usar variantes dinámicas
```csharp
// Uso directo sin crear una clase
var glassVariant = UIButtonVariant.Custom("Glass");
```

### Opción 2: Crear una clase de variantes (RECOMENDADO para proyectos grandes)
```csharp
// Variants/MyCustomButtonVariants.cs
using CdCSharp.BlazorUI.Components.Generic.Button;

public class MyCustomButtonVariants : UIButtonVariant
{
    public MyCustomButtonVariants(string name) : base(name)
    {
    }
    
    public static readonly UIButtonVariant Glass = Custom("Glass");
    public static readonly UIButtonVariant Outline = Custom("Outline");
    public static readonly UIButtonVariant Gradient = Custom("Gradient");
    public static readonly UIButtonVariant Ghost = Custom("Ghost");
}
```

Ventajas de este enfoque:
- Variantes tipadas y centralizadas
- IntelliSense en el IDE
- Evita errores de strings mágicos
- Fácil refactoring

## Registro de Variantes

### Opción 1: Directamente en Program.cs
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorUI();

var app = builder.Build();

// Registrar variantes después de construir el app
using (var scope = app.Services.CreateScope())
{
    var buttonRegistry = scope.ServiceProvider
        .GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
    
    // Registrar usando la clase de variantes
    buttonRegistry.Register(MyCustomButtonVariants.Glass, ButtonTemplates.GlassTemplate);
    buttonRegistry.Register(MyCustomButtonVariants.Outline, ButtonTemplates.OutlineTemplate);
    buttonRegistry.Register(MyCustomButtonVariants.Gradient, ButtonTemplates.GradientTemplate);
}

app.Run();
```

### Opción 2: Mediante método de extensión (más organizado)
```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomVariants(this IServiceCollection services)
    {
        services.AddBlazorUI();
        
        // Registrar después de que el contenedor esté construido
        services.AddTransient<IStartupFilter>(provider => new VariantStartupFilter());
        
        return services;
    }
}

// Startup/VariantStartupFilter.cs
public class VariantStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            RegisterVariants(builder.ApplicationServices);
            next(builder);
        };
    }
    
    private void RegisterVariants(IServiceProvider services)
    {
        var buttonRegistry = services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        
        buttonRegistry.Register(MyCustomButtonVariants.Glass, ButtonTemplates.GlassTemplate);
        buttonRegistry.Register(MyCustomButtonVariants.Outline, ButtonTemplates.OutlineTemplate);
        buttonRegistry.Register(MyCustomButtonVariants.Gradient, ButtonTemplates.GradientTemplate);
    }
}

// Program.cs
builder.Services.AddCustomVariants();
```

## Creación de Templates con Sintaxis Razor

### Conceptos clave

#### ComputedCssClasses
`ComputedCssClasses` contiene todas las clases CSS calculadas:
- Clases base del componente (ej: `ui-button`)
- Clases de la variante (ej: `ui-button--glass`)  
- Clases del usuario pasadas via `AdditionalAttributes`

#### @attributes y el orden importa

El orden de `@attributes` en la template determina la prioridad:
```razor
@using CdCSharp.BlazorUI.Components.Generic.Button

@code {
    // CASO 1: Template básica - hereda todas las clases
    // Las clases vienen completamente de AdditionalAttributes
    public RenderFragment BasicTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };
    
    // CASO 2: Añadir clases preservando las existentes
    // Al poner class DESPUÉS de @attributes, sobrescribimos el atributo
    // Usamos ComputedCssClasses para preservar las clases existentes
    public RenderFragment GlassTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                class="@($"{component.ComputedCssClasses} btn-glass")"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };
    
    // CASO 3: Sobrescribir completamente las clases (casos especiales)
    // Al poner class DESPUÉS de @attributes sin usar ComputedCssClasses
    public RenderFragment OverrideTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                class="btn-completely-different"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };
    
    // CASO 4: Atributos data - usuario tiene prioridad
    // @attributes al FINAL permite al usuario sobrescribir
    public RenderFragment DataTemplate(UIButton component) => __builder =>
    {
        <button data-variant="template-default"
                data-test="template-value"
                @attributes="component.AdditionalAttributes"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };
    
    // CASO 5: Atributos data - template tiene prioridad  
    // @attributes PRIMERO, los atributos de la template ganan
    public RenderFragment DataPriorityTemplate(UIButton component) => __builder =>
    {
        <button @attributes="component.AdditionalAttributes"
                data-variant="template-forced"
                data-test="template-priority"
                @onclick="component.OnClick"
                disabled="@component.Disabled">
            @component.Text
        </button>
    };
}
```

### Template completa de ejemplo
```razor
@* Templates/ButtonTemplates.razor *@
@using CdCSharp.BlazorUI.Components.Generic.Button

@code {
    public static RenderFragment GradientTemplate(UIButton component) => __builder =>
    {
        <div class="gradient-button-wrapper">
            <button @attributes="component.AdditionalAttributes"
                    class="@($"{component.ComputedCssClasses} btn-gradient")"
                    data-variant="gradient"
                    @onclick="component.OnClick"
                    disabled="@component.Disabled">
                
                <span class="gradient-bg"></span>
                
                <span class="btn-content">
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
                </span>
            </button>
            
            <div class="gradient-shadow"></div>
        </div>
    };
}
```

## Uso Final

### Con variantes dinámicas
```razor
@page "/demo-dynamic"

<UIButton Variant="@UIButtonVariant.Custom("Glass")" 
          Text="Glass Button"
          OnClick="@HandleClick" />
```

### Con clase de variantes (RECOMENDADO)
```razor
@page "/demo"
@using YourProject.Variants

<h3>Botones con Variantes Personalizadas</h3>

<!-- Variante Glass -->
<UIButton Variant="@MyCustomButtonVariants.Glass" 
          Text="Glass Button"
          OnClick="@HandleClick" />

<!-- Variante con clases adicionales -->
<UIButton Variant="@MyCustomButtonVariants.Outline" 
          Text="Outline Button"
          AdditionalAttributes="@(new() { 
              ["class"] = "mt-4 shadow-lg",
              ["data-test-id"] = "outline-btn" 
          })" />

<!-- Variante con iconos -->
<UIButton Variant="@MyCustomButtonVariants.Gradient"
          Text="Save Changes"
          LeadingIcon="@Icons.Save"
          Disabled="@isProcessing" />

@code {
    private bool isProcessing = false;
    
    private async Task HandleClick(MouseEventArgs e)
    {
        isProcessing = true;
        await Task.Delay(1000);
        isProcessing = false;
    }
}
```

## Resumen de conceptos clave

1. **Orden de @attributes**: Determina qué tiene prioridad (usuario vs template)
2. **ComputedCssClasses**: Contiene todas las clases calculadas, úsalo para preservarlas
3. **Sobrescribir vs Extender**: Decide si quieres reemplazar o añadir clases
4. **Variantes tipadas**: Considera crear una clase para evitar strings mágicos
5. **Registro al inicio**: Las variantes deben registrarse durante el startup de la aplicación
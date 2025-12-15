using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    // Contiene TODAS las clases: componente + usuario
    public string ComputedCssClasses { get; private set; } = string.Empty;

    public virtual IEnumerable<string> GetAdditionalCssClasses() => [];
    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        // Obtener clases del componente
        string componentClasses = string.Join(" ", GetAdditionalCssClasses());

        // Obtener clases del usuario (si existen)
        string userClasses = AdditionalAttributes.TryGetValue("class", out object? existingClass)
            ? existingClass.ToString() ?? string.Empty
            : string.Empty;

        // Combinar todas las clases
        ComputedCssClasses = string.IsNullOrWhiteSpace(userClasses)
            ? componentClasses
            : $"{componentClasses} {userClasses}".Trim();

        // Actualizar AdditionalAttributes con las clases combinadas
        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            AdditionalAttributes["class"] = ComputedCssClasses;
        }

        // Merge de estilos
        MergeAttribute("style", string.Join(";", GetAdditionalInlineStyles()
                                                  .Select(kv => $"{kv.Key}: {kv.Value}")), ";");

        base.OnParametersSet();
    }

    private void MergeAttribute(string key, string newValue, string separator)
    {
        if (string.IsNullOrWhiteSpace(newValue)) { return; }

        if (AdditionalAttributes.TryGetValue(key, out object? existing))
        {
            AdditionalAttributes[key] = $"{newValue}{separator}{existing}";
        }
        else
        {
            AdditionalAttributes[key] = newValue;
        }
    }
}
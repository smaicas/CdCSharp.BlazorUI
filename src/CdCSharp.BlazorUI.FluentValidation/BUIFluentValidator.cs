using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CdCSharp.BlazorUI.FormsFluentValidation;

public class BUIFluentValidator : ComponentBase, IDisposable
{
    [CascadingParameter] private EditContext? EditContext { get; set; }

    [Parameter] public Type? ValidatorType { get; set; }
    [Parameter] public bool ValidateOnFieldChanged { get; set; } = true;

    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    private ValidationMessageStore? _messageStore;
    private IValidator? _validator;

    protected override void OnInitialized()
    {
        if (EditContext is null)
            throw new InvalidOperationException(
                $"{nameof(BUIFluentValidator)} must be used inside an {nameof(EditForm)}.");

        _messageStore = new ValidationMessageStore(EditContext);

        // Resolve validator: by DI or by explicit type.
        _validator = ValidatorType is not null
            ? (IValidator?)ServiceProvider.GetService(ValidatorType)
            : ServiceProvider.GetService(typeof(IValidator<>).MakeGenericType(EditContext.Model.GetType())) as IValidator;

        if (_validator is null && ValidatorType is not null)
            _validator = (IValidator?)ActivatorUtilities.CreateInstance(ServiceProvider, ValidatorType);

        if (_validator is null)
            throw new InvalidOperationException(
                $"No se encontró un validador para el modelo {EditContext.Model.GetType().FullName}.");

        EditContext.OnValidationRequested += HandleValidationRequested;

        if (ValidateOnFieldChanged)
            EditContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        if (_messageStore is null || _validator is null || EditContext is null) return;

        _messageStore.Clear();
        ValidationResult result = _validator.Validate(new ValidationContext<object>(EditContext.Model));
        AddErrors(result);
        EditContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (_messageStore is null || _validator is null || EditContext is null) return;

        FieldIdentifier fieldIdentifier = e.FieldIdentifier;
        _messageStore.Clear(fieldIdentifier);

        // Validate only changed field: FluentValidation does not support native field validation, so errors are filtered out after validating the entire model.
        string[] properties = new[] { fieldIdentifier.FieldName };
        ValidationContext<object> context = new(
            fieldIdentifier.Model,
            new PropertyChain(),
            new MemberNameValidatorSelector(properties));

        ValidationResult result = _validator.Validate(context);
        AddErrors(result);
        EditContext.NotifyValidationStateChanged();
    }

    private void AddErrors(ValidationResult result)
    {
        if (_messageStore is null || EditContext is null) return;

        foreach (ValidationFailure? error in result.Errors)
        {
            FieldIdentifier fieldIdentifier = ToFieldIdentifier(EditContext, error.PropertyName);
            _messageStore.Add(fieldIdentifier, error.ErrorMessage);
        }
    }

    // Conversion of property paths from FluentValidation to Blazor FieldIdentifier
    private static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
    {
        object? obj = editContext.Model;
        string[] segments = propertyPath.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length - 1; i++)
        {
            string segment = segments[i].TrimEnd(']');
            PropertyInfo? prop = obj.GetType().GetProperty(segment)
                ?? obj.GetType().GetProperty("Item"); // soporte indexadores básico

            if (prop is null)
                break;

            obj = prop.GetValue(obj);
            if (obj is null)
                return new FieldIdentifier(editContext.Model, propertyPath);
        }

        return new FieldIdentifier(obj, segments[^1].TrimEnd(']'));
    }

    public void Dispose()
    {
        if (EditContext is not null)
        {
            EditContext.OnValidationRequested -= HandleValidationRequested;
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }
        _messageStore?.Clear();
    }
}
namespace CdCSharp.BlazorUI.Components.Layout;

public interface IModalService
{
    IReadOnlyList<ModalState> ActiveModals { get; }

    event Action? OnChange;

    Task<TResult?> ShowDialogAsync<TComponent, TResult>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    Task ShowDialogAsync<TComponent>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent;

    Task<TResult?> ShowDrawerAsync<TComponent, TResult>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;

    Task ShowDrawerAsync<TComponent>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent;

    Task CloseAsync();
    Task CloseAllAsync();
}
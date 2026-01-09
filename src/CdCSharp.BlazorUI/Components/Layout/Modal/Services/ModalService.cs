namespace CdCSharp.BlazorUI.Components.Layout.Modal.Services;

public class ModalService : IModalService
{
    private readonly List<ModalState> _modals = [];

    public IReadOnlyList<ModalState> ActiveModals => _modals.AsReadOnly();

    public event Action? OnChange;

    public async Task<TResult?> ShowDialogAsync<TComponent, TResult>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Dialog,
            parameters,
            options ?? new DialogOptions());

        return await ShowAndWaitAsync<TResult>(state);
    }

    public Task ShowDialogAsync<TComponent>(
        object? parameters = null,
        DialogOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Dialog,
            parameters,
            options ?? new DialogOptions());

        return ShowAsync(state);
    }

    public async Task<TResult?> ShowDrawerAsync<TComponent, TResult>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Drawer,
            parameters,
            options ?? new DrawerOptions());

        return await ShowAndWaitAsync<TResult>(state);
    }

    public Task ShowDrawerAsync<TComponent>(
        object? parameters = null,
        DrawerOptions? options = null)
        where TComponent : IModalContent
    {
        ModalState state = CreateModalState<TComponent>(
            ModalType.Drawer,
            parameters,
            options ?? new DrawerOptions());

        return ShowAsync(state);
    }

    public async Task CloseAsync()
    {
        if (_modals.Count == 0) return;

        ModalState current = _modals[^1];
        await CloseModalAsync(current);
    }

    public async Task CloseAllAsync()
    {
        while (_modals.Count > 0)
        {
            await CloseAsync();
        }
    }

    private ModalState CreateModalState<TComponent>(
        ModalType type,
        object? parameters,
        ModalOptionsBase options)
        where TComponent : IModalContent
    {
        string id = $"modal-{Guid.NewGuid():N}";

        ModalReference reference = new(id, OnModalClose);

        Dictionary<string, object?>? paramDict = null;
        if (parameters != null)
        {
            paramDict = parameters.GetType()
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(parameters));
        }

        return new ModalState
        {
            Id = id,
            Type = type,
            ComponentType = typeof(TComponent),
            Reference = reference,
            Options = options,
            Parameters = paramDict
        };
    }

    private Task ShowAsync(ModalState state)
    {
        HideCurrentModal();
        _modals.Add(state);
        NotifyChange();
        return Task.CompletedTask;
    }

    private async Task<TResult?> ShowAndWaitAsync<TResult>(ModalState state)
    {
        HideCurrentModal();
        _modals.Add(state);
        NotifyChange();

        try
        {
            object? result = await state.Reference.Result;
            return result is TResult typed ? typed : default;
        }
        catch (TaskCanceledException)
        {
            return default;
        }
    }

    private void HideCurrentModal()
    {
        if (_modals.Count > 0)
        {
            _modals[^1].IsVisible = false;
        }
    }

    private void ShowPreviousModal()
    {
        if (_modals.Count > 0)
        {
            _modals[^1].IsVisible = true;
        }
    }

    private async Task CloseModalAsync(ModalState state)
    {
        state.IsAnimatingOut = true;
        NotifyChange();

        await Task.Delay(200);

        _modals.Remove(state);
        ShowPreviousModal();
        NotifyChange();
    }

    private void OnModalClose(ModalReference reference)
    {
        ModalState? state = _modals.FirstOrDefault(m => m.Reference == reference);
        if (state != null)
        {
            _ = CloseModalAsync(state);
        }
    }

    private void NotifyChange()
    {
        OnChange?.Invoke();
    }
}
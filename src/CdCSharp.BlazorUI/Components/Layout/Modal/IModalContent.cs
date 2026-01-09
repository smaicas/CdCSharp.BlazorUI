namespace CdCSharp.BlazorUI.Components.Layout;

public interface IModalContent
{
    ModalReference ModalRef { get; set; }
}

public interface IModalContent<TParameters> : IModalContent
{
    TParameters Parameters { get; set; }
}

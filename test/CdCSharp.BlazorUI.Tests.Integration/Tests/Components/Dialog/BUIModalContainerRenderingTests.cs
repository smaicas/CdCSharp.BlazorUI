using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BUIModalContainer")]
public class BUIModalContainerRenderingTests
{
    private static ModalState CreateDialogState(string title = "Test Dialog") =>
        new()
        {
            Id = "test-modal-1",
            Type = ModalType.Dialog,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-modal-1", _ => { }),
            Options = new DialogOptions { Title = title },
            IsVisible = true,
        };

    private static ModalState CreateDrawerState(DrawerPosition position = DrawerPosition.Right) =>
        new()
        {
            Id = "test-drawer-1",
            Type = ModalType.Drawer,
            ComponentType = typeof(DummyModalContent),
            Reference = new ModalReference("test-drawer-1", _ => { }),
            Options = new DrawerOptions { Position = position },
            IsVisible = true,
        };

    private sealed class DummyModalContent : Microsoft.AspNetCore.Components.ComponentBase, IModalContent
    {
        [Microsoft.AspNetCore.Components.Parameter]
        public ModalReference ModalRef { get; set; } = default!;

        // IModalContent requires this property — delegated to ModalRef
        ModalReference IModalContent.ModalReference
        {
            get => ModalRef;
            set => ModalRef = value;
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalContainer> cut = ctx.Render<BUIModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState()));

        // Assert
        cut.Find(".bui-modal-dialog").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drawer_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalContainer> cut = ctx.Render<BUIModalContainer>(p => p
            .Add(c => c.Modal, CreateDrawerState()));

        // Assert
        cut.Find(".bui-modal-drawer").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalContainer> cut = ctx.Render<BUIModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState("My Title")));

        // Assert
        cut.Find(".bui-modal-dialog__title").TextContent.Should().Be("My Title");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Drawer_Position_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalContainer> cut = ctx.Render<BUIModalContainer>(p => p
            .Add(c => c.Modal, CreateDrawerState(DrawerPosition.Left)));

        // Assert
        cut.Find(".bui-modal-drawer--left").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Visible_Class_When_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalContainer> cut = ctx.Render<BUIModalContainer>(p => p
            .Add(c => c.Modal, CreateDialogState()));

        // Assert
        cut.Find(".bui-modal-container--visible").Should().NotBeNull();
    }
}

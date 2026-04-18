using Bunit;
using Microsoft.Extensions.DependencyInjection;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Stubs;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BUIModalHost")]
public class BUIModalHostInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Host_After_Close(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIModalHost> cut = ctx.Render<BUIModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        cut.FindAll(".bui-modal-host").Should().HaveCount(1);

        // Act
        await modalService.CloseAllAsync();

        // Assert
        cut.FindAll(".bui-modal-host").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Stack_Multiple_Modals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIModalHost> cut = ctx.Render<BUIModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bui-modal-container").Should().HaveCount(2);
    }
}

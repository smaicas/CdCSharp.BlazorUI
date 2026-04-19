using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Stubs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BUIModalHost")]
public class BUIModalHostRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_No_Modals(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIModalHost> cut = ctx.Render<BUIModalHost>();

        // Assert
        cut.FindAll(".bui-modal-host").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Host_When_Modal_Shown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIModalHost> cut = ctx.Render<BUIModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bui-modal-host").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Modal_Container_When_Modal_Shown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIModalHost> cut = ctx.Render<BUIModalHost>();
        IModalService modalService = ctx.Services.GetRequiredService<IModalService>();

        // Act
        await modalService.ShowDialogAsync<TestModalContent_TestStub>();

        // Assert
        cut.FindAll(".bui-modal-container").Should().HaveCount(1);
    }
}

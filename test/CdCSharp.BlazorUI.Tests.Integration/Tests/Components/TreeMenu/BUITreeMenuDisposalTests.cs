using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Disposal", "BUITreeMenu")]
public class BUITreeMenuDisposalTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null);

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, [new MenuItem("a", "Alpha")])
            .Add(c => c.KeySelector, m => m.Key));

        // Act + Assert
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_After_Navigation_Subscription_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — render with nested items so nav subscription + expand are tested
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, [
                new MenuItem("parent", "Parent", [new MenuItem("child", "Child")])
            ])
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        cut.Find("[role='menuitem']").Click(); // expand

        // Act + Assert — dispose unsubscribes LocationChanged, no exception
        Func<Task> act = () => { cut.Instance.Dispose(); return Task.CompletedTask; };
        await act.Should().NotThrowAsync();
    }
}

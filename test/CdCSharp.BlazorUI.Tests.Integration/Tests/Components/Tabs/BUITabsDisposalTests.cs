using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Disposal", "BUITabs")]
public class BUITabsDisposalTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Dispose_Without_Exception(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        RenderFragment twoTabs = b =>
        {
            b.OpenComponent<BUITab>(0);
            b.AddAttribute(1, "Id", "t1");
            b.AddAttribute(2, "Label", "T1");
            b.CloseComponent();
            b.OpenComponent<BUITab>(3);
            b.AddAttribute(4, "Id", "t2");
            b.AddAttribute(5, "Label", "T2");
            b.CloseComponent();
        };

        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, twoTabs));

        cut.FindAll("[role='tab']").Should().HaveCount(2);

        // Act + Assert — dispose does not throw
        var act = async () => await cut.Instance.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Tab_When_Unregistered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — start with two tabs
        bool showSecond = true;
        IRenderedComponent<BUITabs> cut = null!;
        cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BUITab>(0);
                b.AddAttribute(1, "Id", "t1");
                b.AddAttribute(2, "Label", "T1");
                b.CloseComponent();
                if (showSecond)
                {
                    b.OpenComponent<BUITab>(3);
                    b.AddAttribute(4, "Id", "t2");
                    b.AddAttribute(5, "Label", "T2");
                    b.CloseComponent();
                }
            }));

        cut.FindAll("[role='tab']").Should().HaveCount(2);

        // Act — remove second tab
        showSecond = false;
        cut.Render(p => p.Add(c => c.ChildContent, b =>
        {
            b.OpenComponent<BUITab>(0);
            b.AddAttribute(1, "Id", "t1");
            b.AddAttribute(2, "Label", "T1");
            b.CloseComponent();
        }));

        // Assert
        cut.FindAll("[role='tab']").Should().HaveCount(1);
    }
}

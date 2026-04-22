using CdCSharp.BlazorUI.Abstractions;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

/// <summary>
/// Direct unit tests for <see cref="DelayedActionHandler" /> and <see cref="TimingUtilities" />.
/// Pins debounce semantics, cancellation, disposal guard, and post-dispose no-op.
/// </summary>
[Trait("Core", "DelayedActionHandler")]
public class DelayedActionHandlerTests
{
    // ─────────── DelayedActionHandler ───────────

    [Fact]
    public async Task ExecuteWithDelayAsync_Should_Invoke_Action_After_Delay()
    {
        using DelayedActionHandler handler = new();
        int invocations = 0;

        await handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(20));

        invocations.Should().Be(1);
    }

    [Fact]
    public async Task Cancel_Before_Delay_Elapses_Should_Skip_Action()
    {
        using DelayedActionHandler handler = new();
        int invocations = 0;

        Task run = handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(200));

        await Task.Delay(20);
        handler.Cancel();
        await run;

        invocations.Should().Be(0);
    }

    [Fact]
    public async Task ReTrigger_Should_Reset_Countdown_Debounce()
    {
        using DelayedActionHandler handler = new();
        int invocations = 0;

        Task first = handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(200));

        await Task.Delay(30);

        Task second = handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(50));

        await Task.WhenAll(first, second);

        invocations.Should().Be(1);
    }

    [Fact]
    public async Task Dispose_Should_Cancel_In_Flight_Delay()
    {
        DelayedActionHandler handler = new();
        int invocations = 0;

        Task run = handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(500));

        await Task.Delay(20);
        handler.Dispose();
        await run;

        invocations.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteWithDelayAsync_After_Dispose_Should_Be_NoOp()
    {
        DelayedActionHandler handler = new();
        handler.Dispose();
        int invocations = 0;

        await handler.ExecuteWithDelayAsync(() =>
        {
            invocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(10));

        invocations.Should().Be(0);
    }

    [Fact]
    public void Dispose_Should_Be_Idempotent()
    {
        DelayedActionHandler handler = new();

        handler.Dispose();
        Action secondDispose = () => handler.Dispose();

        secondDispose.Should().NotThrow();
    }

    [Fact]
    public void Cancel_Without_Pending_Action_Should_Be_NoOp()
    {
        using DelayedActionHandler handler = new();

        Action cancel = () => handler.Cancel();

        cancel.Should().NotThrow();
    }

    [Fact]
    public async Task Second_Trigger_Should_Cancel_First_Action_Callback()
    {
        using DelayedActionHandler handler = new();
        int firstInvocations = 0;
        int secondInvocations = 0;

        Task first = handler.ExecuteWithDelayAsync(() =>
        {
            firstInvocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(500));

        await Task.Delay(20);

        Task second = handler.ExecuteWithDelayAsync(() =>
        {
            secondInvocations++;
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(20));

        await Task.WhenAll(first, second);

        firstInvocations.Should().Be(0);
        secondInvocations.Should().Be(1);
    }

    // ─────────── TimingUtilities.Debounce ───────────

    [Fact]
    public async Task Debounce_Action_Should_Fire_Only_Last_Call_Within_Window()
    {
        int invocations = 0;
        Action debounced = TimingUtilities.Debounce(
            () => Interlocked.Increment(ref invocations),
            TimeSpan.FromMilliseconds(100));

        debounced();
        debounced();
        debounced();

        await Task.Delay(250);

        invocations.Should().Be(1);
    }

    [Fact]
    public async Task Debounce_Action_Should_Fire_After_Quiet_Period()
    {
        int invocations = 0;
        Action debounced = TimingUtilities.Debounce(
            () => Interlocked.Increment(ref invocations),
            TimeSpan.FromMilliseconds(50));

        debounced();
        await Task.Delay(150);
        debounced();
        await Task.Delay(150);

        invocations.Should().Be(2);
    }

    [Fact]
    public async Task Debounce_Generic_Should_Pass_Latest_Argument()
    {
        int captured = 0;
        Action<int> debounced = TimingUtilities.Debounce<int>(
            x => Interlocked.Exchange(ref captured, x),
            TimeSpan.FromMilliseconds(100));

        debounced(1);
        debounced(2);
        debounced(3);

        await Task.Delay(250);

        captured.Should().Be(3);
    }

    [Fact]
    public void Debounce_Null_Action_Should_Throw()
    {
        Action call = () => TimingUtilities.Debounce(null!, TimeSpan.FromMilliseconds(10));

        call.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Debounce_Null_Generic_Action_Should_Throw()
    {
        Action call = () => TimingUtilities.Debounce<int>(null!, TimeSpan.FromMilliseconds(10));

        call.Should().Throw<ArgumentNullException>();
    }
}

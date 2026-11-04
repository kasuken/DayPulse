using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class InsightsTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 11, 4);

    public InsightsTests()
    {
        Services.AddMudServices();
        Services.AddDayPulseLocalization();
        Services.AddSingleton<TimeProvider>(
            new FixedTimeProvider(new DateTimeOffset(2026, 11, 4, 12, 0, 0, TimeSpan.Zero)));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Insights_shows_real_period_statistics_and_accessible_mood_data()
    {
        Services.AddSingleton<ICheckInRepository>(
            new InMemoryCheckInRepository(
            [
                CreateCheckIn(Today.AddDays(-1), mood: -1, energy: 2, sleepHours: 7.5),
                CreateCheckIn(Today, mood: 2, energy: 4, sleepHours: 8)
            ]));

        var cut = Render<Insights>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Your check-in summaries", cut.Markup);
            Assert.Contains("0.5", cut.Markup);
            Assert.Contains("3.0", cut.Markup);
            Assert.Equal("true", cut.Find("[data-testid='insights-period-30']").GetAttribute("aria-pressed"));
            Assert.Equal(2, cut.FindAll("[data-testid='insights-mood-table'] tbody tr").Count);
            Assert.NotEmpty(cut.FindAll(".insights-chart__point"));
            Assert.NotEmpty(cut.FindAll(".insights-chart__segment"));
            Assert.Single(cut.FindAll(".insights-page"));
            Assert.Equal(12, cut.FindAll(".insights-metrics .insights-metric").Count);
        });

        cut.Find("[data-testid='insights-period-7']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("true", cut.Find("[data-testid='insights-period-7']").GetAttribute("aria-pressed"));
            Assert.Equal("false", cut.Find("[data-testid='insights-period-30']").GetAttribute("aria-pressed"));
            Assert.Equal(2, cut.FindAll("[data-testid='insights-mood-table'] tbody tr").Count);
        });
    }

    [Fact]
    public void Insights_shows_an_empty_state_when_no_check_ins_have_been_saved()
    {
        Services.AddSingleton<ICheckInRepository>(new InMemoryCheckInRepository([]));

        var cut = Render<Insights>();

        cut.WaitForAssertion(() =>
        {
            Assert.Single(cut.FindAll("[data-testid='insights-empty']"));
            Assert.Contains("Save a check-in", cut.Markup);
            Assert.Empty(cut.FindAll("[data-testid='insights-mood-table']"));
        });
    }

    [Fact]
    public void Insights_exposes_a_loading_state_until_local_check_ins_are_available()
    {
        var repository = new DelayedCheckInRepository();
        Services.AddSingleton<ICheckInRepository>(repository);

        var cut = Render<Insights>();

        Assert.Contains("Loading your check-ins", cut.Markup);
        repository.Complete([CreateCheckIn(Today, mood: 3)]);

        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("Loading your check-ins", cut.Markup);
            Assert.Single(cut.FindAll("[data-testid='insights-mood-table'] tbody tr"));
        });
    }

    [Fact]
    public void Insights_allows_retry_after_local_check_in_load_failure()
    {
        var repository = new FailOnceCheckInRepository([CreateCheckIn(Today, mood: 4)]);
        Services.AddSingleton<ICheckInRepository>(repository);

        var cut = Render<Insights>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Your check-ins could not be loaded.", cut.Markup);
            Assert.NotEmpty(cut.FindAll("[data-testid='insights-retry']"));
        });

        cut.Find("[data-testid='insights-retry']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(2, repository.CallCount);
            Assert.Single(cut.FindAll("[data-testid='insights-mood-table'] tbody tr"));
        });
    }

    private static CheckIn CreateCheckIn(
        DateOnly date,
        int mood,
        int? energy = null,
        double? sleepHours = null)
    {
        return new CheckIn(
            Guid.NewGuid().ToString(),
            date,
            mood,
            energy,
            sleepHours: sleepHours,
            createdAt: DateTimeOffset.UnixEpoch);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private class InMemoryCheckInRepository(IReadOnlyList<CheckIn> checkIns) : ICheckInRepository
    {
        public virtual Task<IReadOnlyList<CheckIn>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(checkIns);
        }

        public Task<IReadOnlyList<CheckIn>> GetBetweenAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CheckIn?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<CheckIn?> GetByDateAsync(
            DateOnly entryDate,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task CreateAsync(
            CheckIn checkIn,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(
            CheckIn checkIn,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class DelayedCheckInRepository : InMemoryCheckInRepository
    {
        private readonly TaskCompletionSource<IReadOnlyList<CheckIn>> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public DelayedCheckInRepository() : base([])
        {
        }

        public override Task<IReadOnlyList<CheckIn>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _completion.Task.WaitAsync(cancellationToken);
        }

        public void Complete(IReadOnlyList<CheckIn> checkIns)
        {
            _completion.SetResult(checkIns);
        }
    }

    private sealed class FailOnceCheckInRepository(IReadOnlyList<CheckIn> checkIns)
        : InMemoryCheckInRepository(checkIns)
    {
        public int CallCount { get; private set; }

        public override Task<IReadOnlyList<CheckIn>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return CallCount == 1
                ? Task.FromException<IReadOnlyList<CheckIn>>(
                    new InvalidOperationException("The initial read failed."))
                : base.GetAllAsync(cancellationToken);
        }
    }
}

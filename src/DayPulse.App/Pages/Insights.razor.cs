using MudBlazor;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using Microsoft.AspNetCore.Components;

namespace DayPulse.App.Pages;

public partial class Insights : IAsyncDisposable
{
    private static readonly InsightsPeriod[] SupportedPeriods =
    [
        InsightsPeriod.SevenDays,
        InsightsPeriod.ThirtyDays,
        InsightsPeriod.NinetyDays
    ];

    private readonly CancellationTokenSource _disposeCts = new();
    private IReadOnlyList<CheckIn> _checkIns = [];
    private InsightsStatistics? _statistics;
    private InsightsPeriod _selectedPeriod = InsightsPeriod.ThirtyDays;
    private bool _isLoading = true;
    private bool _hasLoadError;
    private bool _isDisposed;

    private InsightsPeriodStatistics CurrentPeriod =>
        _statistics?.ForPeriod(_selectedPeriod)
        ?? throw new InvalidOperationException("Insights are not available before data has loaded.");

    private IReadOnlyList<MoodChartPoint> ChartPoints
    {
        get
        {
            if (_statistics is null)
            {
                return [];
            }

            var summary = CurrentPeriod;
            var width = Math.Max(1, (int)summary.Period - 1);
            return summary.MoodTrend
                .Select(point => new MoodChartPoint(
                    point.Date,
                    24 + ((point.Date.DayNumber - summary.StartDate.DayNumber) / (double)width * 592),
                    18 + ((5 - point.Mood) * 16)))
                .ToArray();
        }
    }

    private IReadOnlyList<MoodChartSegment> ChartSegments
    {
        get
        {
            var points = ChartPoints;
            var segments = new List<MoodChartSegment>();
            for (var index = 1; index < points.Count; index++)
            {
                if (points[index].Date.DayNumber == points[index - 1].Date.DayNumber + 1)
                {
                    segments.Add(new MoodChartSegment(
                        points[index - 1],
                        points[index]));
                }
            }

            return segments;
        }
    }

    private string ChartLine => string.Join(' ', ChartPoints.Select(point =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{point.X:0.##},{point.Y}")));

    private IReadOnlyList<InsightMetric> Metrics
    {
        get
        {
            var period = CurrentPeriod;
            var statistics = _statistics!;
            return
            [
                new(Localizer["InsightsStatsAverageMood"], FormatAverage(period.AverageMood), Icons.Material.Outlined.SentimentSatisfied, true),
                new(Localizer["InsightsStatsCheckInTotal"], FormatService.FormatNumber(period.CheckInCount, "N0"), Icons.Material.Outlined.EventAvailable, true),
                new(Localizer["InsightsStatsCurrentStreak"], FormatDayCount(statistics.CurrentStreak), Icons.Material.Outlined.LocalFireDepartment, true),
                new(Localizer["InsightsStatsAverageEnergy"], FormatAverage(period.AverageEnergy), Icons.Material.Outlined.Bolt),
                new(Localizer["InsightsStatsAverageAnxiety"], FormatAverage(period.AverageAnxiety), Icons.Material.Outlined.Waves),
                new(Localizer["InsightsStatsAverageFocus"], FormatAverage(period.AverageFocus), Icons.Material.Outlined.CenterFocusStrong),
                new(Localizer["InsightsStatsAverageSleep"], FormatAverage(period.AverageSleepHours), Icons.Material.Outlined.Bedtime),
                new(Localizer["InsightsStatsTotalSleep"], FormatAverage(period.TotalSleepHours), Icons.Material.Outlined.NightsStay),
                new(Localizer["InsightsStatsActivitySelections"], FormatService.FormatNumber(period.ActivitySelections, "N0"), Icons.Material.Outlined.DirectionsRun),
                new(Localizer["InsightsStatsLongestStreak"], FormatDayCount(statistics.LongestStreak), Icons.Material.Outlined.EmojiEvents),
                new(Localizer["InsightsStatsBestMood"], FormatExtremum(period.BestMood), Icons.Material.Outlined.TrendingUp, Compact: true),
                new(Localizer["InsightsStatsLowestMood"], FormatExtremum(period.LowestMood), Icons.Material.Outlined.TrendingDown, Compact: true),
            ];
        }
    }

    protected override Task OnInitializedAsync()
    {
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        _isLoading = true;
        _hasLoadError = false;

        try
        {
            _checkIns = await CheckIns.GetAllAsync(_disposeCts.Token);
            var today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
            _statistics = InsightsStatisticsService.Calculate(_checkIns, today);
        }
        catch (OperationCanceledException) when (_disposeCts.IsCancellationRequested)
        {
            // Component disposal cancels any in-flight local storage read.
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Could not load local check-ins for insights.");
            _checkIns = [];
            _statistics = null;
            _hasLoadError = true;
        }
        finally
        {
            if (!_isDisposed)
            {
                _isLoading = false;
            }
        }
    }

    private void SelectPeriod(InsightsPeriod period)
    {
        if (SupportedPeriods.Contains(period))
        {
            _selectedPeriod = period;
        }
    }

    private string PeriodLabel(InsightsPeriod period)
    {
        return period switch
        {
            InsightsPeriod.SevenDays => Localizer["InsightsStatsSevenDays"],
            InsightsPeriod.ThirtyDays => Localizer["InsightsStatsThirtyDays"],
            InsightsPeriod.NinetyDays => Localizer["InsightsStatsNinetyDays"],
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, "Unsupported insights period.")
        };
    }

    private string FormatAverage(decimal? value)
    {
        return value is decimal average
            ? FormatService.FormatNumber(average, "N1")
            : Localizer["InsightsStatsNoData"];
    }

    private string FormatExtremum(MoodExtremum? extremum)
    {
        return extremum is null
            ? Localizer["InsightsStatsNoData"]
            : $"{FormatService.FormatDate(extremum.Date)} · {FormatService.FormatNumber(extremum.Mood, "N0")}";
    }

    private string FormatDayCount(int count)
    {
        var formattedCount = FormatService.FormatNumber(count, "N0");
        return count == 1
            ? Localizer["InsightsStatsOneDay", formattedCount]
            : Localizer["InsightsStatsManyDays", formattedCount];
    }

    public ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _disposeCts.Cancel();
        _disposeCts.Dispose();
        return ValueTask.CompletedTask;
    }

    private sealed record InsightMetric(string Label, string Value, string Icon, bool Featured = false, bool Compact = false);

    private sealed record MoodChartPoint(DateOnly Date, double X, int Y)
    {
        public string Key => $"point:{Date:yyyy-MM-dd}";
    }

    private sealed record MoodChartSegment(
        MoodChartPoint Start,
        MoodChartPoint End)
    {
        public string Key => $"segment:{End.Date:yyyy-MM-dd}";
    }
}

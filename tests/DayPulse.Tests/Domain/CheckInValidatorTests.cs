using DayPulse.Core.Domain;

namespace DayPulse.Tests.Domain;

public sealed class CheckInValidatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 12);

    [Theory]
    [InlineData(-5)]
    [InlineData(5)]
    public void Validate_MoodBoundaryValues_AcceptsCheckIn(int mood)
    {
        var checkIn = CreateCheckIn(mood: mood);

        var result = CheckInValidator.Validate(checkIn, Today);

        Assert.DoesNotContain(result, error => error.Code == CheckInValidationErrorCode.MoodOutOfRange);
    }

    [Theory]
    [InlineData(-6)]
    [InlineData(6)]
    public void Validate_MoodOutsideRange_ReturnsMoodError(int mood)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(mood: mood), Today);

        var error = Assert.Single(
            result,
            validationError => validationError.Code == CheckInValidationErrorCode.MoodOutOfRange);

        Assert.Equal(nameof(CheckIn.Mood), error.PropertyName);
        Assert.Contains("-5 and 5", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_OptionalMetricsOmitted_AcceptsCheckIn()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(), Today);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    public void Validate_OptionalMetricBoundaryValues_AcceptsCheckIn(int minimum, int maximum)
    {
        var checkIn = CreateCheckIn(
            energy: minimum,
            anxiety: maximum,
            focus: minimum);

        var result = CheckInValidator.Validate(checkIn, Today);

        Assert.DoesNotContain(
            result,
            error => error.Code is CheckInValidationErrorCode.EnergyOutOfRange
                or CheckInValidationErrorCode.AnxietyOutOfRange
                or CheckInValidationErrorCode.FocusOutOfRange);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_EnergyOutsideRange_ReturnsEnergyError(int energy)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(energy: energy), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.EnergyOutOfRange
                && error.PropertyName == nameof(CheckIn.Energy));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_AnxietyOutsideRange_ReturnsAnxietyError(int anxiety)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(anxiety: anxiety), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.AnxietyOutOfRange
                && error.PropertyName == nameof(CheckIn.Anxiety));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_FocusOutsideRange_ReturnsFocusError(int focus)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(focus: focus), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.FocusOutOfRange
                && error.PropertyName == nameof(CheckIn.Focus));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(7.5)]
    public void Validate_SleepBoundaryValues_AcceptsCheckIn(double sleepHours)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(sleepHours: sleepHours), Today);

        Assert.DoesNotContain(
            result,
            error => error.Code is CheckInValidationErrorCode.SleepHoursOutOfRange
                or CheckInValidationErrorCode.SleepHoursNotInHalfHourIncrements);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(15.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Validate_SleepOutsideRange_ReturnsSleepRangeError(double sleepHours)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(sleepHours: sleepHours), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.SleepHoursOutOfRange
                && error.PropertyName == nameof(CheckIn.SleepHours));
    }

    [Theory]
    [InlineData(7.25)]
    [InlineData(14.75)]
    [InlineData(7.5000000001)]
    [InlineData(7.4999999999)]
    public void Validate_SleepNotInHalfHourIncrements_ReturnsStepError(double sleepHours)
    {
        var result = CheckInValidator.Validate(CreateCheckIn(sleepHours: sleepHours), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.SleepHoursNotInHalfHourIncrements
                && error.PropertyName == nameof(CheckIn.SleepHours));
    }

    [Fact]
    public void Validate_NotesAtMaximumLength_AcceptsCheckIn()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(notes: new string('x', 500)), Today);

        Assert.DoesNotContain(result, error => error.Code == CheckInValidationErrorCode.NotesTooLong);
    }

    [Fact]
    public void Validate_NotesOverMaximumLength_ReturnsNotesError()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(notes: new string('x', 501)), Today);

        var error = Assert.Single(
            result,
            validationError => validationError.Code == CheckInValidationErrorCode.NotesTooLong);

        Assert.Equal(nameof(CheckIn.Notes), error.PropertyName);
        Assert.Contains("500", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_EntryDateToday_AcceptsCheckIn()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(entryDate: Today), Today);

        Assert.DoesNotContain(
            result,
            error => error.Code is CheckInValidationErrorCode.EntryDateInvalid
                or CheckInValidationErrorCode.EntryDateInFuture);
    }

    [Fact]
    public void Validate_FutureEntryDate_ReturnsFutureDateError()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(entryDate: Today.AddDays(1)), Today);

        var error = Assert.Single(
            result,
            validationError => validationError.Code == CheckInValidationErrorCode.EntryDateInFuture);

        Assert.Equal(nameof(CheckIn.EntryDate), error.PropertyName);
        Assert.Contains("future", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_MinimumDateSentinel_ReturnsInvalidDateError()
    {
        var result = CheckInValidator.Validate(CreateCheckIn(entryDate: DateOnly.MinValue), Today);

        Assert.Contains(
            result,
            error => error.Code == CheckInValidationErrorCode.EntryDateInvalid
                && error.PropertyName == nameof(CheckIn.EntryDate));
    }

    [Fact]
    public void Validate_ExistingCheckInWithSameDate_ReturnsDuplicateDateError()
    {
        var existing = CreateCheckIn(id: "existing");
        var candidate = CreateCheckIn(id: "candidate");

        var result = CheckInValidator.Validate(candidate, Today, [existing]);

        var error = Assert.Single(
            result,
            validationError => validationError.Code == CheckInValidationErrorCode.DuplicateEntryDate);

        Assert.Equal(nameof(CheckIn.EntryDate), error.PropertyName);
    }

    [Fact]
    public void Validate_ExistingCheckInWithSameId_AllowsUpdatingTheEntry()
    {
        var existing = CreateCheckIn(id: "same-id");
        var candidate = CreateCheckIn(id: "same-id", mood: 4);

        var result = CheckInValidator.Validate(candidate, Today, [existing]);

        Assert.DoesNotContain(
            result,
            error => error.Code == CheckInValidationErrorCode.DuplicateEntryDate);
    }

    [Fact]
    public void DuplicateEntryDateValidator_DateSequence_ReturnsDuplicateError()
    {
        var result = DuplicateEntryDateValidator.Validate(
            Today,
            [Today.AddDays(-1), Today]);

        Assert.NotNull(result);
        Assert.Equal(CheckInValidationErrorCode.DuplicateEntryDate, result.Code);
        Assert.Contains("one check-in", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateOrThrow_InvalidCheckIn_ThrowsWithAllErrors()
    {
        var checkIn = CreateCheckIn(
            mood: 6,
            energy: 11,
            anxiety: -1,
            focus: 12,
            sleepHours: 7.25,
            notes: new string('x', 501),
            entryDate: Today.AddDays(1));

        var exception = Assert.Throws<CheckInValidationException>(
            () => CheckInValidator.ValidateOrThrow(checkIn, Today));

        Assert.Contains(
            exception.Errors,
            error => error.Code == CheckInValidationErrorCode.MoodOutOfRange);
        Assert.Contains(
            exception.Errors,
            error => error.Code == CheckInValidationErrorCode.SleepHoursNotInHalfHourIncrements);
        Assert.Contains(
            exception.Errors,
            error => error.Code == CheckInValidationErrorCode.EntryDateInFuture);
    }

    private static CheckIn CreateCheckIn(
        string id = "check-in",
        DateOnly? entryDate = null,
        int mood = 0,
        int? energy = null,
        int? anxiety = null,
        int? focus = null,
        double? sleepHours = null,
        string? notes = null)
    {
        var timestamp = new DateTimeOffset(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

        return new CheckIn(
            id,
            entryDate ?? Today,
            mood,
            energy,
            anxiety,
            focus,
            sleepHours,
            activityIds: [],
            notes,
            timestamp,
            timestamp);
    }
}

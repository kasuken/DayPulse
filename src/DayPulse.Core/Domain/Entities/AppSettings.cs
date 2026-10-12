namespace DayPulse.Core.Domain;

/// <summary>
/// Non-sensitive application preferences.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Creates application settings with the supplied preferences.
    /// </summary>
    /// <param name="onboardingCompleted">Whether the onboarding flow has been completed.</param>
    /// <param name="themeMode">The preferred visual theme.</param>
    /// <param name="reminderEnabled">Whether daily reminders are enabled.</param>
    /// <param name="reminderTime">The local time at which a reminder should be shown.</param>
    /// <param name="energyEnabled">Whether the energy metric is enabled.</param>
    /// <param name="anxietyEnabled">Whether the anxiety metric is enabled.</param>
    /// <param name="focusEnabled">Whether the focus metric is enabled.</param>
    /// <param name="sleepEnabled">Whether the sleep metric is enabled.</param>
    public AppSettings(
        bool onboardingCompleted = false,
        ThemeMode themeMode = ThemeMode.System,
        bool reminderEnabled = false,
        TimeOnly? reminderTime = null,
        bool energyEnabled = true,
        bool anxietyEnabled = true,
        bool focusEnabled = true,
        bool sleepEnabled = true)
    {
        if (!Enum.IsDefined(themeMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(themeMode),
                themeMode,
                "Theme mode must be System, Light, or Dark.");
        }

        OnboardingCompleted = onboardingCompleted;
        ThemeMode = themeMode;
        ReminderEnabled = reminderEnabled;
        ReminderTime = reminderTime ?? new TimeOnly(20, 0);
        EnergyEnabled = energyEnabled;
        AnxietyEnabled = anxietyEnabled;
        FocusEnabled = focusEnabled;
        SleepEnabled = sleepEnabled;
    }

    /// <summary>
    /// Gets a value indicating whether onboarding has been completed.
    /// </summary>
    public bool OnboardingCompleted { get; }

    /// <summary>
    /// Gets the preferred visual theme.
    /// </summary>
    public ThemeMode ThemeMode { get; }

    /// <summary>
    /// Gets a value indicating whether daily reminders are enabled.
    /// </summary>
    public bool ReminderEnabled { get; }

    /// <summary>
    /// Gets the local time at which a reminder should be shown.
    /// </summary>
    public TimeOnly ReminderTime { get; }

    /// <summary>
    /// Gets a value indicating whether the energy metric is enabled.
    /// </summary>
    public bool EnergyEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the anxiety metric is enabled.
    /// </summary>
    public bool AnxietyEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the focus metric is enabled.
    /// </summary>
    public bool FocusEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether the sleep metric is enabled.
    /// </summary>
    public bool SleepEnabled { get; }
}

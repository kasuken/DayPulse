using Bunit;
using DayPulse.App.Pages;
using DayPulse.App.Shared;
using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;
using DayPulse.Infrastructure;
using DayPulse.Infrastructure.Storage;
using DayPulse.Tests.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace DayPulse.Tests;

public sealed class JournalFlowTests : BunitContext
{
    private readonly JournalTestStorage _storage = new();

    public JournalFlowTests()
    {
        Services.AddMudServices();
        Services.AddInfrastructure();
        Services.AddSingleton<IAtomicClientStorage>(_storage);
        Services.AddDayPulseLocalization();
        Services.AddSingleton<TimeProvider>(new FixedJournalTimeProvider());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task Quick_save_precedes_optional_fields_and_saves_only_an_explicit_mood()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 2);

        Assert.True(cut.Markup.IndexOf("data-testid=\"entry-quick-save\"", StringComparison.Ordinal)
            < cut.Markup.IndexOf("data-testid=\"entry-energy\"", StringComparison.Ordinal));
        await cut.Find("[data-testid='entry-quick-save']").ClickAsync(new());

        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        var entry = Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(2, entry.Mood);
        Assert.Null(entry.Energy);
        Assert.Null(entry.SleepHours);
        Assert.Null(entry.Notes);
        Assert.Empty(entry.ActivityIds);
    }

    [Fact]
    public async Task Quick_save_without_mood_focuses_the_error_summary()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var save = await cut.WaitForElementAsync("[data-testid='entry-quick-save']");

        await save.ClickAsync(new());

        cut.WaitForAssertion(() => Assert.Contains("Choose your mood", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Equal("-1", cut.Find("[data-testid='entry-error-summary']").GetAttribute("tabindex"));
        Assert.Contains(JSInterop.Invocations, invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task History_search_filters_notes_and_dates_and_clears_without_changing_entries()
    {
        var today = FixedJournalTimeProvider.Today;
        await Entries.CreateAsync(new CheckIn("walk", today, 2, notes: "A quiet walk"), Xunit.TestContext.Current.CancellationToken);
        await Entries.CreateAsync(new CheckIn("older", today.AddDays(-1), 0), Xunit.TestContext.Current.CancellationToken);
        var cut = Render<History>();
        var search = await cut.WaitForElementAsync("input[data-testid='history-search']");

        await search.InputAsync(new ChangeEventArgs { Value = "  QUIET  " });
        Assert.Single(cut.FindAll("[data-testid='journal-history-entry']"));
        Assert.Equal("/history/walk", cut.Find("[data-testid='journal-history-entry'] a").GetAttribute("href"));
        Assert.Equal("Matching check-ins: 1", cut.Find("[data-testid='history-search-count']").TextContent);

        await search.InputAsync(new ChangeEventArgs { Value = Services.GetRequiredService<LocalizedFormatService>().FormatDate(today.AddDays(-1), "D") });
        Assert.Single(cut.FindAll("[data-testid='journal-history-entry']"));
        Assert.Equal("/history/older", cut.Find("[data-testid='journal-history-entry'] a").GetAttribute("href"));

        await search.InputAsync(new ChangeEventArgs { Value = "not in this journal" });
        Assert.Empty(cut.FindAll("[data-testid='journal-history-entry']"));
        Assert.Contains("No matching check-ins", cut.Markup);
        await cut.FindAll("button").Single(button => button.TextContent.Contains("Clear search", StringComparison.Ordinal)).ClickAsync(new());
        Assert.Equal(2, cut.FindAll("[data-testid='journal-history-entry']").Count);
        Assert.Empty(cut.FindAll("[data-testid='history-search-count']"));
        Assert.Equal(2, (await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken)).Count);
    }

    [Fact]
    public async Task New_check_in_requires_an_explicit_mood_selection()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();

        var save = await cut.WaitForElementAsync("[data-testid='entry-save']");
        await save.ClickAsync(new());

        cut.WaitForAssertion(() => Assert.Contains("Choose your mood", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal("false", cut.Find("[data-testid='entry-mood-status']").GetAttribute("data-selected"));
        Assert.Equal("Not chosen yet", Range(cut, "entry-mood").GetAttribute("aria-valuetext"));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(-4)]
    [InlineData(-3)]
    [InlineData(-2)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Every_mood_choice_saves_its_exact_value(int chosenMood)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();

        await ChooseMoodAsync(cut, chosenMood);
        await cut.Find("[data-testid='entry-save']").ClickAsync(new());

        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        Assert.Equal(chosenMood, Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken)).Mood);
    }

    [Fact]
    public async Task New_check_in_saves_zero_mood_and_optional_values_for_the_local_date()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 0);
        await cut.Find("[data-testid='entry-sleep-input']").ChangeAsync(new ChangeEventArgs { Value = "7.5" });
        await SetNotesAsync(cut, "A quiet morning");
        var work = cut.FindComponents<MudCheckBox<bool>>().Single(check => check.Instance.Label == "Work");
        await cut.InvokeAsync(() => work.Instance.ValueChanged.InvokeAsync(true));

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        var entry = Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(FixedJournalTimeProvider.Today, entry.EntryDate);
        Assert.Equal(0, entry.Mood);
        Assert.Null(entry.Energy);
        Assert.Equal(7.5, entry.SleepHours);
        Assert.Equal("A quiet morning", entry.Notes);
        Assert.Contains("default-work", entry.ActivityIds);
    }

    [Fact]
    public async Task Duplicate_date_retains_mood_and_notes_and_does_not_replace_the_existing_entry()
    {
        await Entries.CreateAsync(new CheckIn("existing", FixedJournalTimeProvider.Today, -2), Xunit.TestContext.Current.CancellationToken);
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 3);
        await SetNotesAsync(cut, "Keep this draft");

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("already have a check-in", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Equal("3", Range(cut, "entry-mood").GetAttribute("value"));
        Assert.Contains("Very good", cut.Find("[data-testid='entry-mood-status']").TextContent);
        Assert.Equal("Keep this draft", ((AngleSharp.Html.Dom.IHtmlTextAreaElement)Notes(cut).Find("textarea")).Value);
        Assert.Equal(-2, Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken)).Mood);
    }

    [Fact]
    public async Task Editing_preserves_hidden_metrics_inactive_activities_and_creation_time()
    {
        await Services.GetRequiredService<ISettingsRepository>().SaveAsync(new AppSettings(
            energyEnabled: false, anxietyEnabled: false, focusEnabled: false, sleepEnabled: false), Xunit.TestContext.Current.CancellationToken);
        var activities = Services.GetRequiredService<IActivityRepository>();
        await activities.SeedDefaultsAsync(Xunit.TestContext.Current.CancellationToken);
        await activities.DeactivateAsync("default-work", Xunit.TestContext.Current.CancellationToken);
        var created = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.Zero);
        await Entries.CreateAsync(new CheckIn("edit-me", FixedJournalTimeProvider.Today, 1,
            energy: 2, anxiety: 3, focus: 4, sleepHours: 6.5,
            activityIds: ["default-work", "removed-custom"], notes: "Before", createdAt: created), Xunit.TestContext.Current.CancellationToken);
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>(parameters => parameters.Add(page => page.Id, "edit-me"));
        await ChooseMoodAsync(cut, -1);
        await SetNotesAsync(cut, "After");

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.EndsWith("/history/edit-me", Services.GetRequiredService<NavigationManager>().Uri));
        var entry = Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(created, entry.CreatedAt);
        Assert.Equal(-1, entry.Mood);
        Assert.Equal("After", entry.Notes);
        Assert.Equal(2, entry.Energy);
        Assert.Equal(3, entry.Anxiety);
        Assert.Equal(4, entry.Focus);
        Assert.Equal(6.5, entry.SleepHours);
        Assert.Contains("default-work", entry.ActivityIds);
        Assert.Contains("removed-custom", entry.ActivityIds);
        Assert.Empty(cut.FindAll("[data-testid='entry-energy'], [data-testid='entry-anxiety'], [data-testid='entry-focus'], [data-testid='entry-sleep']"));
    }

    [Fact]
    public async Task Future_date_is_rejected_without_discarding_notes()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 2);
        await SetNotesAsync(cut, "Keep this too");
        await cut.InvokeAsync(() => cut.FindComponent<MudDatePicker>().Instance.DateChanged
            .InvokeAsync(FixedJournalTimeProvider.Today.AddDays(1).ToDateTime(TimeOnly.MinValue)));

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("Choose today or an earlier date.", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal("Keep this too", ((AngleSharp.Html.Dom.IHtmlTextAreaElement)Notes(cut).Find("textarea")).Value);
    }

    [Fact]
    public async Task Sleep_must_use_half_hour_steps()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 2);
        await cut.Find("[data-testid='entry-sleep-input']").ChangeAsync(new ChangeEventArgs { Value = "7.25" });

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("half-hour steps", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Custom_activity_is_persisted_and_selected()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);
        var custom = cut.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "New activity name");
        await cut.InvokeAsync(() => custom.Instance.ValueChanged.InvokeAsync("Gardening"));
        cut.Find("button:contains('Add and select activity')").Click();
        cut.WaitForAssertion(() => Assert.Contains(cut.FindComponents<MudCheckBox<bool>>(),
            check => check.Instance.Label == "Gardening"
                && ((AngleSharp.Html.Dom.IHtmlInputElement)check.Find("input")).IsChecked));

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        var activity = Assert.Single(
            await Services.GetRequiredService<IActivityRepository>().GetAllAsync(Xunit.TestContext.Current.CancellationToken),
            item => !item.IsDefault);
        Assert.Equal("Gardening", activity.Name);
        Assert.Contains(activity.Id, Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken)).ActivityIds);
    }

    [Fact]
    public void Empty_history_offers_a_check_in_without_coming_soon_content()
    {
        var cut = Render<History>();

        Assert.Contains("Your story starts here", cut.Markup);
        Assert.Equal("/check-in", cut.Find("a:contains('Check in')").GetAttribute("href"));
        Assert.Empty(cut.FindAll("[data-testid='journal-history-entry']"));
        Assert.DoesNotContain("Coming next", cut.Markup);
    }

    [Fact]
    public async Task History_is_reverse_chronological_and_links_to_details()
    {
        await Entries.CreateAsync(new CheckIn("older", FixedJournalTimeProvider.Today.AddDays(-1), -3), Xunit.TestContext.Current.CancellationToken);
        await Entries.CreateAsync(new CheckIn("newer", FixedJournalTimeProvider.Today, 4), Xunit.TestContext.Current.CancellationToken);

        var cut = Render<History>();

        var rows = cut.FindAll("[data-testid='journal-history-entry']");
        Assert.Equal(2, rows.Count);
        Assert.Equal("/history/newer", rows[0].QuerySelector("a")?.GetAttribute("href"));
        Assert.Equal("/history/older", rows[1].QuerySelector("a")?.GetAttribute("href"));
    }

    [Fact]
    public async Task Detail_shows_all_recorded_values_even_when_a_metric_is_disabled_for_new_entries()
    {
        await Services.GetRequiredService<ISettingsRepository>().SaveAsync(new AppSettings(energyEnabled: false), Xunit.TestContext.Current.CancellationToken);
        await Entries.CreateAsync(new CheckIn("detail", FixedJournalTimeProvider.Today, 3,
            energy: 8, anxiety: 2, focus: 4, sleepHours: 7.5, notes: "First line\nSecond line"), Xunit.TestContext.Current.CancellationToken);

        var cut = Render<EntryDetail>(parameters => parameters.Add(page => page.Id, "detail"));

        Assert.Contains("First line\nSecond line", cut.Find(".journal-notes").TextContent);
        Assert.Equal(5, cut.FindAll(".daypulse-metric-overview__item").Count);
        Assert.Contains("Energy", cut.FindAll(".daypulse-metric-overview__item")[1].TextContent);
        Assert.Contains("8", cut.FindAll(".daypulse-metric-overview__item")[1].TextContent);
        Assert.Contains("7.5", cut.Markup);
        Assert.Equal("/check-in/detail", cut.Find("a:contains('Edit check-in')").GetAttribute("href"));
    }

    [Fact]
    public async Task Delete_requires_confirmation_and_cancel_keeps_the_entry()
    {
        await Entries.CreateAsync(new CheckIn("delete-me", FixedJournalTimeProvider.Today, 1), Xunit.TestContext.Current.CancellationToken);
        var cut = Render<EntryDetail>(parameters => parameters.Add(page => page.Id, "delete-me"));

        cut.Find("[data-testid='entry-delete']").Click();

        Assert.NotNull(await Entries.GetByIdAsync("delete-me", Xunit.TestContext.Current.CancellationToken));
        Assert.Contains("cannot be undone", cut.Find("[role='alertdialog']").TextContent);
        cut.Find("[role='alertdialog'] button:contains('Cancel')").Click();
        Assert.Empty(cut.FindAll("[role='alertdialog']"));
        Assert.NotNull(await Entries.GetByIdAsync("delete-me", Xunit.TestContext.Current.CancellationToken));

        cut.Find("[data-testid='entry-delete']").Click();
        cut.Find("[data-testid='entry-confirm-delete']").Click();

        cut.WaitForAssertion(() => Assert.EndsWith("/history", Services.GetRequiredService<NavigationManager>().Uri));
        Assert.Null(await Entries.GetByIdAsync("delete-me", Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_storage_failure_keeps_confirmation_and_entry_for_retry()
    {
        await Entries.CreateAsync(new CheckIn("keep-me", FixedJournalTimeProvider.Today, 1), Xunit.TestContext.Current.CancellationToken);
        var cut = Render<EntryDetail>(parameters => parameters.Add(page => page.Id, "keep-me"));
        _storage.FailWrites = true;
        cut.Find("[data-testid='entry-delete']").Click();

        cut.Find("[data-testid='entry-confirm-delete']").Click();

        cut.WaitForAssertion(() => Assert.Contains("could not be deleted", cut.Markup));
        Assert.NotNull(await Entries.GetByIdAsync("keep-me", Xunit.TestContext.Current.CancellationToken));
        Assert.NotNull(cut.Find("[data-testid='entry-confirm-delete']"));
    }

    [Fact]
    public async Task Save_storage_failure_retains_the_draft_for_retry()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 4);
        await SetNotesAsync(cut, "Still here");
        _storage.FailWrites = true;

        cut.Find("[data-testid='entry-save']").Click();

        cut.WaitForAssertion(() => Assert.Contains("could not be saved", cut.Find("[data-testid='entry-errors']").TextContent));
        Assert.Equal("Still here", ((AngleSharp.Html.Dom.IHtmlTextAreaElement)Notes(cut).Find("textarea")).Value);
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
        _storage.FailWrites = false;
        cut.Find("[data-testid='entry-save']").Click();
        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        Assert.Equal("Still here", Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken)).Notes);
    }

    [Fact]
    public void History_load_failure_offers_a_working_retry()
    {
        _storage.FailReads = true;
        var cut = Render<History>();
        Assert.Contains("could not be loaded", cut.Markup);

        _storage.FailReads = false;
        cut.Find("button:contains('Try again')").Click();

        cut.WaitForAssertion(() => Assert.Contains("Your story starts here", cut.Markup));
    }

    [Fact]
    public void History_announces_loading_before_showing_empty_content()
    {
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _storage.PendingReads = pending.Task;

        var cut = Render<History>();

        Assert.Contains("Loading your journal", cut.Find("[role='status']").TextContent);
        Assert.DoesNotContain("Your story starts here", cut.Markup);
        pending.SetResult();
        cut.WaitForAssertion(() => Assert.Contains("Your story starts here", cut.Markup));
    }

    [Fact]
    public void Editor_load_failure_can_be_retried_without_offering_an_uninitialized_form()
    {
        Render<MudPopoverProvider>();
        _storage.FailReads = true;
        var cut = Render<CheckInEditor>();
        Assert.Empty(cut.FindAll("[data-testid='entry-save']"));
        Assert.Contains("could not be loaded", cut.Markup);

        _storage.FailReads = false;
        cut.Find("button:contains('Try again')").Click();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find("[data-testid='entry-save']")));
    }

    [Fact]
    public void Missing_detail_and_editor_do_not_offer_mutations()
    {
        var detail = Render<EntryDetail>(parameters => parameters.Add(page => page.Id, "missing"));
        var editor = Render<CheckInEditor>(parameters => parameters.Add(page => page.Id, "missing"));

        Assert.Contains("no longer exists", detail.Markup);
        Assert.Contains("no longer exists", editor.Markup);
        Assert.Empty(detail.FindAll("[data-testid='entry-delete']"));
        Assert.Empty(editor.FindAll("[data-testid='entry-save']"));
    }

    [Fact]
    public async Task Mood_scale_uses_one_native_slider_and_five_named_face_shortcuts()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();

        var fieldset = await cut.WaitForElementAsync("fieldset[data-testid='entry-mood']");
        Assert.Contains("Choose your mood", fieldset.QuerySelector("legend")!.TextContent);
        var input = Assert.Single(fieldset.QuerySelectorAll("input"));
        Assert.Equal("range", input.GetAttribute("type"));
        Assert.Equal("-5", input.GetAttribute("min"));
        Assert.Equal("5", input.GetAttribute("max"));
        Assert.Equal("1", input.GetAttribute("step"));
        Assert.Equal("0", input.GetAttribute("value"));
        Assert.Equal("entry-mood-label", input.GetAttribute("aria-labelledby"));
        Assert.Equal("entry-mood-help", input.GetAttribute("aria-describedby"));
        Assert.Equal("Not chosen yet", input.GetAttribute("aria-valuetext"));
        var anchors = fieldset.QuerySelectorAll("button");
        Assert.Equal(new[] { "Awful, −5", "Low, −3", "Neutral, 0", "Very good, +3", "Excellent, +5" },
            anchors.Select(anchor => anchor.GetAttribute("aria-label")));
        Assert.All(anchors, anchor => Assert.Equal("false", anchor.GetAttribute("aria-pressed")));
        Assert.Equal("true", cut.Find("[data-testid='entry-mood-face']").GetAttribute("aria-hidden"));
        Assert.Equal("false", cut.Find("[data-testid='entry-mood-status']").GetAttribute("data-selected"));
    }

    [Fact]
    public async Task Choosing_a_mood_announces_its_descriptor_and_score()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();

        await ChooseMoodAsync(cut, 3);

        var status = cut.Find("[data-testid='entry-mood-status']");
        Assert.Equal("true", status.GetAttribute("data-selected"));
        Assert.Contains("Very good", status.TextContent);
        Assert.Contains("+3", status.TextContent);
        Assert.Equal("3", Range(cut, "entry-mood").GetAttribute("value"));
        Assert.Equal("Very good, +3", Range(cut, "entry-mood").GetAttribute("aria-valuetext"));
        Assert.Single(cut.FindAll("[data-testid='entry-mood'] button[aria-pressed='true']"));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(-3)]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Face_shortcuts_explicitly_choose_and_save_their_exact_score(int score)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var anchor = await cut.WaitForElementAsync($"[data-testid='entry-mood-anchor-{score}']");

        await anchor.ClickAsync(new());

        Assert.Equal("true", anchor.GetAttribute("aria-pressed"));
        Assert.Single(cut.FindAll("[data-testid='entry-mood'] button[aria-pressed='true']"));
        Assert.Equal(score.ToString(System.Globalization.CultureInfo.InvariantCulture), Range(cut, "entry-mood").GetAttribute("value"));
        Assert.Equal(score, (await SaveAndGetSingleAsync(cut)).Mood);
    }

    [Fact]
    public async Task Tapping_the_resting_mood_thumb_explicitly_records_neutral()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var input = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");
        Assert.Equal("Not chosen yet", input.GetAttribute("aria-valuetext"));

        await input.ClickAsync(new());

        Assert.Equal("Neutral, 0", input.GetAttribute("aria-valuetext"));
        Assert.Equal("true", cut.Find("[data-testid='entry-mood-anchor-0']").GetAttribute("aria-pressed"));
        Assert.Equal(0, (await SaveAndGetSingleAsync(cut)).Mood);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public async Task Confirming_the_resting_mood_thumb_with_the_keyboard_records_neutral(string key)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var input = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");

        await input.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });

        Assert.Equal("Neutral, 0", input.GetAttribute("aria-valuetext"));
        Assert.Equal(0, (await SaveAndGetSingleAsync(cut)).Mood);
    }

    [Fact]
    public async Task Focus_tab_and_heading_click_leave_mood_unchosen_and_required()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var input = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");

        // There is deliberately no focus handler. bUnit cannot execute native DOM focus;
        // it reports the absent handler instead. Browser focus must leave the null state alone.
        await Assert.ThrowsAsync<MissingEventHandlerException>(() => input.FocusAsync(new()));
        await input.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Tab" });
        await Assert.ThrowsAsync<MissingEventHandlerException>(() => cut.Find("#entry-mood-label").ClickAsync(new()));

        Assert.Equal("Not chosen yet", input.GetAttribute("aria-valuetext"));
        Assert.Equal("false", cut.Find("[data-testid='entry-mood-status']").GetAttribute("data-selected"));
        Assert.Empty(cut.FindAll("label[for='entry-mood-input']"));
        await cut.Find("[data-testid='entry-save']").ClickAsync(new());
        Assert.Contains("Choose your mood", cut.Find("[data-testid='entry-errors']").TextContent);
        Assert.Empty(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("ArrowLeft", -1)]
    [InlineData("ArrowRight", 1)]
    [InlineData("Home", -5)]
    [InlineData("End", 5)]
    public async Task Native_keyboard_input_records_the_emitted_mood_score(string key, int score)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var input = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");

        await input.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });
        // bUnit does not execute the browser's default range-key action; emit its native input event.
        await input.InputAsync(new ChangeEventArgs { Value = score.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        await input.ChangeAsync(new ChangeEventArgs { Value = score.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        await input.ClickAsync(new());

        Assert.Equal(score, (await SaveAndGetSingleAsync(cut)).Mood);
    }

    [Fact]
    public async Task Large_face_varies_both_eyes_and_mouth_for_every_exact_score()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        var eyes = new HashSet<string?>(StringComparer.Ordinal);
        var mouths = new HashSet<string?>(StringComparer.Ordinal);
        foreach (var score in Enumerable.Range(-5, 11))
        {
            await ChooseMoodAsync(cut, score);
            var features = cut.FindAll("[data-testid='entry-mood-face'] path");
            Assert.Equal(2, features.Count);
            eyes.Add(features[0].GetAttribute("d"));
            mouths.Add(features[1].GetAttribute("d"));
        }

        Assert.Equal(11, eyes.Count);
        Assert.Equal(11, mouths.Count);
    }

    [Fact]
    public async Task Disabled_mood_picker_ignores_all_selection_handlers()
    {
        var selections = new List<int?>();
        var cut = Render<MoodPicker>(parameters => parameters
            .Add(picker => picker.Disabled, true)
            .Add(picker => picker.ValueChanged, (int? value) => selections.Add(value)));
        var input = cut.Find("[data-testid='entry-mood-input']");
        Assert.True(input.HasAttribute("disabled"));
        Assert.All(cut.FindAll("button"), button => Assert.True(button.HasAttribute("disabled")));

        await input.InputAsync(new ChangeEventArgs { Value = "4" });
        await input.ChangeAsync(new ChangeEventArgs { Value = "-2" });
        await input.ClickAsync(new());
        await input.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        await cut.Find("[data-testid='entry-mood-anchor-0']").ClickAsync(new());

        Assert.Empty(selections);
        Assert.Equal("Not chosen yet", input.GetAttribute("aria-valuetext"));
    }

    [Fact]
    public async Task Untouched_optional_metrics_are_saved_as_not_recorded()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        Assert.All(cut.FindAll("[data-testid='entry-energy'], [data-testid='entry-anxiety'], [data-testid='entry-focus'], [data-testid='entry-sleep']"),
            metric => Assert.Equal("false", metric.GetAttribute("data-recorded")));
        Assert.Contains("Not recorded", cut.Find("[data-testid='entry-energy-value']").TextContent);
        await ChooseMoodAsync(cut, 1);

        var entry = await SaveAndGetSingleAsync(cut);

        Assert.Null(entry.Energy);
        Assert.Null(entry.Anxiety);
        Assert.Null(entry.Focus);
        Assert.Null(entry.SleepHours);
    }

    [Fact]
    public async Task Moving_scales_records_exact_scores_including_zero()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await Range(cut, "entry-energy").InputAsync(new ChangeEventArgs { Value = "8" });
        await Range(cut, "entry-anxiety").ChangeAsync(new ChangeEventArgs { Value = "0" });
        await Range(cut, "entry-focus").ChangeAsync(new ChangeEventArgs { Value = "10" });

        Assert.Equal("true", cut.Find("[data-testid='entry-anxiety']").GetAttribute("data-recorded"));
        Assert.Contains("8", cut.Find("[data-testid='entry-energy-value']").TextContent);
        Assert.Equal("8 out of 10", Range(cut, "entry-energy").GetAttribute("aria-valuetext"));
        var entry = await SaveAndGetSingleAsync(cut);
        Assert.Equal(8, entry.Energy);
        Assert.Equal(0, entry.Anxiety);
        Assert.Equal(10, entry.Focus);
    }

    [Fact]
    public async Task Clearing_a_scale_returns_it_to_not_recorded()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);
        await Range(cut, "entry-energy").ChangeAsync(new ChangeEventArgs { Value = "8" });

        await cut.Find("[data-testid='entry-energy-clear']").ClickAsync(new());

        Assert.Equal("false", cut.Find("[data-testid='entry-energy']").GetAttribute("data-recorded"));
        Assert.Empty(cut.FindAll("[data-testid='entry-energy-clear']"));
        Assert.Null((await SaveAndGetSingleAsync(cut)).Energy);
    }

    [Fact]
    public async Task Tapping_or_confirming_an_unrecorded_scale_records_the_shown_value()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await Range(cut, "entry-energy").ClickAsync(new());
        await Range(cut, "entry-focus").KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        await Range(cut, "entry-sleep").ClickAsync(new());

        var entry = await SaveAndGetSingleAsync(cut);
        Assert.Equal(5, entry.Energy);
        Assert.Null(entry.Anxiety);
        Assert.Equal(5, entry.Focus);
        Assert.Equal(7, entry.SleepHours);
    }

    [Fact]
    public async Task Editing_shows_recorded_scores_on_the_scales()
    {
        await Entries.CreateAsync(new CheckIn("prefilled", FixedJournalTimeProvider.Today, -4,
            energy: 0, anxiety: 7, focus: 3, sleepHours: 8.5), Xunit.TestContext.Current.CancellationToken);
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>(parameters => parameters.Add(page => page.Id, "prefilled"));

        var mood = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");
        Assert.Equal("-4", mood.GetAttribute("value"));
        Assert.Equal("Very low, −4", mood.GetAttribute("aria-valuetext"));
        Assert.Equal("true", cut.Find("[data-testid='entry-mood-status']").GetAttribute("data-selected"));
        Assert.Empty(cut.FindAll("[data-testid='entry-mood'] button[aria-pressed='true']"));
        Assert.Equal("0", Range(cut, "entry-energy").GetAttribute("value"));
        Assert.Equal("true", cut.Find("[data-testid='entry-energy']").GetAttribute("data-recorded"));
        Assert.Equal("7", Range(cut, "entry-anxiety").GetAttribute("value"));
        Assert.Equal("3", Range(cut, "entry-focus").GetAttribute("value"));
        Assert.Equal("8.5", Range(cut, "entry-sleep").GetAttribute("value"));
        Assert.Contains("8.5", cut.Find("[data-testid='entry-sleep-value']").TextContent);

        var entry = await SaveAndGetSingleAsync(cut);
        Assert.Equal(-4, entry.Mood);
        Assert.Equal(0, entry.Energy);
        Assert.Equal(7, entry.Anxiety);
        Assert.Equal(3, entry.Focus);
        Assert.Equal(8.5, entry.SleepHours);
    }

    [Theory]
    [InlineData("entry-sleep-increase", 7.5)]
    [InlineData("entry-sleep-decrease", 6.5)]
    public async Task Sleep_steps_by_half_an_hour_from_a_typical_night(string button, double expected)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await cut.Find($"[data-testid='{button}']").ClickAsync(new());

        Assert.Equal(expected, (await SaveAndGetSingleAsync(cut)).SleepHours);
    }

    [Theory]
    [InlineData("0", "entry-sleep-decrease", 0)]
    [InlineData("15", "entry-sleep-increase", 15)]
    public async Task Sleep_stepper_stays_within_its_bounds(string hours, string outwardButton, double expected)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);
        await Range(cut, "entry-sleep").ChangeAsync(new ChangeEventArgs { Value = hours });

        // Stays enabled so keyboard focus is not dropped at the limit; the press simply has no effect.
        var button = cut.Find($"[data-testid='{outwardButton}']");
        Assert.False(button.HasAttribute("disabled"));
        await button.ClickAsync(new());

        Assert.Equal(expected, (await SaveAndGetSingleAsync(cut)).SleepHours);
    }

    [Theory]
    [InlineData(" ", true)]
    [InlineData("Enter", true)]
    [InlineData("Tab", false)]
    [InlineData("Shift", false)]
    [InlineData("ArrowRight", false)]
    public async Task Only_confirming_keys_record_an_unrecorded_scale(string key, bool records)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await Range(cut, "entry-anxiety").KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });
        await Range(cut, "entry-sleep").KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });

        var expected = records ? "true" : "false";
        Assert.Equal(expected, cut.Find("[data-testid='entry-anxiety']").GetAttribute("data-recorded"));
        Assert.Equal(expected, cut.Find("[data-testid='entry-sleep']").GetAttribute("data-recorded"));
        var entry = await SaveAndGetSingleAsync(cut);
        Assert.Equal(records ? 5 : null, entry.Anxiety);
        Assert.Equal(records ? 7 : null, entry.SleepHours);
    }

    [Theory]
    [InlineData("entry-energy")]
    [InlineData("entry-anxiety")]
    [InlineData("entry-focus")]
    [InlineData("entry-sleep")]
    public void Scale_names_are_not_clickable_labels_that_would_record_a_value(string id)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();

        var input = cut.WaitForElement($"[data-testid='{id}-input']");
        var name = cut.Find($"#{input.GetAttribute("aria-labelledby")}");

        Assert.NotEqual("LABEL", name.TagName);
        Assert.False(string.IsNullOrWhiteSpace(name.TextContent));
        Assert.Empty(cut.FindAll($"label[for='{input.Id}']"));
    }

    [Fact]
    public async Task Clearing_a_recorded_zero_while_editing_saves_not_recorded()
    {
        await Entries.CreateAsync(new CheckIn("zero", FixedJournalTimeProvider.Today, 2,
            energy: 0, sleepHours: 0), Xunit.TestContext.Current.CancellationToken);
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>(parameters => parameters.Add(page => page.Id, "zero"));
        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find("[data-testid='entry-energy']").GetAttribute("data-recorded")));

        await cut.Find("[data-testid='entry-energy-clear']").ClickAsync(new());
        await cut.Find("[data-testid='entry-sleep-clear']").ClickAsync(new());

        var entry = await SaveAndGetSingleAsync(cut);
        Assert.Null(entry.Energy);
        Assert.Null(entry.SleepHours);
    }

    [Theory]
    [InlineData("entry-energy")]
    [InlineData("entry-sleep")]
    public async Task Clearing_returns_focus_to_the_slider(string id)
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await Range(cut, id).ChangeAsync(new ChangeEventArgs { Value = "3" });
        var focusCallsBefore = FocusCalls();

        await cut.Find($"[data-testid='{id}-clear']").ClickAsync(new());

        Assert.Equal(focusCallsBefore + 1, FocusCalls());
    }

    private int FocusCalls() =>
        JSInterop.Invocations.Count(invocation => invocation.Identifier.EndsWith("focus", StringComparison.OrdinalIgnoreCase));

    [Fact]
    public async Task Sleep_preset_records_its_hours_and_can_be_cleared()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await cut.Find("[data-testid='entry-sleep-preset-8']").ClickAsync(new());
        Assert.Equal("true", cut.Find("[data-testid='entry-sleep-preset-8']").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("[data-testid='entry-sleep-preset-7']").GetAttribute("aria-pressed"));
        await cut.Find("[data-testid='entry-sleep-clear']").ClickAsync(new());

        Assert.Equal("false", cut.Find("[data-testid='entry-sleep']").GetAttribute("data-recorded"));
        Assert.Null((await SaveAndGetSingleAsync(cut)).SleepHours);
    }

    [Fact]
    public async Task Sleep_preset_value_is_saved()
    {
        Render<MudPopoverProvider>();
        var cut = Render<CheckInEditor>();
        await ChooseMoodAsync(cut, 1);

        await cut.Find("[data-testid='entry-sleep-preset-8']").ClickAsync(new());

        Assert.Equal(8, (await SaveAndGetSingleAsync(cut)).SleepHours);
    }

    private ICheckInRepository Entries => Services.GetRequiredService<ICheckInRepository>();

    private static async Task ChooseMoodAsync(IRenderedComponent<CheckInEditor> cut, int mood)
    {
        var input = await cut.WaitForElementAsync("[data-testid='entry-mood-input']");
        await input.InputAsync(new ChangeEventArgs { Value = mood.ToString(System.Globalization.CultureInfo.InvariantCulture) });
    }

    private static AngleSharp.Dom.IElement Range(IRenderedComponent<CheckInEditor> cut, string id) =>
        cut.WaitForElement($"[data-testid='{id}-input']");

    private async Task<CheckIn> SaveAndGetSingleAsync(IRenderedComponent<CheckInEditor> cut)
    {
        await cut.Find("[data-testid='entry-save']").ClickAsync(new());
        cut.WaitForAssertion(() => Assert.Contains("/history/", Services.GetRequiredService<NavigationManager>().Uri));
        return Assert.Single(await Entries.GetAllAsync(Xunit.TestContext.Current.CancellationToken));
    }

    private static Task SetNotesAsync(IRenderedComponent<CheckInEditor> cut, string notes) =>
        cut.InvokeAsync(() => Notes(cut).Instance.ValueChanged.InvokeAsync(notes));

    private static IRenderedComponent<MudTextField<string>> Notes(IRenderedComponent<CheckInEditor> cut) =>
        cut.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Notes");

    private sealed class JournalTestStorage : IAtomicClientStorage
    {
        private readonly InMemoryClientStorage _inner = new();
        public bool FailReads { get; set; }
        public bool FailWrites { get; set; }
        public Task? PendingReads { get; set; }

        public async ValueTask<string?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (FailReads) throw new IOException("Storage unavailable.");
            if (PendingReads is { } pending) await pending.WaitAsync(cancellationToken);
            return await _inner.GetAsync(key, cancellationToken);
        }

        public ValueTask SetAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            if (FailWrites) throw new IOException("Storage unavailable.");
            return _inner.SetAsync(key, value, cancellationToken);
        }

        public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (FailWrites) throw new IOException("Storage unavailable.");
            return _inner.RemoveAsync(key, cancellationToken);
        }

        public ValueTask<bool> TrySetAsync(string key, string? expectedValue, string value, CancellationToken cancellationToken = default)
        {
            if (FailWrites) throw new IOException("Storage unavailable.");
            return _inner.TrySetAsync(key, expectedValue, value, cancellationToken);
        }

        public ValueTask<bool> TryRemoveAsync(string key, string? expectedValue, CancellationToken cancellationToken = default)
        {
            if (FailWrites) throw new IOException("Storage unavailable.");
            return _inner.TryRemoveAsync(key, expectedValue, cancellationToken);
        }
    }
}

internal sealed class FixedJournalTimeProvider : TimeProvider
{
    public static DateOnly Today => new(2024, 6, 3);
    public override DateTimeOffset GetUtcNow() => new(2024, 6, 2, 23, 30, 0, TimeSpan.Zero);
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.CreateCustomTimeZone(
        "JournalTestOffset", TimeSpan.FromHours(2), "Journal test offset", "Journal test offset");
}

using DayPulse.Core.Domain;
using DayPulse.Core.Repositories;

namespace DayPulse.App.Shared;

/// <summary>
/// Coordinates the selected theme mode and its resolved light or dark state.
/// </summary>
public sealed class ThemeState : IDisposable
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly SemaphoreSlim _stateGate = new(1, 1);
    private readonly object _providerLifecycleGate = new();
    private AppSettings? _settings;
    private bool _isDarkMode;
    private bool _settingsInitializationFailed;
    private bool _providerInitializationFailed;
    private bool _providerInitialized;
    private bool _providerRegistered;
    private TaskCompletionSource<bool>? _providerRetryCompletion;
    private bool _disposed;

    /// <summary>
    /// Creates the shared theme state.
    /// </summary>
    /// <param name="settingsRepository">The browser-backed settings repository.</param>
    public ThemeState(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository
            ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    /// <summary>
    /// Occurs when the selected theme mode changes.
    /// </summary>
    public event EventHandler? Changed;

    internal event EventHandler? RetryRequested;

    /// <summary>
    /// Gets a value indicating whether persisted settings have been loaded.
    /// </summary>
    public bool IsInitialized => _settings is not null;

    /// <summary>
    /// Gets a value indicating whether settings or the browser theme provider failed
    /// to initialize.
    /// </summary>
    public bool IsInitializationFailed =>
        _settingsInitializationFailed || _providerInitializationFailed;

    /// <summary>
    /// Gets a value indicating whether the browser theme provider has initialized,
    /// or no provider is registered for this state.
    /// </summary>
    public bool IsProviderInitialized
    {
        get
        {
            lock (_providerLifecycleGate)
            {
                return !_providerRegistered || _providerInitialized;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the browser theme provider failed to initialize.
    /// </summary>
    public bool IsProviderInitializationFailed
    {
        get
        {
            lock (_providerLifecycleGate)
            {
                return _providerRegistered && _providerInitializationFailed;
            }
        }
    }

    /// <summary>
    /// Gets the selected theme mode.
    /// </summary>
    public ThemeMode Mode => _settings?.ThemeMode ?? ThemeMode.System;

    /// <summary>
    /// Gets a value indicating whether the active palette is dark.
    /// </summary>
    public bool IsDarkMode => _isDarkMode;

    /// <summary>
    /// Loads the persisted application settings once.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the load.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _stateGate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_settings is not null)
            {
                return;
            }

            try
            {
                var settings = await _settingsRepository.LoadAsync(cancellationToken);
                _settings = settings;
                _isDarkMode = settings.ThemeMode == ThemeMode.Dark;
                _settingsInitializationFailed = false;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                _settingsInitializationFailed = true;
                throw;
            }
        }
        finally
        {
            _stateGate.Release();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Retries theme initialization and notifies the browser theme provider when
    /// persisted settings are already available.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the retry.</param>
    public async Task RetryInitializationAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var wasInitialized = IsInitialized;
        var providerRetryCompletion = PrepareProviderRetry();
        try
        {
            await InitializeAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (wasInitialized && providerRetryCompletion is not null)
            {
                RetryRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (wasInitialized)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }

            if (providerRetryCompletion is not null)
            {
                await providerRetryCompletion.Task.WaitAsync(cancellationToken);
            }
        }
        catch
        {
            CompleteProviderRetry(false);
            throw;
        }
    }

    internal void RegisterProvider()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_providerLifecycleGate)
        {
            _providerRegistered = true;
            _providerInitialized = false;
            _providerInitializationFailed = false;
        }
    }

    internal void UnregisterProvider()
    {
        lock (_providerLifecycleGate)
        {
            _providerRegistered = false;
            _providerInitialized = false;
            _providerInitializationFailed = false;
        }

        CompleteProviderRetry(false);
    }

    /// <summary>
    /// Records that the browser theme provider completed initialization.
    /// </summary>
    internal void MarkProviderInitialized()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_providerLifecycleGate)
        {
            if (_providerInitialized && !_providerInitializationFailed)
            {
                return;
            }

            _providerInitialized = true;
            _providerInitializationFailed = false;
        }

        Changed?.Invoke(this, EventArgs.Empty);
        CompleteProviderRetry(true);
    }

    /// <summary>
    /// Records that the browser theme provider failed to initialize.
    /// </summary>
    internal void MarkProviderInitializationFailed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_providerLifecycleGate)
        {
            if (_providerInitializationFailed && !_providerInitialized)
            {
                return;
            }

            _providerInitialized = false;
            _providerInitializationFailed = true;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal void CompleteProviderRetry(bool initialized)
    {
        TaskCompletionSource<bool>? completion;
        lock (_providerLifecycleGate)
        {
            completion = _providerRetryCompletion;
            _providerRetryCompletion = null;
        }

        completion?.TrySetResult(initialized);
    }

    /// <summary>
    /// Persists a new selected mode while retaining every other application setting.
    /// </summary>
    /// <param name="mode">The mode to persist.</param>
    /// <param name="cancellationToken">
    /// The token used to cancel initialization or the operation before persistence begins.
    /// Once persistence begins, the update is allowed to complete so in-memory state cannot
    /// diverge from a committed browser-storage value.
    /// </param>
    public async Task SetModeAsync(
        ThemeMode mode,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Theme mode must be System, Light, or Dark.");
        }

        await InitializeAsync(cancellationToken);

        await _stateGate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var currentSettings = _settings
                ?? throw new InvalidOperationException(
                    "Theme state was not initialized.");
            if (currentSettings.ThemeMode == mode)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var previousSettings = currentSettings;
            var previousIsDarkMode = _isDarkMode;
            _settings = currentSettings.WithThemeMode(mode);
            if (mode is ThemeMode.Light or ThemeMode.Dark)
            {
                _isDarkMode = mode == ThemeMode.Dark;
            }

            try
            {
                // Notify consumers before persistence completes so the visible theme changes
                // immediately. The state gate remains held to serialize competing changes.
                Changed?.Invoke(this, EventArgs.Empty);
                _settings = await _settingsRepository.UpdateAsync(
                    settings => settings.WithThemeMode(mode),
                    CancellationToken.None);
            }
            catch
            {
                _settings = previousSettings;
                _isDarkMode = previousIsDarkMode;
                Changed?.Invoke(this, EventArgs.Empty);
                throw;
            }
        }
        finally
        {
            _stateGate.Release();
        }
    }

    /// <summary>
    /// Applies a browser system preference while the selected mode is <see cref="ThemeMode.System"/>.
    /// </summary>
    /// <param name="isDarkMode">Whether the browser currently prefers dark mode.</param>
    /// <returns><see langword="true"/> when the resolved mode changed.</returns>
    public bool ApplySystemPreference(bool isDarkMode)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Mode != ThemeMode.System || _isDarkMode == isDarkMode)
        {
            return false;
        }

        _isDarkMode = isDarkMode;
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Changed = null;
        RetryRequested = null;
        CompleteProviderRetry(false);
        _stateGate.Dispose();
    }

    private TaskCompletionSource<bool>? PrepareProviderRetry()
    {
        lock (_providerLifecycleGate)
        {
            if (!_providerRegistered || _providerInitialized)
            {
                return null;
            }

            _providerRetryCompletion ??= new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            return _providerRetryCompletion;
        }
    }
}

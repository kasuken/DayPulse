using Microsoft.JSInterop;

namespace DayPulse.App.Shared;

internal sealed class BrowserLocaleInterop(
    IJSRuntime jsRuntime) : IAsyncDisposable
{
    internal const string ModulePath = "./Shared/BrowserLocaleInitializer.razor.js";
    internal const string GetBrowserLocalesMethod = "getBrowserLocales";
    internal const string SetDocumentLanguageMethod = "setDocumentLanguage";
    internal const string SetBootstrapErrorTextMethod = "setBootstrapErrorText";
    internal const string DisposeMethod = "dispose";

    private readonly IJSRuntime _jsRuntime =
        jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private IJSObjectReference? _module;
    private bool _disposed;

    internal async ValueTask<string[]?> GetBrowserLocalesAsync(
        CancellationToken cancellationToken)
    {
        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<string[]?>(
            GetBrowserLocalesMethod,
            cancellationToken);
    }

    internal async ValueTask SetDocumentLanguageAsync(
        string cultureName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);

        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync(
            SetDocumentLanguageMethod,
            cancellationToken,
            cultureName);
    }

    internal async ValueTask SetBootstrapErrorTextAsync(
        string unhandledError,
        string reload,
        string errorBoundary,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unhandledError);
        ArgumentException.ThrowIfNullOrWhiteSpace(reload);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorBoundary);

        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync(
            SetBootstrapErrorTextMethod,
            cancellationToken,
            unhandledError,
            reload,
            errorBoundary);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var module = _module;
        _module = null;
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync(DisposeMethod);
        }
        catch (JSDisconnectedException)
        {
            return;
        }

        try
        {
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser context has already gone away.
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync(
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            cancellationToken,
            ModulePath);
    }
}

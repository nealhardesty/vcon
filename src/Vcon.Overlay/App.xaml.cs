using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Vcon.Core.Abstractions;
using Vcon.Core.Configuration;
using Vcon.Core.Models;
using Vcon.Input;
using Vcon.Overlay.Services;
using Vcon.Overlay.ViewModels;
using Vcon.Overlay.Windows;

namespace Vcon.Overlay;

/// <summary>
/// WPF application entry point: single-instance enforcement, DI composition, and lifecycle management.
/// </summary>
public partial class App : Application
{
    private const string MutexName = "Global\\VconOverlay";

    private Mutex? _mutex;
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!AcquireSingleInstance())
        {
            Shutdown(1);
            return;
        }

        ConfigureLogging();
        _serviceProvider = ConfigureServices();

        DispatcherUnhandledException += (_, args) =>
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            logger.LogCritical(args.Exception, "Unhandled exception");
            args.Handled = true;
        };

        InitializeAsync().ConfigureAwait(false);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var logger = _serviceProvider?.GetService<ILogger<App>>();

        try
        {
            _serviceProvider?.GetService<TrayIconService>()?.Dispose();
            _serviceProvider?.GetService<GlobalHotkeyService>()?.Dispose();

            if (_serviceProvider?.GetService<IInputEmulator>() is IDisposable emulator)
                emulator.Dispose();

            _serviceProvider?.Dispose();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during shutdown cleanup");
        }
        finally
        {
            ReleaseMutex();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

    private bool AcquireSingleInstance()
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (createdNew)
            return true;

        _mutex.Dispose();
        _mutex = null;
        return false;
    }

    private void ReleaseMutex()
    {
        if (_mutex is null)
            return;

        try
        {
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException)
        {
            // Mutex was not owned — safe to ignore
        }

        _mutex.Dispose();
        _mutex = null;
    }

    private static void ConfigureLogging()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "vcon", "logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDir, "vcon-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        services.AddSingleton<InputEmulatorFactory>();
        services.AddSingleton<IProfileManager, ProfileManager>();
        services.AddSingleton<OverlayViewModel>();
        services.AddSingleton<EditorViewModel>();
        services.AddSingleton<OverlayWindowService>();
        services.AddSingleton<GlobalHotkeyService>();
        services.AddSingleton<TrayIconService>();

        return services.BuildServiceProvider();
    }

    private async Task InitializeAsync()
    {
        var logger = _serviceProvider!.GetRequiredService<ILogger<App>>();
        var profileManager = _serviceProvider.GetRequiredService<IProfileManager>();
        var viewModel = _serviceProvider.GetRequiredService<OverlayViewModel>();
        var tray = _serviceProvider.GetRequiredService<TrayIconService>();

        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "vcon", "settings.json");

            AppSettings settings;
            if (File.Exists(settingsPath))
            {
                var json = await File.ReadAllTextAsync(settingsPath);
                settings = ProfileSerializer.DeserializeSettings(json) ?? new AppSettings();
            }
            else
            {
                settings = new AppSettings();
            }

            await profileManager.SwitchProfileAsync(settings.ActiveProfileId);
            var profile = profileManager.ActiveProfile;

            await Dispatcher.InvokeAsync(() =>
            {
                var overlayWindow = new OverlayWindow(viewModel);
                overlayWindow.LoadLayout(profile);
                overlayWindow.Show();

                var hotkeyService = _serviceProvider.GetRequiredService<GlobalHotkeyService>();
                hotkeyService.Initialize(overlayWindow, settings.Hotkeys);
                hotkeyService.ToggleOverlayRequested += (_, _) => viewModel.ToggleVisibility();
                hotkeyService.ToggleEditModeRequested += (_, _) => viewModel.ToggleEditMode();
                hotkeyService.CycleProfileRequested += (_, _) => _ = viewModel.CycleProfileAsync();

                tray.Initialize(viewModel);
            });

            logger.LogInformation("vcon started with profile '{ProfileName}'", profile.Name);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to initialize vcon");
            Shutdown(1);
        }
    }
}

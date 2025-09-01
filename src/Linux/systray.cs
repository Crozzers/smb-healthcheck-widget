using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using SMBUtils;
using Avalonia.Media.Imaging;
using Utils;

namespace SysTray;

public class Systray
{

    public Systray() { }

    public void Run()
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime([]);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .LogToTrace();
}

class App : Application
{
    NativeMenu? Menu = null;
    CancellationTokenSource _cts;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(FileUtils.LocalFilePath("assets/icon32.ico")),
                ToolTipText = "Samba Healthcheck Widget"
            };

            Menu = new NativeMenu();

            _cts = new CancellationTokenSource();
            MenuUpdateLoop(_cts.Token);

            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (_, _) =>
            {
                _cts.Cancel();
                desktop.Shutdown();
            };
            Menu.Items.Add(exitItem);

            trayIcon.Menu = Menu;
            trayIcon.IsVisible = true;

            PopulateMenu();

            // Keep app alive without window
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }

    private async Task MenuUpdateLoop(CancellationToken token)
    {
        if (Menu == null)
        {
            return;
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(600), token);
                PopulateMenu();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void PopulateMenu()
    {
        while (Menu.Items.Count > 1)
        {
            Menu.Items.RemoveAt(0);
        }

        foreach (var share in SMBShare.Enumerate())
        {
            string text;
            string icon;
            if (share.IsConnected())
            {
                text = $"{share.Address}/{share.Share}: ok";
                icon = "green";

                var storage = share.GetStorageSize();
                if (storage != null)
                {
                    var used = (int)(storage.Value.Used / 1_000_000_000);
                    var total = (int)(storage.Value.Total / 1_000_000_000);
                    text = $"{text} ({used}GB / {total}GB)";
                }
            }
            else
            {
                text = $"{share.Address}/{share.Share}: {share.Diagnose()}";
                icon = "red";
            }

            var menuItem = new NativeMenuItem(text)
            {
                Icon = new Bitmap(FileUtils.LocalFilePath($"assets/{icon}_dot.ico"))
            };
            menuItem.Click += (_, _) =>
            {
                var mountPoint = share.GetMountPoint();
                if (mountPoint != null)
                {
                    FileUtils.OpenDirectory(mountPoint);
                }
            };
            Menu.Items.Insert(0, menuItem);
        }

        // now add the refresh button
        var refreshItem = new NativeMenuItem($"Refresh (last: {DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2})");
        refreshItem.Click += (_, _) => PopulateMenu();
        Menu.Items.Insert(Menu.Items.Count - 1, new NativeMenuItemSeparator());
        Menu.Items.Insert(Menu.Items.Count - 1, refreshItem);
        Menu.Items.Insert(Menu.Items.Count - 1, new NativeMenuItemSeparator());
    }
}

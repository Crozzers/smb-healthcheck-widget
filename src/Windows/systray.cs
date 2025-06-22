using Microsoft.VisualBasic;
using SMBUtils;
using Utils;

namespace SysTray;

class Systray
{
    //Add Notify Icon and Task
    NotifyIcon TrayIcon;
    Task BalloonTask;
    ContextMenuStrip ContextMenu;

    public Systray()
    {
        ContextMenu = new ContextMenuStrip();
        var menuItem = new ToolStripMenuItem("Exit", null, Exit);
        ContextMenu.Items.Add(menuItem);

        TrayIcon = new NotifyIcon
        {
            Text = "Sambda Healthcheck Widget",
            Icon = new Icon(FileUtils.LocalFilePath("assets/icon32.ico"), 40, 40),
            Visible = true
        };

        BalloonTask = new Task(() => TaskOperation());
    }

    public void Run()
    {
        BalloonTask.Start();
        Application.Run();
    }

    private void PopulateMenu()
    {
        while (ContextMenu.Items.Count > 1)
        {
            ContextMenu.Items.RemoveAt(0);
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

            var menuItem = new ToolStripMenuItem(
                text,
                Image.FromFile(FileUtils.LocalFilePath($"assets/{icon}_dot.ico")),
                (_, _) => FileUtils.OpenDirectory(share.GetMountPoint())

            );
            ContextMenu.Items.Insert(0, menuItem);
        }
    }

    private void TaskOperation()
    {
        TrayIcon.ContextMenuStrip = ContextMenu;
        PopulateMenu();
        TrayIcon.Visible = true;
    }

    private void Exit(object sender, EventArgs e)
    {
        Application.Exit();
    }
}

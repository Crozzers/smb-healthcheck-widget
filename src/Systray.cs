#if _WINDOWS
using Microsoft.VisualBasic;
using WindowsSMB;
#else
using LinuxSMB;
#endif

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
            Icon = new Icon("assets/icon32.ico", 40, 40),
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
            }
            else
            {
                text = $"{share.Address}/{share.Share}: {share.Diagnose()}";
                icon = "red";
            }

            var menuItem = new ToolStripMenuItem(text, Image.FromFile($"assets/{icon}_dot.ico"));
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

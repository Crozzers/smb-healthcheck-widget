using SysTray;

namespace smb_healthcheck_widget;

public class Program
{
    static void Main()
    {
        var systray = new Systray();
        systray.Run();
    }
}

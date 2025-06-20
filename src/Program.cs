#if _WINDOWS
using SysTray;
#else
using LinuxSMB;
#endif

namespace smb_healthcheck_widget;

public class Program
{
    static void Main()
    {
#if _WINDOWS
        var systray = new Systray();
        systray.Run();
#else
    foreach (var share in SMBShare.Enumerate()) {
        if (share.IsConnected())
        {
            Console.Writeline($"{share.Address}/{share.Share}: ok");
        }
        else
        {
            Console.Writeline($"{share.Address}/{share.Share}: {share.Diagnose()}");
        }
    }
#endif
    }
}

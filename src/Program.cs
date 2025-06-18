#if _WINDOWS
using WindowsSMB;
#else
using LinuxSMB;
#endif


class Program
{
    static int Main()
    {

        foreach (var share in SMBShare.Enumerate())
        {
            Console.WriteLine($"{share.Address}/{share.Share}: ok");
            // if (share.IsConnected())
            // {
            // }
            // else
            // {
            //     Console.WriteLine($"{share.Address}/{share.Share}: {share.Diagnose()}");
            // }
        }
        return 0;
    }
}

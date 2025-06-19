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
            if (share.IsConnected())
            {
                Console.WriteLine($"{share.Address}/{share.Share}: ok");
            }
            else
            {
                Console.WriteLine($"{share.Address}/{share.Share}: {share.Diagnose()}");
            }
        }
        return 0;
    }
}

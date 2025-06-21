using System.Diagnostics;
using System.Runtime.InteropServices;
using SMBLibrary;
using SMBLibrary.Client;

namespace Utils;

public enum SMBDiagnosis
{
    Unreachable,
    GuestLoginFailed,
    ShareNotFound,
    Unknown
}


interface ISMBShare<T> where T: ISMBShare<T>
{
    public string Address { get; init; }
    public string Share { get; init; }
    public abstract static List<T> Enumerate();
    public abstract bool IsConnected();
};


public abstract class SMBShareBase: ISMBShare<SMBShareBase>
{
    public SMBShareBase(string address, string share)
    {
        Address = address;
        Share = share;
    }
    public string Address { get; init; }
    public string Share { get; init; }

    public SMBDiagnosis Diagnose()
    {
        var client = new SMB2Client();
        bool connected = client.Connect(Address, SMBTransportType.DirectTCPTransport);

        if (!connected)
        {
            return SMBDiagnosis.Unreachable;
        }

        NTStatus status = client.Login(String.Empty, "GUEST", String.Empty);
        if (status != NTStatus.STATUS_SUCCESS)
        {
            return SMBDiagnosis.GuestLoginFailed;
        }

        var shareAvailable = client.ListShares(out status).Contains(Share);
        client.Logoff();
        client.Disconnect();

        if (!shareAvailable)
        {
            return SMBDiagnosis.ShareNotFound;
        }

        return SMBDiagnosis.Unknown;
    }

    public static List<SMBShareBase> Enumerate()
    {
        throw new NotImplementedException();
    }

    public abstract bool IsConnected();
}


public class FileUtils
{
    public static string LocalFilePath(string path)
    {
        return Path.Combine(Directory.GetParent(Environment.ProcessPath).FullName, path);
    }
    public static void OpenDirectory(string directory)
    {
        var explorer = FindFileExplorer();
        if (explorer == null)
        {
            return;
        }

        var process = new ProcessStartInfo()
        {
            FileName = explorer,
            Arguments = directory
        };
        Process.Start(process);
    }

    private static string? FindFileExplorer()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "explorer.exe";
        }
        foreach (var explorer in (string[])["xdg-open", "nautilus", "dolphin"])
        {
            if (ExecutableExists(explorer))
            {
                return explorer;
            }
        }
        return null;
    }

    private static bool ExecutableExists(string exe)
    {
        foreach (var path in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
        {
            var fullPath = Path.Combine(path, exe);
            if (File.Exists(fullPath))
            {
                return true;
            }
        }
        return false;
    }
}

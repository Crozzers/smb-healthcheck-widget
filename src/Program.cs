using SMBLibrary;
using SMBLibrary.Client;

public struct SMBShare
{
    public SMBShare(string address, string share)
    {
        Address = address;
        Share = share;
    }
    public string Address { get; init; }
    public string Share { get; init; }
}

public enum SMBDiagnosis
{
    Unreachable,
    GuestLoginFailed,
    ShareNotFound,
    Unknown
}

class Program
{
    static int Main()
    {
        var shares = ParseFstab();
        for (var i = 0; i < shares.Count(); i++)
        {
            var share = shares[i];

            if (IsShareMounted(share))
            {
                Console.WriteLine($"{share.Address}/{share.Share}: ok");
            }
            else
            {
                Console.WriteLine($"{share.Address}/{share.Share}: {DiagnoseSMBConnection(share)}");
            }
        }
        return 0;
    }

    static List<SMBShare> ParseFstab()
    {
        var shares = new List<SMBShare>();

        var reader = new StreamReader("/etc/fstab");
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var drive = line.Split([' ', '\t'])[0];

            if (!drive.StartsWith("//"))
            {
                continue;
            }

            if (line.Split([' ', '\t'])[2] != "cifs")
            {
                continue;
            }

            drive = drive.Replace("//", "");
            var address = drive.Split('/')[0];

            shares.Add(new SMBShare(address, drive.Replace(address, "").Trim('/')));
        }

        return shares;
    }

    static bool IsShareMounted(SMBShare share)
    {
        var reader = new StreamReader("/proc/mounts");
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith($"//{share.Address}/{share.Share}"))
            {
                return true;
            }
        }
        return false;
    }

    static SMBDiagnosis DiagnoseSMBConnection(SMBShare share)
    {
        var client = new SMB2Client();
        bool connected = client.Connect(share.Address, SMBTransportType.DirectTCPTransport);

        if (!connected)
        {
            return SMBDiagnosis.Unreachable;
        }

        NTStatus status = client.Login(String.Empty, "GUEST", String.Empty);
        if (status != NTStatus.STATUS_SUCCESS)
        {
            return SMBDiagnosis.GuestLoginFailed;
        }

        var shareAvailable = client.ListShares(out status).Contains(share.Share);
        client.Logoff();
        client.Disconnect();

        if (!shareAvailable)
        {
            return SMBDiagnosis.ShareNotFound;
        }

        return SMBDiagnosis.Unknown;
    }
}
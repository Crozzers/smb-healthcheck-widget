using SMBLibrary;
using SMBLibrary.Client;

public struct SMBShare
{
    public SMBShare(string address, string share)
    {
        this.address = address;
        this.share = share;
    }
    public string address { get; init; }
    public string share { get; init; }
}

class Program
{
    static int Main()
    {
        foreach (SMBShare share in ParseFstab())
        {
            var client = new SMB2Client();
            bool connected = client.Connect(share.address, SMBTransportType.DirectTCPTransport);

            Console.WriteLine($"{share.address}: {connected}");

            NTStatus status = client.Login(String.Empty, "GUEST", String.Empty);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                var shares = client.ListShares(out status);
                foreach (var smallShare in shares)
                {
                    Console.WriteLine($"Share: {smallShare}");
                }
            }
            else
            {
                Console.WriteLine("Guest login failed");
            }

            client.Disconnect();
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

            drive = drive.Replace("//", "");
            var address = drive.Split('/')[0];

            shares.Add(new SMBShare(address, drive.Replace(address, "")));
        }

        return shares;
    }
}
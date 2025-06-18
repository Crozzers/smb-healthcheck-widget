using Utils;

namespace LinuxSMB;

public class SMBShare : SMBShareBase, ISMBShare<SMBShare>
{
    public SMBShare(string address, string share) : base(address, share) { }

    public new static List<SMBShare> Enumerate()
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

    public override bool IsConnected()
    {
        var reader = new StreamReader("/proc/mounts");
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith($"//{Address}/{Share}"))
            {
                return true;
            }
        }
        return false;
    }
}

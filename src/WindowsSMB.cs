using Utils;
using System.Management;
using System.Text.RegularExpressions;

namespace WindowsSMB;

public class SMBShare : SMBShareBase, ISMBShare<SMBShare>
{
    public SMBShare(string address, string share) : base(address, share) { }

    public new static List<SMBShare> Enumerate()
    {
        var shares = new List<SMBShare>();

        var searcher = new ManagementObjectSearcher(
            "root\\CIMV2",
            "SELECT * FROM Win32_MappedLogicalDisk"
        );
        foreach (ManagementObject drive in searcher.Get())
        {
            var provider = drive["ProviderName"].ToString();
            if (Regex.Match(provider, @"\\\\([^\\]+)").Groups[1] == null)
            {
                continue;
            }
            provider = provider.Trim('\\');
            var address = provider.Split("\\")[0];
            shares.Add(new SMBShare(address, provider.Replace(address, "").Trim('\\')));
        }

        return shares;
    }

    public override bool IsConnected()
    {
        return false;
    }
}

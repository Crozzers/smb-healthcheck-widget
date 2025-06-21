using Utils;
using System.Management;
using System.Runtime.Versioning;

namespace SMBUtils;

[SupportedOSPlatform("windows")]
public class SMBShare : SMBShareBase, ISMBShare<SMBShare>
{
    public SMBShare(string address, string share, string letter) : base(address, share)
    {
        Letter = letter;
    }

    public string Letter { get; init; }

    public new static List<SMBShare> Enumerate()
    {
        var shares = new List<SMBShare>();

        var searcher = new ManagementObjectSearcher(
            "root\\CIMV2",
            "SELECT * FROM Win32_MappedLogicalDisk"
        );
        foreach (ManagementObject drive in searcher.Get())
        {
            if (drive["ProviderName"] == null)
            {
                continue;
            }
            var provider = drive["ProviderName"].ToString().Trim('\\');
            var address = provider.Split("\\")[0];
            shares.Add(new SMBShare(address, provider.Replace(address, "").Trim('\\'), (string)drive["Name"]));
        }

        return shares;
    }

    public override bool IsConnected()
    {
        return Directory.Exists(Letter);
    }
}

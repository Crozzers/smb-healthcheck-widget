using SMBLibrary;
using SMBLibrary.Client;
using SMBLibrary.Server;

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

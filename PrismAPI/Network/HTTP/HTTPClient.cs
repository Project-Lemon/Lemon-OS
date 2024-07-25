using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4;
using System.Text;

namespace PrismAPI.Network.HTTP;

public class HTTPClient
{
	#region Constructors

	public HTTPClient(string URL)
    {
        this.URL = new(URL);
    }

    public HTTPClient(URL URL)
    {
        this.URL = URL;
    }

    public HTTPClient()
    {
        URL = new("");
    }

	#endregion

	#region Methods

	public byte[] Get()
    {
		int Port = URL.HasPort ? int.Parse(URL.Port) : 80;

        EndPoint EP = new(URL.Address, (ushort)Port);
        string Request =
            $"GET {URL.Path} HTTP/1.1\n" +
            "Connection: Keep - Alive";

        //TcpClient Client = new(URL.Address, Port);
        //Client.Connect(URL.Address, Port);
        //Client.Send(Encoding.UTF8.GetBytes(Request));
        //return Client.Receive(ref EP);
        return Array.Empty<byte>();
    }

    #endregion

    #region Fields

    public URL URL;

    #endregion

}
using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.System.Network.IPv4.UDP.DNS;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4;
using PrismAPI.Tools.Diagnostics;
using System.Net.Sockets;

namespace PrismAPI.Network;

public static class NetworkManager
{
	#region Methods

	/// <summary>
	/// Pings an IP address.
	/// </summary>
	/// <param name="A"></param>
	/// <returns>Time in miliseconds it takes for the server to respond.</returns>
	public static ulong Ping(Address A)
	{
		DateTime T = DateTime.Now;
		//new TcpClient().Connect(A, 80);
		//return (ulong)(DateTime.Now - T).TotalMilliseconds;
		return 0;
	}

	/// <summary>
	/// Initializes the system networking interface.
	/// </summary>
	public static void Init()
	{
		try
		{
			// Initialize the network;
			Debugger = new("Network");

			Debugger.WritePartial("Initializing network...");

			// Initialize networking.
			_ = new DHCPClient().SendDiscoverPacket();

			// Set-up the DNS (cloudflare).
			DNSClient.Connect(new(1, 1, 1, 1));
			Debugger.Finalize(Severity.Success);
		}
		catch
		{
			Debugger.Finalize(Severity.Fail);
		}
	}

	#endregion

	#region Fields

	public static DnsClient DNSClient = null!;
	public static Debugger Debugger = null!;

	#endregion
}
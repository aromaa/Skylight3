using System.Text;
using Net.Sockets;

namespace Skylight.API.Net.Connection;

public interface INetworkConnectionHandler
{
	//TODO
	public void Accept(ISocket socket, Encoding encoding, string revision, string? cryptoPrime = null, string? cryptoGenerator = null, string? cryptoKey = null, string? cryptoPremix = null);
}

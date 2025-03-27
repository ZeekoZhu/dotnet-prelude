using System.Text.Json;
using BlazorWire.Core;

namespace BlazorWire.Server;

public class ServerSideWire(IWireHub client) : IWire
{
  public async Task<TR> SendAsync<TR>(IWireAction<TR> request)
  {
    var result =
      await client.HandleWireAction(WireActionMessage.Create(request));
    return result.Deserialize<TR>()!;
  }
}

public class PrerenderWire() : IWire
{
  public Task<TR> SendAsync<TR>(IWireAction<TR> request)
  {
    throw new InvalidOperationException(
      "The client side wire is not available in prerender mode");
  }
}

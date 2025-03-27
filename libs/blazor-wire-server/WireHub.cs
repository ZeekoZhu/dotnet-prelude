using BlazorWire.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorWire.Server;

public class WireHub(IServiceProvider sp, ILogger<WireHub> logger)
  : Hub<IWireHub>
{
  public async Task<object?> HandleWireAction(WireActionMessage message)
  {
    var provider = sp.CreateAsyncScope();
    var handler = new ServerSideWireHandler(sp, new ServerSideWire(Clients.Caller));
    try
    {
      logger.LogInformation("Executing wire action: {WireActionMessage}",
        message.ActionName);
      return await handler.ExecuteHandlerFor(message);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error handling wire action");
      throw;
    }
    finally
    {
      await provider.DisposeAsync();
    }
  }
}

public class ServerRenderWire(
  IServiceProvider sp,
  ILogger<ServerRenderWire> logger) : IWire
{
  public async Task<TR> SendAsync<TR>(IWireAction<TR> request)
  {
    var provider = sp.CreateAsyncScope();
    var handler = new ServerSideWireHandler(sp, new PrerenderWire());
    try
    {
      var msg = WireActionMessage.Create(request);
      logger.LogInformation("Executing wire action: {WireActionMessage}",
        msg.ActionName);
      var result =
        await handler.ExecuteHandlerFor(msg);
      return (TR)result!;
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error handling wire action");
      throw;
    }
    finally
    {
      await provider.DisposeAsync();
    }
  }
}

public abstract class
  ServerSideWireActionHandlerBase<T, TR> : IWireActionHandler<T, TR>
  where T : IWireAction<TR>
{
  public IWire Client { get; set; } = null!;
  public abstract Task<TR> Handle(T request);
}


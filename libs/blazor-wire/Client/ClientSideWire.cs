using BlazorWire.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWire.Client;

public class ClientSideWire : IWire, IAsyncDisposable
{
  private readonly HubConnection _conn;

  public ClientSideWire(NavigationManager navigationManager)
  {
    _conn = new HubConnectionBuilder()
      .WithUrl(navigationManager.ToAbsoluteUri("/wirehub"))
      .AddJsonProtocol(options =>
      {
        options.PayloadSerializerOptions.Converters.Add(
          new Shared.UnitJsonConverter());
      })
      .WithAutomaticReconnect()
      .Build();

    _conn.On("HandleWireAction",
      async (WireActionMessage msg) =>
        await Handlers.ExecuteHandlerFor(msg));
  }

  public WireActionHandlers Handlers { get; } = new();

  public async Task<TR> SendAsync<TR>(IWireAction<TR> request)
  {
    if (_conn.State == HubConnectionState.Disconnected)
    {
      await _conn.StartAsync();
    }

    var message = WireActionMessage.Create(request);
    return await _conn.InvokeAsync<TR>("HandleWireAction", message);
  }

  public async ValueTask DisposeAsync()
  {
    await _conn.DisposeAsync();
  }
}

using BlazorWire.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWire.Client;

public static class WireServiceExtension
{
  public static IServiceCollection AddWire(this IServiceCollection services)
  {
    services.AddSingleton<ClientSideWire>();
    services.AddSingleton<IWire>(sp=> sp.GetRequiredService<ClientSideWire>());
    services.AddTransient<ComponentWire>();
    return services;
  }
}

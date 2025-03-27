using BlazorWire.Client;
using BlazorWire.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWire.Server;

public static class WireServiceExtensions
{
  public static IServiceCollection AddWire(this IServiceCollection services)
  {
    services.AddTransient<IWire, ServerRenderWire>();
    services.AddTransient<ComponentWire>();
    services.AddTransient<ServerSideWireHandler>();

    return services;
  }


  /// <summary>
  /// Registers the ServerSideWireHandlerTypes as a singleton and configures it using the provided action.
  /// </summary>
  /// <param name="services">The service collection to add the handler types to.</param>
  /// <param name="configure">Action to configure the handler types.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddWireHandler(
    this IServiceCollection services,
    Action<ServerSideWireHandlerTypes> configure)
  {
    services.AddSingleton<ServerSideWireHandlerTypes>(_ =>
    {
      var handlerTypes = new ServerSideWireHandlerTypes();
      configure(handlerTypes);
      return handlerTypes;
    });

    return services;
  }
}

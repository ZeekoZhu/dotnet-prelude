using System.Reflection;
using System.Text.Json;
using BlazorWire.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWire.Server;

public class ServerSideWireHandler(IServiceProvider sp, IWire client)
{
  public object GetHandler(WireActionMessage message)
  {
    var type = Type.GetType(message.ActionName) ?? throw new InvalidOperationException(
        $"Invalid request, unknown action type: {message.ActionName}");

    var handlerTypes = sp.GetRequiredService<ServerSideWireHandlerTypes>();
    if (!handlerTypes.TryGetHandlerType(type, out var handlerType) || handlerType == null)
    {
      throw new InvalidOperationException(
        $"Invalid request, unable to find handler type for action type: {message.ActionName}"
      );
    }
    var handler = sp.GetRequiredService(handlerType);

    // Set Client property if exists
    var clientProperty = handler.GetType().GetProperty("Client", typeof(IWire));
    if (clientProperty != null && clientProperty.CanWrite)
    {
        clientProperty.SetValue(handler, client);
    }

    return handler;
  }

  public object GetConcretePayload(object handler, WireActionMessage message)
  {
    var handlerType = handler.GetType();
    var handlerInterface = handlerType.GetInterfaces()
        .FirstOrDefault(i => i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IWireActionHandler<,>));

    if (handlerInterface == null)
    {
        throw new InvalidOperationException(
            $"Handler {handlerType.Name} doesn't implement IWireActionHandler<,>");
    }

    var payloadType = handlerInterface.GetGenericArguments()[0];
    return message.Payload.Deserialize(payloadType, WireActionMessage.JsonSerializerOptions)!;
  }

  private MethodInfo GetConcreteHandleMethod(object handler, Type payloadType)
  {
    var handleMethod = handler.GetType().GetMethod(
        "Handle",
        [payloadType]);

    if (handleMethod == null)
    {
        throw new InvalidOperationException(
            $"Handler {handler.GetType().Name} is missing Handle method for {payloadType.Name}");
    }

    return handleMethod;
  }

  public async Task<object?> ExecuteHandlerFor(WireActionMessage message)
  {
    var handler = GetHandler(message);
    var payload = GetConcretePayload(handler, message);
    var payloadType = handler.GetType().GetInterfaces()
        .First(i => i.GetGenericTypeDefinition() == typeof(IWireActionHandler<,>))
        .GetGenericArguments()[0];

    var handleMethod = GetConcreteHandleMethod(handler, payloadType);

    try
    {
        var task = (Task)handleMethod.Invoke(handler, [payload])!;
        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }
    catch (TargetInvocationException ex)
    {
        throw ex.InnerException ?? ex; // Unwrap async exceptions
    }
  }
}

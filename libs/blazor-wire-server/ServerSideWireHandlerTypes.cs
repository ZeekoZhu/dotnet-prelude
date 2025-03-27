using BlazorWire.Core;

namespace BlazorWire.Server;

public class ServerSideWireHandlerTypes
{
  public Dictionary<Type, Type?> ActionTypeToHandlerType { get; set; } = new();

  public void Add<T>() where T : IWireActionHandler
  {
    if (WireActionHandlerExtensions.TryGetActionType<T>(out var actionType) && actionType != null)
    {
      ActionTypeToHandlerType[actionType] = typeof(T);
    }
    else
    {
      throw new InvalidOperationException("Invalid handler type");
    }
  }

  public bool TryGetHandlerType(Type actionType, out Type? handlerType)
  {
    return ActionTypeToHandlerType.TryGetValue(actionType, out handlerType);
  }
}

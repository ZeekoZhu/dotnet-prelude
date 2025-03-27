using System.Text.Json;

namespace BlazorWire.Core;

public interface IWire
{
  /// <summary>
  /// Send a request to the server, and return the response
  /// </summary>
  /// <param name="request"></param>
  /// <typeparam name="TR"></typeparam>
  /// <returns></returns>
  public Task<TR> SendAsync<TR>(IWireAction<TR> request);
}

public interface IWireHub
{
  public Task<JsonDocument> HandleWireAction(WireActionMessage message);
}

public interface IWireActionHandler;

public interface IWireActionHandler<in T, TR> : IWireActionHandler
  where T : IWireAction<TR>
{
  Task<TR> Handle(T request);
}

public static class WireActionHandlerExtensions
{
  public static bool TryGetActionType<T>(
    out Type? actionType)
  {
    var handlerType = typeof(T);
    var interfaceType = handlerType.GetInterfaces()
      .FirstOrDefault(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() ==
                           typeof(IWireActionHandler<,>));

    if (interfaceType == null)
    {
      actionType = null;
      return false;
    }

    actionType = interfaceType.GetGenericArguments()[0];
    return true;
  }
}

public class InlineWireActionHandler<T, TR>(Func<T, Task<TR>> handler)
  : IWireActionHandler<T, TR>
  where T : IWireAction<TR>
{
  public Task<TR> Handle(T request) => handler(request);
}

public interface IWireAction;

public interface IWireAction<out TR> : IWireAction;

public record WireActionMessage(string ActionName, JsonDocument Payload)
{
  public static readonly JsonSerializerOptions JsonSerializerOptions =
    new(JsonSerializerDefaults.Web);

  static WireActionMessage()
  {
    JsonSerializerOptions.Converters.Add(new Shared.UnitJsonConverter());
  }

  public static WireActionMessage Create<T>(T request) where T : IWireAction =>
    new(request.GetType().AssemblyQualifiedName!,
      JsonSerializer.SerializeToDocument<object>(request,
        JsonSerializerOptions));
}

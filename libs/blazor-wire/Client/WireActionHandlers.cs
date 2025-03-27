using System.Collections.Concurrent;
using System.Text.Json;
using BlazorWire.Core;

namespace BlazorWire.Client;

public class WireActionHandlers
{
  private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
  private readonly ConcurrentBag<object> _handlers = [];

  protected virtual void SetupHandler(object handler)
  {
  }

  public void AddHandler<T, TR>(IWireActionHandler<T, TR> handler)
    where T : IWireAction<TR> =>
    _handlers.Add(handler);

  public void AddHandler(object handler) => _handlers.Add(handler);

  public void RemoveHandler<T, TR>(IWireActionHandler<T, TR> handler)
    where T : IWireAction<TR>
  {
    _handlers.TryTake(out _);
  }

  protected static object GetAction(WireActionMessage doc)
  {
    var actionName = doc.ActionName;

    if (actionName == null)
    {
      throw new InvalidOperationException(
        "Invalid request, missing ActionName in payload");
    }

    var type = Type.GetType(actionName);

    if (type == null)
    {
      throw new InvalidOperationException(
        $"Invalid request, unknown action type: {actionName}");
    }

    var req = doc.Payload.Deserialize(type, JsonSerializerOptions);

    if (req == null)
    {
      throw new InvalidOperationException("Invalid request");
    }

    return req;
  }
  public Task<object?> ExecuteHandlerFor(WireActionMessage message)
  {
    var request = GetAction(message);
    var actionInterface = request.GetType()
      .GetInterfaces()
      .FirstOrDefault(it => it.IsGenericType &&
                            it.GetGenericTypeDefinition() ==
                            typeof(IWireAction<>));

    if (actionInterface == null)
    {
      throw new InvalidOperationException(
        "Invalid request, missing IWireAction interface");
    }

    var responseType = actionInterface.GetGenericArguments()[0];

    var handlerType =
      typeof(IWireActionHandler<,>).MakeGenericType(request.GetType(),
        responseType);
    var handler =
      _handlers.FirstOrDefault(it => handlerType.IsInstanceOfType(it));
    if (handler == null)
    {
      throw new InvalidOperationException("No handler found");
    }

    SetupHandler(handler);

    var handlerMethod = handlerType
      .GetMethods()
      .FirstOrDefault(it =>
        it.Name == "Handle" && it.GetParameters().Length == 1 &&
        it.GetParameters()[0].ParameterType == request.GetType());

    if (handlerMethod == null)
    {
      throw new InvalidOperationException("No handler method found");
    }

    var result = handlerMethod.Invoke(handler, [request]);

    return AwaitResultTaskAsync(result);
  }

  public static async Task<object?> AwaitResultTaskAsync(
    object? resultObject)
  {
    switch (resultObject)
    {
      case Task task:
      {
        await task;

        // Use reflection to get the result for generic Task<T>
        var taskType = task.GetType();
        var result = taskType.GetProperty("Result")?.GetValue(task);
        return result;
      }
      default:
        return resultObject;
    }
  }
}

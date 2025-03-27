using BlazorWire.Core;
using R3;

namespace BlazorWire.Client;

public class ComponentWire(IWire wire)
{
  public Task<TR> SendAsync<TR>(IWireAction<TR> request) => wire.SendAsync(request);

  public IDisposable Subscribe<T, TR>(Func<T, Task<TR>> handlerFunc) where T : IWireAction<TR>
  {
    if (wire is not ClientSideWire w) return Disposable.Empty;
    var handler = new InlineWireActionHandler<T, TR>(handlerFunc);
    w.Handlers.AddHandler(handler);
    return Disposable.Create(() =>
    {
      w.Handlers.RemoveHandler(handler);
    });
  }

}

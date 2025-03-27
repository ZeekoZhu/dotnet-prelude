using System.Text.Json;
using BlazorWire.Core;
using NSubstitute;

namespace BlazorWire.Server.Test;

[TestFixture]
[TestOf(typeof(ServerSideWireHandler))]
public partial class ServerSideWireHandlerTest
{
  // Common setup and helpers remain in outer class
  private IServiceProvider _serviceProviderSub = null!;
  private IWire _wireSub = null!;
  private ServerSideWireHandler _handler = null!;

  [SetUp]
  public void Setup()
  {
    _serviceProviderSub = Substitute.For<IServiceProvider>();
    _wireSub = Substitute.For<IWire>();
    _handler = new ServerSideWireHandler(_serviceProviderSub, _wireSub);
  }

  private WireActionMessage CreateMessageForPayload(TestPayload payload)
  {
    return new WireActionMessage(
      typeof(ValidPayloadHandler).AssemblyQualifiedName!,
      JsonDocument.Parse(JsonSerializer.Serialize(payload,
        WireActionMessage.JsonSerializerOptions)));
  }

  // Test classes for each method

  // Test handler classes remain in outer class
  internal class TestAction : IWireAction<object> { }

  internal class TestHandler : IWireActionHandler<TestAction, object>
  {
    public Task<object> Handle(TestAction request) =>
      Task.FromResult(new object());
  }

  internal class TestActionWithClient : IWireAction<object> { }

  internal class
    TestHandlerWithClient : IWireActionHandler<TestActionWithClient, object>
  {
    public IWire Client { get; set; } = null!;

    public Task<object> Handle(TestActionWithClient request) =>
      Task.FromResult(new object());
  }

  internal class TestActionWithoutClient : IWireAction<object> { }

  internal class
    TestHandlerWithoutClient : IWireActionHandler<TestActionWithoutClient, object>
  {
    public Task<object> Handle(TestActionWithoutClient request) =>
      Task.FromResult(new object());
  }

  internal class TestPayload : IWireAction<string>
  {
    public string? TestProperty { get; set; }
  }

  internal class ValidPayloadHandler : IWireActionHandler<TestPayload, string>
  {
    public Task<string> Handle(TestPayload request) => Task.FromResult("test");
  }

  internal class InvalidPayloadHandler
  {
  }

  internal class SuccessfulHandlerAction : IWireAction<string>
  {
    public string? TestProperty { get; set; }
  }

  internal class SuccessfulHandler : IWireActionHandler<SuccessfulHandlerAction, string>
  {
    private readonly string _result;
    public SuccessfulHandler(string result) => _result = result;
    public Task<string> Handle(SuccessfulHandlerAction request) => Task.FromResult(_result);
  }

  internal class ExplicitInterfaceHandlerAction : IWireAction<object>
  {
    public string? TestProperty { get; set; }
  }

  internal class
    ExplicitInterfaceHandler : IWireActionHandler<ExplicitInterfaceHandlerAction, object>
  {
    Task<object> IWireActionHandler<ExplicitInterfaceHandlerAction, object>.Handle(
      ExplicitInterfaceHandlerAction request) =>
      Task.FromResult(new object());
  }

  internal class ExceptionThrowingHandlerAction : IWireAction<string>
  {
    public string? TestProperty { get; set; }
  }

  internal class
    ExceptionThrowingHandler : IWireActionHandler<ExceptionThrowingHandlerAction, string>
  {
    private readonly string _exceptionMessage;

    public ExceptionThrowingHandler(string message) =>
      _exceptionMessage = message;

    public Task<string> Handle(ExceptionThrowingHandlerAction request) =>
      throw new InvalidOperationException(_exceptionMessage);
  }

  internal class JsonOptionsTestHandlerAction : IWireAction<string>
  {
    public string? TestProperty { get; set; }
  }

  internal class
    JsonOptionsTestHandler : IWireActionHandler<JsonOptionsTestHandlerAction, string>
  {
    public Task<string> Handle(JsonOptionsTestHandlerAction request) =>
      Task.FromResult(request.TestProperty!);
  }

  internal abstract class BaseHandlerWithClient
  {
    public IWire Client { get; set; } = null!;
  }

  internal class TestActionWithBaseClient : IWireAction<object> { }

  internal class TestHandlerWithBaseClient : BaseHandlerWithClient, IWireActionHandler<TestActionWithBaseClient, object>
  {
    public Task<object> Handle(TestActionWithBaseClient request) =>
      Task.FromResult(new object());
  }
}

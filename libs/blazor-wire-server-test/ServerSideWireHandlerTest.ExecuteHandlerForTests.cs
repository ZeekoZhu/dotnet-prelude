using System.Text.Json;
using BlazorWire.Core;
using NSubstitute;

namespace BlazorWire.Server.Test;

public partial class ServerSideWireHandlerTest
{
  [TestFixture]
  [TestOf(typeof(ServerSideWireHandler))]
  public class ExecuteHandlerForTests
  {
    private ServerSideWireHandlerTest _parent = null!;

    [SetUp]
    public void Setup()
    {
      _parent = new ServerSideWireHandlerTest();
      _parent._serviceProviderSub = Substitute.For<IServiceProvider>();
      _parent._wireSub = Substitute.For<IWire>();

      // Setup the ServerSideWireHandlerTypes for testing
      var handlerTypes = new TestServerSideWireHandlerTypes();
      handlerTypes.ClearTypes();
      handlerTypes.Add<SuccessfulHandler>();
      handlerTypes.Add<ExplicitInterfaceHandler>();
      handlerTypes.Add<ExceptionThrowingHandler>();
      handlerTypes.Add<JsonOptionsTestHandler>();

      // Configure service provider to return our test instance
      _parent._serviceProviderSub.GetService(typeof(ServerSideWireHandlerTypes)).Returns(handlerTypes);

      _parent._handler =
        new ServerSideWireHandler(_parent._serviceProviderSub,
          _parent._wireSub);
    }

    [Test]
    public async Task ReturnsHandlerResult()
    {
      // Arrange
      var expectedResult = "test-result";
      var handler = new SuccessfulHandler(expectedResult);
      var message = new WireActionMessage(
        typeof(SuccessfulHandlerAction).AssemblyQualifiedName!,
        JsonDocument.Parse("{\"testProperty\": \"test\"}"));

      _parent._serviceProviderSub.GetService(typeof(SuccessfulHandler))
        .Returns(handler);

      // Act
      var result = await _parent._handler.ExecuteHandlerFor(message);

      // Assert
      Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void ThrowsWhenHandleMethodNotFound()
    {
      // Arrange
      var handler = new ExplicitInterfaceHandler();
      var message = new WireActionMessage(
        typeof(ExplicitInterfaceHandlerAction).AssemblyQualifiedName!,
        JsonDocument.Parse("{\"testProperty\": \"test\"}"));

      _parent._serviceProviderSub.GetService(typeof(ExplicitInterfaceHandler))
        .Returns(handler);

      // Act & Assert
      var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await _parent._handler.ExecuteHandlerFor(message));
      Assert.That(ex!.Message, Does.Contain("is missing Handle method for"));
    }

    [Test]
    public void PropagatesHandlerExceptions()
    {
      // Arrange
      var exceptionMessage = "Handler failure";
      var handler = new ExceptionThrowingHandler(exceptionMessage);
      var message = new WireActionMessage(
        typeof(ExceptionThrowingHandlerAction).AssemblyQualifiedName!,
        JsonDocument.Parse("{}")); // Using TestPayload as the action type

      _parent._serviceProviderSub.GetService(typeof(ExceptionThrowingHandler))
        .Returns(handler);

      // Act & Assert
      var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await _parent._handler.ExecuteHandlerFor(message));
      Assert.That(ex!.Message,
        Is.EqualTo(exceptionMessage)); // Changed to check ex.Message
    }

    [Test]
    public async Task UsesCorrectJsonOptions()
    {
      // Arrange
      var handler = new JsonOptionsTestHandler();
      var json = """{"testProperty": "camel-case-value"}""";
      var message = new WireActionMessage(
        typeof(JsonOptionsTestHandlerAction).AssemblyQualifiedName!,
        JsonDocument.Parse(json));

      _parent._serviceProviderSub.GetService(typeof(JsonOptionsTestHandler))
        .Returns(handler);

      // Act
      var result = await _parent._handler.ExecuteHandlerFor(message);

      // Assert
      Assert.That(result, Is.EqualTo("camel-case-value"));
    }
  }
}

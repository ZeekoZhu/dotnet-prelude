using System.Text.Json;
using BlazorWire.Core;
using NSubstitute;

namespace BlazorWire.Server.Test;

public partial class ServerSideWireHandlerTest
{
  [TestFixture]
  [TestOf(typeof(ServerSideWireHandler))]
  public class GetHandlerTests
  {
    private ServerSideWireHandlerTest _parent = null!;

    [SetUp]
    public void Setup()
    {
      _parent = new ServerSideWireHandlerTest
      {
        _serviceProviderSub = Substitute.For<IServiceProvider>(),
        _wireSub = Substitute.For<IWire>()
      };

      // Setup the ServerSideWireHandlerTypes for testing
      // Create a test subclass to access protected members for testing
      var handlerTypes = new TestServerSideWireHandlerTypes();
      handlerTypes.ClearTypes();
      handlerTypes.Add<TestHandler>();
      handlerTypes.Add<TestHandlerWithClient>();
      handlerTypes.Add<TestHandlerWithoutClient>();
      handlerTypes.Add<TestHandlerWithBaseClient>();

      // Configure service provider to return our test instance
      _parent._serviceProviderSub.GetService(typeof(ServerSideWireHandlerTypes)).Returns(handlerTypes);

      _parent._handler = new ServerSideWireHandler(_parent._serviceProviderSub, _parent._wireSub);
    }

    [Test]
    public void ReturnsRegisteredHandler()
    {
      // Arrange
      var expectedHandler = new TestHandler();
      var message = new WireActionMessage(
        typeof(TestAction).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      _parent._serviceProviderSub.GetService(typeof(TestHandler)).Returns(expectedHandler);

      // Act
      var result = _parent._handler.GetHandler(message);

      // Assert
      Assert.That(result, Is.InstanceOf<TestHandler>());
      Assert.That(result, Is.SameAs(expectedHandler));
    }

    [Test]
    public void ThrowsForUnknownActionType()
    {
      // Arrange
      var message = new WireActionMessage("Invalid.Type.Name", JsonDocument.Parse("{}"));

      // Act & Assert
      var ex = Assert.Throws<InvalidOperationException>(() => _parent._handler.GetHandler(message));
      Assert.That(ex!.Message, Does.Contain("Invalid request, unknown action type: Invalid.Type.Name"));
    }

    [Test]
    public void ThrowsForUnregisteredHandlerType()
    {
      // Arrange
      // Create a new empty handler types instance for this test
      var emptyHandlerTypes = new TestServerSideWireHandlerTypes();
      _parent._serviceProviderSub.GetService(typeof(ServerSideWireHandlerTypes)).Returns(emptyHandlerTypes);

      var message = new WireActionMessage(
        typeof(TestAction).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      // Act & Assert
      var ex = Assert.Throws<InvalidOperationException>(() => _parent._handler.GetHandler(message));
      Assert.That(ex!.Message, Does.Contain($"Invalid request, unable to find handler type for action type: {typeof(TestAction).AssemblyQualifiedName}"));

      // Restore handler types for other tests
      Setup();
    }

    [Test]
    public void SetsClientPropertyWhenExists()
    {
      // Arrange
      var handlerWithClient = new TestHandlerWithClient();
      var message = new WireActionMessage(
        typeof(TestActionWithClient).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      _parent._serviceProviderSub.GetService(typeof(TestHandlerWithClient)).Returns(handlerWithClient);

      // Act
      var result = _parent._handler.GetHandler(message);

      // Assert
      var typedResult = result as TestHandlerWithClient;
      Assert.That(typedResult!.Client, Is.SameAs(_parent._wireSub));
    }

    [Test]
    public void SkipsClientPropertyWhenNotExists()
    {
      // Arrange
      var handlerWithoutClient = new TestHandlerWithoutClient();
      var message = new WireActionMessage(
        typeof(TestActionWithoutClient).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      _parent._serviceProviderSub.GetService(typeof(TestHandlerWithoutClient)).Returns(handlerWithoutClient);

      // Act & Assert
      Assert.DoesNotThrow(() => _parent._handler.GetHandler(message));
    }

    [Test]
    public void SetsClientPropertyFromBaseClass()
    {
      // Arrange
      var handlerWithBaseClient = new TestHandlerWithBaseClient();
      var message = new WireActionMessage(
        typeof(TestActionWithBaseClient).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      _parent._serviceProviderSub.GetService(typeof(TestHandlerWithBaseClient)).Returns(handlerWithBaseClient);

      // Act
      var result = _parent._handler.GetHandler(message);

      // Assert
      var typedResult = result as TestHandlerWithBaseClient;
      Assert.That(typedResult!.Client, Is.SameAs(_parent._wireSub));
    }
  }

  // Test subclass to access protected members
  private class TestServerSideWireHandlerTypes : ServerSideWireHandlerTypes
  {
    public void ClearTypes()
    {
      ActionTypeToHandlerType.Clear();
    }
  }
}

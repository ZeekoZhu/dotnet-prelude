using System.Text.Json;
using BlazorWire.Core;
using NSubstitute;

namespace BlazorWire.Server.Test;

public partial class ServerSideWireHandlerTest
{
  [TestFixture]
  [TestOf(typeof(ServerSideWireHandler))]
  public class GetConcretePayloadTests
  {
    private ServerSideWireHandlerTest _parent = null!;

    [SetUp]
    public void Setup()
    {
      _parent = new ServerSideWireHandlerTest();
      _parent._serviceProviderSub = Substitute.For<IServiceProvider>();
      _parent._wireSub = Substitute.For<IWire>();
      _parent._handler = new ServerSideWireHandler(_parent._serviceProviderSub, _parent._wireSub);
    }

    [Test]
    public void ReturnsDeserializedPayload_WhenHandlerImplementsInterface()
    {
      // Arrange
      var handler = new ValidPayloadHandler();
      var expectedPayload = new TestPayload { TestProperty = "test-value" };
      var message = _parent.CreateMessageForPayload(expectedPayload);

      // Act
      var result = _parent._handler.GetConcretePayload(handler, message);

      // Assert
      Assert.That(result, Is.InstanceOf<TestPayload>());
      var payload = (TestPayload)result;
      Assert.That(payload.TestProperty, Is.EqualTo("test-value"));
    }

    [Test]
    public void Throws_WhenHandlerDoesNotImplementInterface()
    {
      // Arrange
      var invalidHandler = new InvalidPayloadHandler();
      var message = new WireActionMessage(
        typeof(InvalidPayloadHandler).AssemblyQualifiedName!,
        JsonDocument.Parse("{}"));

      // Act & Assert
      var ex = Assert.Throws<InvalidOperationException>(
        () => _parent._handler.GetConcretePayload(invalidHandler, message));

      Assert.That(ex.Message, Does.Contain("doesn't implement IWireActionHandler<,>"));
    }

    [Test]
    public void UsesCorrectJsonOptions_ForPropertyNaming()
    {
      // Arrange
      var handler = new ValidPayloadHandler();
      var json = """{"testProperty": "camel-case-value"}""";
      var message = new WireActionMessage(
        typeof(ValidPayloadHandler).AssemblyQualifiedName!,
        JsonDocument.Parse(json));

      // Act
      var result = _parent._handler.GetConcretePayload(handler, message);

      // Assert
      var payload = (TestPayload)result;
      Assert.That(payload.TestProperty, Is.EqualTo("camel-case-value"));
    }
  }
}

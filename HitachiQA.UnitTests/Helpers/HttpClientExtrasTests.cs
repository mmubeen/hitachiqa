using HitachiQA.Source.Hooks.HttpClientExtras;
using HitachiQA.Source.HttpClients;
using Microsoft.Extensions.Configuration;
using Telerik.JustMock;

namespace HitachiQA.UnitTests.Helpers
{
    [TestClass]
    public class HttpClientExtrasTests
    {
        HttpAuthHandler httpAuthHandler { get; init; }
        IConfiguration Configuration { get; init; }
        public HttpClientExtrasTests()
        {
            var innerHandlerMock = Mock.Create<HttpMessageHandler>();
            var clientMock = Mock.Create<AuthorizationClient>();
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new[]
             {
                new KeyValuePair<string, string?>("SERVER_HOST", "https://example.com/api")
            });
            Configuration = builder.Build();
            httpAuthHandler = Mock.Create<HttpAuthHandler>(innerHandlerMock, clientMock, Configuration);

        }
        [TestMethod]
        [DataRow("https://example.com/api/path")]
        [DataRow("http://example.com/api/path")]
        [DataRow("https://example.com/api/")]
        [DataRow("https://example.com/api")]
        [DataRow("https://example.com/")]
        [DataRow("https://example.com")]
        public void AuthRequired_SameHost_ReturnsTrue(string requestUriStr)
        {
            // Arrange
            var requestUri = new Uri(requestUriStr);

            // Act
            var result = httpAuthHandler.isAuthRequired(requestUri);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void AuthRequired_DifferentHost_ReturnsFalse()
        {
            // Arrange
            var requestUri = new Uri("https://exampled.com/");

            // Act
            var result = httpAuthHandler.isAuthRequired(requestUri);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AuthRequired_NullUri_ReturnsFalse()
        {
            // Arrange
            Uri? requestUri = null;

            // Act
            var result = httpAuthHandler.isAuthRequired(requestUri);

            // Assert
            result.Should().BeFalse();
        }
    }
}

using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using HitachiQA.Helpers;

namespace HitachiQA.UnitTests.Helpers
{
    [TestClass]
    public class RestAPITests
    {
        RestAPI RestAPI { get; init; }
        HttpClient HttpClient { get; init; }

        public RestAPITests()
        {
            RestAPI = Mock.Create<RestAPI>();
            HttpClient = Mock.Create<HttpClient>();
            HttpClient.BaseAddress = new Uri("https://example.com/api");
            RestAPI.Arrange(a => a.Client).Returns(HttpClient);
        }

        [TestMethod]
        [DataRow("api/resource")]
        [DataRow("/api/resource")]
        [DataRow("resource")]
        [DataRow("/resource")]
        public void BuildRequestMessage_ShouldBuildBasicRequest(string inputPath)
        {
            // Arrange

            // Act
            var request = RestAPI.BuildRequestMessage(HttpMethod.Get, inputPath, null, null, null);

            // Assert
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri.ToString().Should().BeOneOf("/api/resource", "api/resource");
            request.Headers.Authorization.Should().BeNull();
            request.Content.Should().BeNull();
        }
        [TestMethod]
        public void BuildRequestMessage_ShouldBuildRequestWithAuthorization()
        {
            // Arrange
            var authHeader = new AuthenticationHeaderValue("Bearer", "your_token");

            // Act
            var request = RestAPI.BuildRequestMessage(HttpMethod.Post, "api/resource", null, authHeader, null);

            // Assert
            request.Headers.Authorization.Should().Be(authHeader);
        }
        [TestMethod]
        public void BuildRequestMessage_ShouldBuildRequestWithHeaders()
        {
            // Arrange
            var headers = new Dictionary<string, string>
            {
                { "Header1", "Value1" },
                { "Header2", "Value2" }
            };

            // Act
            var request = RestAPI.BuildRequestMessage(HttpMethod.Put, "api/resource", null, null, headers);

            // Assert
            request.Headers.GetValues("Header1").Should().Contain("Value1");
            request.Headers.GetValues("Header2").Should().Contain("Value2");
        }
        [TestMethod]
        public void BuildRequestMessage_ShouldBuildRequestWithRequestBodyFromObject()
        {
            // Arrange
            var body = new { Key = "Value" };

            // Act
            var request = RestAPI.BuildRequestMessage(HttpMethod.Delete, "api/resource", body, null, null);

            // Assert
            request.Content.Should().NotBeNull();
            var content = request.Content.ReadAsStringAsync().Result;
            content.Should().Be("{\"Key\":\"Value\"}");
        }
        [TestMethod]
        public void BuildRequestMessage_ShouldBuildRequestWithRequestBodyFromString()
        {
            // Arrange
            var body = "{\"Key\":\"Value\"}";

            // Act
            var request = RestAPI.BuildRequestMessage(HttpMethod.Delete, "api/resource", body, null, null);

            // Assert
            request.Content.Should().NotBeNull();
            var content = request.Content.ReadAsStringAsync().Result;
            content.Should().Be("{\"Key\":\"Value\"}");
        }
    }
}

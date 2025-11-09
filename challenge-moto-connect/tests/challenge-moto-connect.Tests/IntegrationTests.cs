using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace challenge_moto_connect.Tests
{
    public class IntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public void Swagger_ReturnsOk()
        {
            // Act
            var response = _client.GetAsync("/swagger/index.html").Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void RootRedirectsToSwagger()
        {
            // Act
            var response = _client.GetAsync("/").Result;

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
        }
    }
}

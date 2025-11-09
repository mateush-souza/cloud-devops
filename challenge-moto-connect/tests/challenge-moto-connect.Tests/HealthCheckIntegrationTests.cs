using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace challenge_moto_connect.Tests
{
    public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/health");

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task HealthCheck_ReturnsJsonContentType()
        {
            var response = await _client.GetAsync("/health");

            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task HealthCheck_ReturnsHealthyStatus()
        {
            var response = await _client.GetAsync("/health");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Healthy", content);
        }
    }
}


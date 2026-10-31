using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Exchange.Tests.Fixtures;

using Xunit;

namespace Exchange.Tests
{
    public class SwaggerTests
    {
        [Fact]
        public async Task Root_ServesSwaggerWithApiKeySecurityWithoutLaunchingAsync()
        {
            using var deployment = new UtilityDeploymentFixture();
            var launcher = new CountingLauncher();
            await using var host = await ApiHostFixture.StartAsync(deployment, launcher: launcher);

            using var response = await host.Client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("swagger-ui", html);

            using var initializerResponse = await host.Client.GetAsync("/index.js");
            Assert.Equal(HttpStatusCode.OK, initializerResponse.StatusCode);
            var initializer = await initializerResponse.Content.ReadAsStringAsync();
            Assert.Contains("./swagger/v1/swagger.json", initializer);

            using var schemaResponse = await host.Client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, schemaResponse.StatusCode);
            var document = await schemaResponse.Content.ReadFromJsonAsync<JsonElement>();
            var operation = document.GetProperty("paths").GetProperty("/admin/exchanges").GetProperty("post");
            Assert.True(operation.GetProperty("responses").TryGetProperty("401", out _));
            var scheme = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("ApiKey");
            Assert.Equal("apiKey", scheme.GetProperty("type").GetString());
            Assert.Equal("header", scheme.GetProperty("in").GetString());
            Assert.Equal("X-Api-Key", scheme.GetProperty("name").GetString());
            Assert.True(document.GetProperty("security")[0].TryGetProperty("ApiKey", out _));
            var schema = await schemaResponse.Content.ReadAsStringAsync();
            Assert.False(schema.Contains(ApiHostFixture.Key, StringComparison.Ordinal), "Documentation must not expose the configured key.");

            using var unauthorized = await host.Client.PostAsJsonAsync("/admin/exchanges", new { });
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
            Assert.Equal(0, launcher.Calls);
        }
    }
}
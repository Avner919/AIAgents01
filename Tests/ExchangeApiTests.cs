using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Exchange.Tests.Fixtures;

using Xunit;

namespace Exchange.Tests
{
    public class ExchangeApiTests
    {
        [Fact]
        public async Task DefaultLaunch_ReturnsOwnedProcessIdBeforeReleaseAsync()
        {
            using var deployment = new UtilityDeploymentFixture();
            await using var child = new OwnedChildFixture();
            await using var host = await ApiHostFixture.StartAsync(deployment, () => deployment.EnvironmentValues(child.PipeName));
            host.Client.DefaultRequestHeaders.Add("X-Api-Key", ApiHostFixture.Key);
            using var response = await host.Client.PostAsJsonAsync("/admin/exchanges", new { });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var pid = json.GetProperty("processId").GetInt32();
            child.Attach(pid);
            var observation = await child.ReadObservationAsync();
            Assert.True(child.IsAlive);
            Assert.Equal(pid, observation.GetProperty("ProcessId").GetInt32());
            var defaults = json.GetProperty("defaults");
            Assert.Equal(6, defaults.EnumerateObject().Count());
            foreach (var property in defaults.EnumerateObject())
            {
                var name = char.ToUpperInvariant(property.Name[0]) + property.Name[1..];
                Assert.Equal(property.Value.ToString(), observation.GetProperty(name).ToString());
            }
            Assert.Equal(deployment.DirectoryPath, observation.GetProperty("WorkingDirectory").GetString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("wrong")]
        public async Task Unauthorized_DefaultLaunchDoesNotStartChildAsync(string? key)
        {
            using var deployment = new UtilityDeploymentFixture();
            var launcher = new CountingLauncher();
            await using var host = await ApiHostFixture.StartAsync(deployment, launcher: launcher);
            if (key != null)
            {
                host.Client.DefaultRequestHeaders.Add("X-Api-Key", key);
            }
            using var response = await host.Client.PostAsJsonAsync("/admin/exchanges", new { });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(0, launcher.Calls);
        }
    }
}
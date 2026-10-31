using Exchange.Api.Options;
using Exchange.Api.Services;
using Exchange.Tests.Fixtures;

using Xunit;

namespace Exchange.Tests
{
    public class UtilityProcessLauncherTests
    {
        [Fact]
        public async Task Start_UsesConfiguredTargetAndWorkingDirectoryAsync()
        {
            using var deployment = new UtilityDeploymentFixture();
            await using var child = new OwnedChildFixture();
            var preparation = new UtilityLaunchPreparation(new ApiOptions { UtilityPath = deployment.UtilityPath },
                () => deployment.EnvironmentValues(child.PipeName));
            var snapshot = preparation.Prepare();
            var pid = new UtilityProcessLauncher().Start(snapshot);
            child.Attach(pid);
            var observation = await child.ReadObservationAsync();
            Assert.Equal(pid, observation.GetProperty("ProcessId").GetInt32());
            Assert.Equal(deployment.DirectoryPath, observation.GetProperty("WorkingDirectory").GetString());
            Assert.True(child.IsAlive);
        }
    }
}
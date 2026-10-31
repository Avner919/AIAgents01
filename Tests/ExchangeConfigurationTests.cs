using System.Text.Json;

using Exchange.Configuration;
using Exchange.Tests.Fixtures;

using Xunit;

namespace Exchange.Tests
{
    public class ExchangeConfigurationTests
    {
        [Fact]
        public void SharedDefaults_UseUtilityFolderAndEnvironment()
        {
            using var deployment = new UtilityDeploymentFixture();
            File.WriteAllText(Path.Combine(deployment.DirectoryPath, "appsettings.json"),
                JsonSerializer.Serialize(new { Exchange = new { Agent = new { Timeout = 4567 } } }));
            var environment = deployment.EnvironmentValues();
            environment["Exchange__Agent__Timeout"] = "6789";
            var settings = ExchangeProcess.ResolveDefaults(ExchangeConfigurationLoader.Load(deployment.DirectoryPath, environment), deployment.DirectoryPath);
            Assert.Equal(6789, settings.TimeoutMilliseconds);
            Assert.Equal(Path.Combine(deployment.DirectoryPath, "client folder"), settings.ClientWorkingDirectory);
            Assert.Equal(Path.Combine(settings.ClientWorkingDirectory, settings.ClientFileName), settings.ClientExecutablePath);
            Assert.Equal(Path.Combine(deployment.DirectoryPath, "input folder"), settings.InputDirectory);
            Assert.NotEqual(deployment.DirectoryPath, settings.ClientWorkingDirectory);
        }
    }
}
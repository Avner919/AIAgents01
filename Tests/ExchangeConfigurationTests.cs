using System.Text.Json;

using Exchange.Configuration;
using Exchange.Tests.Fixtures;

using Xunit;

namespace Exchange.Tests
{
    public class ExchangeConfigurationTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void BlankExchangeDirectories_RemainEmptyInChildAndArguments(string? value)
        {
            using var deployment = new UtilityDeploymentFixture();
            File.WriteAllText(Path.Combine(deployment.DirectoryPath, "appsettings.json"),
                JsonSerializer.Serialize(new { Exchange = new { InFilePath = value, OutFilePath = value } }));
            var environment = deployment.EnvironmentValues();
            var settings = ExchangeProcess.ResolveDefaults(ExchangeConfigurationLoader.Load(deployment.DirectoryPath, environment), deployment.DirectoryPath);
            var startInfo = ExchangeProcess.BuildClientStartInfo(settings);
            Assert.Equal(string.Empty, settings.InputDirectory);
            Assert.Equal(string.Empty, settings.OutputDirectory);
            Assert.Equal(4, startInfo.ArgumentList.Count);
            Assert.Equal(string.Empty, startInfo.ArgumentList[2]);
            Assert.Equal(string.Empty, startInfo.ArgumentList[3]);

            var childEnvironment = settings.CreateChildEnvironment(environment);
            var childSettings = ExchangeProcess.ResolveDefaults(ExchangeConfigurationLoader.Load(deployment.DirectoryPath, childEnvironment), deployment.DirectoryPath);
            Assert.Equal(string.Empty, childSettings.InputDirectory);
            Assert.Equal(string.Empty, childSettings.OutputDirectory);
        }

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

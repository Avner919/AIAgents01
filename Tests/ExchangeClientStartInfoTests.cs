using System.Diagnostics;

using Exchange.Options;

using Xunit;

namespace Exchange.Tests
{
    public class ExchangeClientStartInfoTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void BuildClientStartInfo_PreservesOrderedArguments(int scenario)
        {
            var utilityDirectory = Path.Combine(Path.GetTempPath(), scenario >= 2 ? "utility folder" : "utility");
            var clientDirectory = scenario == 3 ? Path.Combine(utilityDirectory, "absolute client") : "client";
            var inputDirectory = scenario == 3 ? Path.Combine(utilityDirectory, "absolute input") : "input files";
            var outputDirectory = scenario == 3 ? Path.Combine(utilityDirectory, "absolute output") : "output files";
            var options = new ExchangeOptions
            {
                AgentCatalog = clientDirectory,
                InFilePath = inputDirectory,
                OutFilePath = outputDirectory,
                Agent = new AgentOptions
                {
                    Login = scenario == 1 ? "synthetic user" : "synthetic-user",
                    Password = scenario == 1 ? "synthetic password" : "synthetic-password",
                    Timeout = 1234
                }
            };

            var settings = ExchangeProcess.ResolveDefaults(options, utilityDirectory);
            var startInfo = ExchangeProcess.BuildClientStartInfo(settings);
            var expectedArguments = new[]
            {
                options.Agent.Login, options.Agent.Password,
                Path.GetFullPath(inputDirectory, utilityDirectory), Path.GetFullPath(outputDirectory, utilityDirectory)
            };

            Assert.True(startInfo.ArgumentList.SequenceEqual(expectedArguments), "Client argument order and boundaries must be preserved.");
            Assert.True(string.IsNullOrEmpty(startInfo.Arguments), "The combined argument string must be empty.");
            Assert.False(startInfo.UseShellExecute);
            Assert.Equal(Path.GetFullPath(clientDirectory, utilityDirectory), startInfo.WorkingDirectory);
            Assert.Equal(Path.Combine(startInfo.WorkingDirectory, "ExchangeClient.exe"), startInfo.FileName);
        }
    }
}
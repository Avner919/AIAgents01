using System.Text.Json;

namespace Exchange.Tests.Fixtures
{
    public sealed class UtilityDeploymentFixture : IDisposable
    {
        public UtilityDeploymentFixture()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "Exchange.Tests", Guid.NewGuid().ToString("N"), "utility folder");
            Directory.CreateDirectory(DirectoryPath);
            foreach (var file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "HarmlessUtility")))
            {
                File.Copy(file, Path.Combine(DirectoryPath, Path.GetFileName(file)));
            }
            File.WriteAllText(Path.Combine(DirectoryPath, "embeddedsettings.json"), JsonSerializer.Serialize(new
            {
                Exchange = new
                {
                    AgentCatalog = "client folder",
                    InFilePath = "input folder",
                    OutFilePath = "output folder",
                    Agent = new { Login = "synthetic-user", Password = "synthetic-password", Timeout = 1234 }
                }
            }));
            File.WriteAllText(Path.Combine(DirectoryPath, "appsettings.json"), "{}");
            File.WriteAllText(Path.Combine(DirectoryPath, "exchangeNlog.config"), "<nlog />");
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "client folder"));
            File.WriteAllText(Path.Combine(DirectoryPath, "client folder", "ExchangeClient.exe"), "never executed");
        }

        public string DirectoryPath { get; }
        public string UtilityPath => Path.Combine(DirectoryPath, "HarmlessUtility.exe");

        public Dictionary<string, string> EnvironmentValues(string? pipe = null)
        {
            var result = Exchange.Configuration.ExchangeConfigurationLoader.CaptureEnvironment()
                .Where(p => !p.Key.Replace("__", ":").StartsWith("Exchange:", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
            if (pipe != null)
            {
                result["EXCHANGE_TEST_PIPE"] = pipe;
            }
            return result;
        }

        public void Dispose()
        {
            Directory.Delete(Path.GetDirectoryName(DirectoryPath)!, true);
        }
    }
}
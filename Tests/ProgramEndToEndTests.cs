using System.Diagnostics;

using Xunit;

namespace Exchange.Tests
{
    public class ProgramEndToEndTests
    {
        [Fact]
        public async Task InvalidConfiguration_ReportsErrorAndReturnsFailureAsync()
        {
            var applicationDirectory = Path.Combine(AppContext.BaseDirectory, "ExchangeApp");
            var testDirectory = Path.Combine(Path.GetTempPath(), "Exchange.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDirectory);

            try
            {
                foreach (var file in Directory.EnumerateFiles(applicationDirectory))
                {
                    File.Copy(file, Path.Combine(testDirectory, Path.GetFileName(file)));
                }

                await File.WriteAllTextAsync(Path.Combine(testDirectory, "embeddedsettings.json"), "{}");
                // Fail before ExchangeProcess can start or stop external client processes.
                await File.WriteAllTextAsync(Path.Combine(testDirectory, "appsettings.json"), "{ invalid json }");
                await File.WriteAllTextAsync(Path.Combine(testDirectory, "exchangeNlog.config"),
                    """
                    <nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
                          xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                      <targets><target name="discard" xsi:type="Null" /></targets>
                      <rules><logger name="*" minlevel="Trace" writeTo="discard" /></rules>
                    </nlog>
                    """);

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
                        WorkingDirectory = testDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                process.StartInfo.ArgumentList.Add(Path.Combine(testDirectory, "Exchange.dll"));
                Assert.True(process.Start(), "The Exchange executable could not be started.");
                var standardOutput = process.StandardOutput.ReadToEndAsync();
                var standardError = process.StandardError.ReadToEndAsync();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                try
                {
                    await process.WaitForExitAsync(timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                    throw new TimeoutException("Exchange did not exit within 30 seconds after invalid configuration.");
                }

                var error = await standardError;
                var output = await standardOutput;
                Assert.True(process.ExitCode == 99,
                    $"Expected exit code 99; got {process.ExitCode}. stderr: {error}; stdout: {output}");
                Assert.Contains("appsettings.json", error);
                Assert.Contains("InvalidDataException", error);
            }
            finally
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }
}
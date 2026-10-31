using System.IO.Pipes;
using System.Text.Json;

using Exchange;
using Exchange.Configuration;

using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var directory = Environment.CurrentDirectory;
var environment = ExchangeConfigurationLoader.CaptureEnvironment();
var settings = ExchangeProcess.ResolveDefaults(ExchangeConfigurationLoader.Load(directory, environment), directory);
using var pipe = new NamedPipeClientStream(".", environment["EXCHANGE_TEST_PIPE"], PipeDirection.InOut, PipeOptions.Asynchronous);
await pipe.ConnectAsync(deadline.Token);
using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
using var reader = new StreamReader(pipe, leaveOpen: true);
await writer.WriteLineAsync(JsonSerializer.Serialize(new
{
    ProcessId = Environment.ProcessId,
    WorkingDirectory = directory,
    settings.ClientFileName,
    settings.ClientExecutablePath,
    settings.ClientWorkingDirectory,
    settings.InputDirectory,
    settings.OutputDirectory,
    settings.TimeoutMilliseconds
}));
await reader.ReadLineAsync(deadline.Token);
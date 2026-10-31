using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace Exchange.Tests.Fixtures
{
    public sealed class OwnedChildFixture : IAsyncDisposable
    {
        private readonly NamedPipeServerStream _pipe;
        private Process? _process;
        private StreamWriter? _writer;
        private StreamReader? _reader;

        public OwnedChildFixture()
        {
            PipeName = "exchange-test-" + Guid.NewGuid().ToString("N");
            _pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public string PipeName { get; }
        public bool IsAlive => _process != null && !_process.HasExited;
        public void Attach(int pid) => _process = Process.GetProcessById(pid);

        public async Task<JsonElement> ReadObservationAsync()
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _pipe.WaitForConnectionAsync(deadline.Token);
            _writer = new StreamWriter(_pipe, leaveOpen: true) { AutoFlush = true };
            _reader = new StreamReader(_pipe, leaveOpen: true);
            var line = await _reader.ReadLineAsync(deadline.Token);
            using var document = JsonDocument.Parse(line!);
            return document.RootElement.Clone();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_writer != null && IsAlive)
                {
                    await _writer.WriteLineAsync("release").WaitAsync(TimeSpan.FromSeconds(5));
                }
                if (IsAlive)
                {
                    await _process!.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception ex) when (ex is TimeoutException or IOException or InvalidOperationException)
            {
                if (IsAlive)
                {
                    _process!.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
                }
            }
            finally
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _pipe.Dispose();
                _process?.Dispose();
            }
        }
    }
}
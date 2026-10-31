using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Exchange.Api.Services
{
    public sealed class UtilityLaunchSnapshot
    {
        public UtilityLaunchSnapshot(string utilityPath, ResolvedExchangeSettings settings, IReadOnlyDictionary<string, string> environment)
        {
            UtilityPath = utilityPath;
            UtilityDirectory = Path.GetDirectoryName(utilityPath)!;
            Settings = settings;
            Environment = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(environment, StringComparer.OrdinalIgnoreCase));
        }

        public string UtilityPath { get; }
        public string UtilityDirectory { get; }
        public ResolvedExchangeSettings Settings { get; }
        public IReadOnlyDictionary<string, string> Environment { get; }
    }

    public sealed class UtilityProcessLauncher : IUtilityProcessLauncher
    {
        public int Start(UtilityLaunchSnapshot snapshot)
        {
            var info = new ProcessStartInfo(snapshot.UtilityPath)
            {
                WorkingDirectory = snapshot.UtilityDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            info.Environment.Clear();
            foreach (var pair in snapshot.Environment)
            {
                info.Environment[pair.Key] = pair.Value;
            }
            using var process = Process.Start(info) ?? throw new InvalidOperationException("Utility did not start.");
            return process.Id;
        }
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Exchange
{
    public sealed class ResolvedExchangeSettings
    {
        private readonly string _login;
        private readonly string _password;

        public ResolvedExchangeSettings(string clientFileName, string clientExecutablePath,
            string clientWorkingDirectory, string inputDirectory, string outputDirectory,
            int timeoutMilliseconds, string login, string password)
        {
            ClientFileName = clientFileName;
            ClientExecutablePath = clientExecutablePath;
            ClientWorkingDirectory = clientWorkingDirectory;
            InputDirectory = inputDirectory;
            OutputDirectory = outputDirectory;
            TimeoutMilliseconds = timeoutMilliseconds;
            _login = login;
            _password = password;
        }

        public string ClientFileName { get; }
        public string ClientExecutablePath { get; }
        public string ClientWorkingDirectory { get; }
        public string InputDirectory { get; }
        public string OutputDirectory { get; }
        public int TimeoutMilliseconds { get; }

        internal string Login => _login;
        internal string Password => _password;

        public IReadOnlyDictionary<string, string> CreateChildEnvironment(IReadOnlyDictionary<string, string> inherited)
        {
            var values = new Dictionary<string, string>(inherited, System.StringComparer.OrdinalIgnoreCase);
            var pinned = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                ["Exchange:AgentCatalog"] = ClientWorkingDirectory,
                ["Exchange:InFilePath"] = InputDirectory,
                ["Exchange:OutFilePath"] = OutputDirectory,
                ["Exchange:Agent:Timeout"] = TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture),
                ["Exchange:Agent:Login"] = _login,
                ["Exchange:Agent:Password"] = _password
            };
            foreach (var key in values.Keys.Where(k => pinned.ContainsKey(k.Replace("__", ":"))).ToArray())
            {
                values.Remove(key);
            }
            foreach (var pair in pinned)
            {
                values[pair.Key.Replace(":", "__")] = pair.Value;
            }
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(values);
        }
    }
}
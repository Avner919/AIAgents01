using Exchange.Api.Options;
using Exchange.Configuration;

namespace Exchange.Api.Services
{
    public sealed class UtilityLaunchPreparation(ApiOptions options, Func<IReadOnlyDictionary<string, string>> captureEnvironment)
    {
        public UtilityLaunchSnapshot Prepare()
        {
            if (!Path.IsPathFullyQualified(options.UtilityPath) || !File.Exists(options.UtilityPath)
                || !string.Equals(Path.GetExtension(options.UtilityPath), ".exe", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Utility executable is unavailable.");
            }
            var directory = Path.GetDirectoryName(options.UtilityPath)!;
            if (!File.Exists(Path.Combine(directory, "exchangeNlog.config")))
            {
                throw new InvalidOperationException("Utility logging configuration is unavailable.");
            }
            var inherited = new Dictionary<string, string>(captureEnvironment(), StringComparer.OrdinalIgnoreCase);
            var settings = ExchangeProcess.ResolveDefaults(ExchangeConfigurationLoader.Load(directory, inherited), directory);
            if (!File.Exists(settings.ClientExecutablePath))
            {
                throw new InvalidOperationException("Default client executable is unavailable.");
            }
            return new UtilityLaunchSnapshot(options.UtilityPath, settings, settings.CreateChildEnvironment(inherited));
        }
    }
}
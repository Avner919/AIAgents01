using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;

using Exchange.Options;

namespace Exchange.Configuration
{
    public static class ExchangeConfigurationLoader
    {
        public static IReadOnlyDictionary<string, string> CaptureEnvironment()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                result[(string)item.Key] = (string)item.Value;
            }
            return result;
        }

        public static ExchangeOptions Load(string utilityDirectory, IReadOnlyDictionary<string, string> environment)
        {
            var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in environment)
            {
                normalized[item.Key.Replace("__", ":")] = item.Value;
            }
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(utilityDirectory))
                .AddJsonFile("embeddedsettings.json", false, false)
                .AddJsonFile("appsettings.json", false, false)
                .AddInMemoryCollection(normalized)
                .Build();
            using var lifetime = config as IDisposable;
            return config.GetSection(ExchangeOptions.SectionName).Get<ExchangeOptions>()
                ?? throw new InvalidOperationException("Exchange configuration is missing.");
        }
    }
}
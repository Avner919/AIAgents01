using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Web;
using Exchange.Options;

namespace Exchange
{
    public static class Program
    {
        private const string LogMessage = "В 'Exchange' возникла ошибка: ";

        private static int Main()
        {
            var logger = NLogBuilder.ConfigureNLog("exchangeNlog.config")
                                    .GetCurrentClassLogger();

            try
            {
                var config = new ConfigurationBuilder().AddJsonFile("embeddedsettings.json")
                                                       .AddJsonFile("appsettings.json")
                                                       .AddEnvironmentVariables()
                                                       .Build();

                var exchangeOptions = config.GetSection(ExchangeOptions.SectionName)
                                            .Get<ExchangeOptions>();

                var exchangeProcess = new ExchangeProcess(logger, exchangeOptions);

                exchangeProcess.StartExchangeProcess();

                return 0;
            }
            catch (Exception exception)
            {
                logger.Error(exception, LogMessage);

                Console.Error.WriteLine(LogMessage + exception);

                return GetExitCode(exception.Message);
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static int GetExitCode(string message)
        {
            var regex = Regex.Match(message, @"^(\d+):.+", RegexOptions.None, TimeSpan.FromMilliseconds(5));
            var result = regex.Success
                ? regex.Groups[1].Value
                : string.Empty;

            return int.TryParse(result, out var code)
                 ? code
                 : 99;
        }
    }
}

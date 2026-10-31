using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using System.Linq;

using NLog;

using Exchange.Options;

namespace Exchange
{
    public class ExchangeProcess(Logger logger, ExchangeOptions exchangeOptions)
    {
        // Имя процесса
        private const string ExchangeProcessName = "ExchangeClient";
        // Имя файла 
        private const string ExchangeProcessFileName = ExchangeProcessName + ".exe";

        private readonly ResolvedExchangeSettings _settings = ResolveDefaults(exchangeOptions, Environment.CurrentDirectory);

        public static ResolvedExchangeSettings ResolveDefaults(ExchangeOptions options, string utilityDirectory)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (options.Agent == null || string.IsNullOrWhiteSpace(options.AgentCatalog)
                || options.Agent.Login == null || options.Agent.Password == null || options.Agent.Timeout < -1)
            {
                throw new InvalidOperationException("Exchange configuration is invalid.");
            }
            var root = Path.GetFullPath(utilityDirectory);
            var clientDirectory = Path.GetFullPath(options.AgentCatalog, root);
            return new ResolvedExchangeSettings(ExchangeProcessFileName, Path.Combine(clientDirectory, ExchangeProcessFileName),
                clientDirectory, ResolveExchangeDirectory(options.InFilePath, root), ResolveExchangeDirectory(options.OutFilePath, root),
                options.Agent.Timeout, options.Agent.Login, options.Agent.Password);
        }

        private static string ResolveExchangeDirectory(string path, string utilityDirectory)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetFullPath(path, utilityDirectory);
        }

        public static ProcessStartInfo BuildClientStartInfo(ResolvedExchangeSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            var info = new ProcessStartInfo
            {
                FileName = settings.ClientExecutablePath,
                WorkingDirectory = settings.ClientWorkingDirectory,
                UseShellExecute = false
            };
            info.ArgumentList.Add(settings.Login);
            info.ArgumentList.Add(settings.Password);
            info.ArgumentList.Add(settings.InputDirectory);
            info.ArgumentList.Add(settings.OutputDirectory);
            return info;
        }

        // Событие, инициирующее закрытие 
        private const string CloseExchangeEventName = "CloseExchng";
        // Событие, инициирующее запуск обмена со шлюзом
        private const string OneTimeExchangeEventName = "OneTimeExchng";
        // Событие, сообщающее о завершении обмена со шлюзом
        private const string ExchangeEndEventName = "ExchngEnd";
        // Событие, сообщающее о завершении запуска процесса

        private const string LoadEndEventName = "LoadEnd";

        // Обработчики событий процесса
        private IDictionary<string, EventWaitHandle> _exchangeEventHandlers;

        public void StartExchangeProcess()
        {
            try
            {
                logger.Info("Начинаем обмен со шлюзом");

                // Регистрируем обработчики событий
                RegisterEventHandlers();

                // Получаем существующий или инициализируем новый процесс обмена со шлюзом.
                StartClient();

                // Создаем обработчик события завершения обмена из агента обмена.
                CreateEventHandlerExchangeEnd();

                // Ожидаем обмен завершения обмена за отведенное время
                StartExchangeAndWaitExchangeEnd();

                logger.Info("Завершаем обмен с ГП");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Произошла ошибка при обмене файлами со шлюзом. Ошибка вызвавшая исключение: ");

                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                CloseEventHandlers();

                StopClients();
            }
        }

        /// <summary>
        ///     Создаем обработчики событий процесса.
        /// </summary>
        private void RegisterEventHandlers()
        {
            logger.Info("Получаем обработчики событий.");

            _exchangeEventHandlers = new Dictionary<string, EventWaitHandle>
            {
                [CloseExchangeEventName] = CreateEventByName(CloseExchangeEventName),
                [OneTimeExchangeEventName] = CreateEventByName(OneTimeExchangeEventName),
                [ExchangeEndEventName] = null,
                [LoadEndEventName] = CreateEventByName(LoadEndEventName)
            };
        }

        /// <summary>
        ///     Запуск процесса
        /// </summary>
        private void StartClient()
        {
            StopClients();

            //Получаем полный путь до утилиты
            var processPath = _settings.ClientExecutablePath;
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException($"1: Утилита обмена {ExchangeProcessFileName} не найдена в каталоге {exchangeOptions.AgentCatalog}.");
            }

            logger.Info("Создаем процесс");

            var clientProcess = new Process
            {
                StartInfo = BuildClientStartInfo(_settings)
            };

            clientProcess.Start();

            logger.Info("Создали и запустили процесс.");

            logger.Info($"Ожидаем завершения процедуры запуска процесса {exchangeOptions.Agent.Timeout / 1000} секунд.");

            //Ожидаем в основном потоке запуска утилиты обмена (ожидание указано в файле конфигурации)
            if (_exchangeEventHandlers[LoadEndEventName].WaitOne(_settings.TimeoutMilliseconds))
            {
                _exchangeEventHandlers[LoadEndEventName].Reset();
            }
            else
            {
                throw new TimeoutException("2: Процесс не сообщил о завершении своего запуска.");
            }

            logger.Info("Процесс успешно запущен.");
        }

        /// <summary>
        ///     Создаем обработчик события завершения обмена со шлюзом.
        /// </summary>
        private void CreateEventHandlerExchangeEnd()
        {
            logger.Info("Получаем обработчик события завершения обмена со шлюзом.");

            _exchangeEventHandlers[ExchangeEndEventName] = OpenEventByName(ExchangeEndEventName)
                                                        ?? throw new ArgumentException("3: Не удалось получить обработчик события завершения обмена со шлюзом.");
        }

        /// <summary>
        ///  Запуск обмена со шлюзом и ожидание его завершения в течение таймаута, заданного в настройках.
        /// </summary>
        private void StartExchangeAndWaitExchangeEnd()
        {
            logger.Info("Запускаем обмен со шлюзом .");

            // Сбрасываем событие завершения обмена, так как это событие может висеть
            // в установленном состоянии с предыдущей попытки запуска обмена со шлюзом.
            _exchangeEventHandlers[ExchangeEndEventName].Reset();

            // Запуск обмена
            _exchangeEventHandlers[OneTimeExchangeEventName].Set();

            // Ожидание завершения обмена
            if (_exchangeEventHandlers[ExchangeEndEventName].WaitOne(_settings.TimeoutMilliseconds))
            {
                _exchangeEventHandlers[ExchangeEndEventName].Reset();

                logger.Info("Обмен со шлюзом успешно завершен.");
            }
            else
            {
                throw new TimeoutException("4: Процесс не сообщил о завершении обмена со шлюзом.");
            }
        }

        /// <summary>
        ///     Метод останавливает все обнаруженные процессы 
        /// </summary>
        private void StopClients()
        {
            var clientProcesses = Process.GetProcesses()
                                            .Where(p => string.Equals(p.ProcessName, ExchangeProcessName,
                                                                      StringComparison.InvariantCultureIgnoreCase))
                                            .ToArray();

            foreach (var process in clientProcesses)
            {
                logger.Info($"Останавливаем процесс с ИД '{process.Id}'.");

                process.Kill();
            }
        }

        /// <summary>
        ///     Освобождаем ресурсы обработчиков событий обмена.
        /// </summary>
        private void CloseEventHandlers()
        {
            if (_exchangeEventHandlers.ContainsKey(CloseExchangeEventName))
            {
                _exchangeEventHandlers[CloseExchangeEventName].Close();
                _exchangeEventHandlers[CloseExchangeEventName].Dispose();
            }

            if (_exchangeEventHandlers.ContainsKey(OneTimeExchangeEventName))
            {
                _exchangeEventHandlers[OneTimeExchangeEventName].Close();
                _exchangeEventHandlers[OneTimeExchangeEventName].Dispose();
            }
        }

        /// <summary>
        ///     Создает или открывает обработчик события с указанным идентификатором.
        /// </summary>
        /// <param name="eventName">Идентификатор события</param>
        /// <returns>Обработчик события</returns>
        private static EventWaitHandle CreateEventByName(string eventName) => OpenEventByName(eventName)
                                                                           ?? new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

        /// <summary>
        ///     Открывает обработчик события с указанным идентификатором.
        /// </summary>
        /// <remarks>
        ///     Если обработчик события не найден - возвращается null.
        /// </remarks>
        /// <param name="eventName">Идентификатор события</param>
        /// <returns>Обработчик события</returns>
#pragma warning disable CA1416 // Проверка совместимости платформы
        private static EventWaitHandle OpenEventByName(string eventName) => EventWaitHandle.TryOpenExisting(eventName, out var existingEvent)
                                                                          ? existingEvent
                                                                          : null;
#pragma warning restore CA1416 // Проверка совместимости платформы
    }
}

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
        // Имя процесса ElpClient
        private const string ExchangeProcessName = "m_exchng";
        // Имя файла ElpClient
        private const string ExchangeProcessFileName = ExchangeProcessName + ".exe";

        // Событие, инициирующее закрытие ElpClient
        private const string CloseExchangeEventName = "CloseExchng";
        // Событие, инициирующее в ElpClient запуск обмена со шлюзом
        private const string OneTimeExchangeEventName = "OneTimeExchng";
        // Событие, сообщающее о завершении обмена со шлюзом в ElpClient
        private const string ExchangeEndEventName = "ExchngEnd";
        // Событие, сообщающее о завершении запуска ElpClient
        private const string LoadEndEventName = "LoadEnd";

        // Обработчики событий ElpClient
        private IDictionary<string, EventWaitHandle> _exchangeEventHandlers;

        public void StartExchangeProcess()
        {
            try
            {
                logger.Info("Начинаем обмен с ГП");

                // Регистрируем обработчики событий
                RegisterEventHandlers();

                // Получаем существующий или инициализируем новый процесс обмена со шлюзом.
                StartElpClient();

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

                StopElpClients();
            }
        }

        /// <summary>
        ///     Создаем обработчики событий ElpClient.
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
        ///     Запуск процесса ElpClient
        /// </summary>
        private void StartElpClient()
        {
            StopElpClients();

            //Получаем полный путь до утилиты
            var processPath = Path.Combine(exchangeOptions.AgentCatalog, ExchangeProcessFileName);
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException($"1: Утилита обмена {ExchangeProcessFileName} не найдена в каталоге {exchangeOptions.AgentCatalog}.");
            }

            logger.Info("Создаем процесс ElpClient");

            var elpClientProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processPath,
                    WorkingDirectory = exchangeOptions.AgentCatalog,
                    Arguments = $"{exchangeOptions.Agent.Login} {exchangeOptions.Agent.Password} {exchangeOptions.InFilePath} {exchangeOptions.OutFilePath}",
                }
            };

            elpClientProcess.Start();

            logger.Info("Создали и запустили процесс ElpClient.");

            logger.Info($"Ожидаем завершения процедуры запуска ElpClient {exchangeOptions.Agent.Timeout / 1000} секунд.");

            //Ожидаем в основном потоке запуска утилиты обмена (ожидание указано в файле конфигурации)
            if (_exchangeEventHandlers[LoadEndEventName].WaitOne(exchangeOptions.Agent.Timeout))
            {
                _exchangeEventHandlers[LoadEndEventName].Reset();
            }
            else
            {
                throw new TimeoutException("2: ElpClient не сообщил о завершении своего запуска.");
            }

            logger.Info("Процесс ElpClient успешно запущен.");
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
            logger.Info("Запускаем в ElpClient обмен с ГП.");

            // Сбрасываем событие завершения обмена, так как это событие может висеть
            // в установленном состоянии с предыдущей попытки запуска обмена со шлюзом.
            _exchangeEventHandlers[ExchangeEndEventName].Reset();

            // Запуск обмена
            _exchangeEventHandlers[OneTimeExchangeEventName].Set();

            // Ожидание завершения обмена
            if (_exchangeEventHandlers[ExchangeEndEventName].WaitOne(exchangeOptions.Agent.Timeout))
            {
                _exchangeEventHandlers[ExchangeEndEventName].Reset();

                logger.Info("Обмен с ГП в ElpClient успешно завершен.");
            }
            else
            {
                throw new TimeoutException("4: ElpClient не сообщил о завершении обмена с ГП.");
            }
        }

        /// <summary>
        ///     Метод останавливает все обнаруженные процессы ElpClient
        /// </summary>
        private void StopElpClients()
        {
            var elpClientProcesses = Process.GetProcesses()
                                            .Where(p => string.Equals(p.ProcessName, ExchangeProcessName,
                                                                      StringComparison.InvariantCultureIgnoreCase))
                                            .ToArray();

            foreach (var process in elpClientProcesses)
            {
                logger.Info($"Останавливаем процесс ElpClient с ИД '{process.Id}'.");

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
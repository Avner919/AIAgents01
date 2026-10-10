namespace Exchange.Options
{
    /// <summary>
    ///     Модель настроек из файла конфигурации.
    /// </summary>
    public class ExchangeOptions
    {
        /// <summary>
        ///     Имя секции файла конфигурации.
        /// </summary>
        public const string SectionName = "Exchange";

        /// <summary>
        ///     Директория исходящих файлов обмена с ГП.
        /// </summary>
        public string OutFilePath { get; set; }

        /// <summary>
        ///     Директория входящих файлов обмена с ГП.
        /// </summary>
        public string InFilePath { get; set; }

        /// <summary>
        ///     Директория, содержащая утилиту обмена с ГП.
        /// </summary>
        public string AgentCatalog { get; set; }

        /// <summary>
        ///     <see cref="AgentOptions"/>
        /// </summary>
        public AgentOptions Agent { get; set; }
    }
}
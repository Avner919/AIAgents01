namespace Exchange.Options
{
    /// <summary>
    ///     Модель настроек обмена с ГП из файла конфигурации.
    /// </summary>
    public class AgentOptions
    {
        /// <summary>
        ///     Таймаут для обмена со шлюзом.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        ///     Логин для запуска утилиты обмена со шлюзом.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        ///     Пароль для запуска утилиты обмена со шлюзом.
        /// </summary>
        public string Password { get; set; }
    }
}
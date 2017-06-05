using System;
using System.Diagnostics;

namespace InnerService
{
    /// <summary>
    /// Утилитный класс для сервиса
    /// </summary>
    class ServiceHelper
    {
        /// <summary>
        /// Метод записи сообщений в системный журнал сообщений
        /// </summary>
        /// <param name="message"></param>
        public static void LogEvent(string message)
        {
            // источник логов
            string eventSource = "Inner Service";
            DateTime dt = new DateTime();
            // текущая дата
            dt = System.DateTime.UtcNow;
            message = dt.ToLocalTime() + ": " + message;

            // Запись в журнал
            EventLog.WriteEntry(eventSource, message);
        }

    }
}

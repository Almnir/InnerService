namespace InnerService
{
    /// <summary>
    /// Класс реализации задачи процессинга
    /// </summary>
    class ProcessJob : Job
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="isRepeatable"></param>
        /// <param name="repeatTime"></param>
        public ProcessJob(bool isRepeatable, int repeatTime) : base(isRepeatable, repeatTime)
        {
        }

        /// <summary>
        /// Название задачи
        /// </summary>
        /// <returns></returns>
        public override string JobName()
        {
            return "Задача сбора и выгрузки данных рассадок";
        }

        /// <summary>
        /// Основной метод выполнения задачи
        /// </summary>
        public override void DoWork()
        {
            // если соединение с БД есть
            if (DatabaseHelper.CheckConnection())
            {
                string xmldata = DatabaseExport.GetXML();

            }
            // если нет соединения с БД
            else
            {
                ServiceHelper.LogEvent("Нет соединения с БД!");
            }
        }
    }
}

using System;
using System.IO;

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
            return "Задача сбора и выгрузки данных";
        }

        /// <summary>
        /// Основной метод выполнения задачи
        /// </summary>
        public override void DoWork()
        {
            // если соединение с БД есть
            if (DatabaseHelper.CheckConnection())
            {
                // выгрузка в файл во временный каталог
                DatabaseHelper.WriteXML();
                // получение доступа к разделяемому каталогу удалённого сервера открытой сети
                using (NetworkShareAccesser.Access(Globals.RemoteServer, Globals.DomainName, Globals.UserName, Globals.Password))
                {
                    try
                    {
                        // копирование файла с данными управления видеонаблюдением на сервер открытой сети с перезаписью
                        File.Copy(Globals.GetTempFilePath(), @"\\OPENSERVER\Shared\auditoriums.xml", true);
                    }
                    catch (Exception)
                    {
                        ServiceHelper.LogEvent("Ошибка при копировании файла auditoriums.xml!");
                    }
                }
            }
            // если нет соединения с БД
            else
            {
                ServiceHelper.LogEvent("Нет соединения с БД!");
            }
        }
    }
}

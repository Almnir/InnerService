using System;
using System.Threading;

namespace InnerService
{
    /// <summary>
    /// Абстрактный класс задачи
    /// </summary>
    public abstract class Job
    {
        // признак повторяющейся задачи
        private bool IsRepeatable { get; set; }
        // интервал повторения
        private int RepeatTime { get; set; }
        // объект синхронизации
        private object threadLock = new object();

        /// <summary>
        /// Конструктор задачи
        /// </summary>
        /// <param name="isRepeatable"></param>
        /// <param name="repeatTime"></param>
        protected Job(bool isRepeatable, int repeatTime)
        {
            this.IsRepeatable = isRepeatable;
            this.RepeatTime = repeatTime;
        }

        /// <summary>
        /// Метод выполнения работы
        /// </summary>
        /// <param name="obj">объект токена отмены</param>
        public void ExecuteJob(object obj)
        {
            // токен отмены
            CancellationToken token = (CancellationToken)obj;
            // блок синхронизирован
            lock (threadLock)
            {
                try
                {
                    do
                    {
                        ServiceHelper.LogEvent(string.Format("{0} запущена...", JobName()));
                        DoWork();
                        token.ThrowIfCancellationRequested();
                        token.WaitHandle.WaitOne(RepeatTime);
                        token.ThrowIfCancellationRequested();
                    }
                    while (IsRepeatable);
                    ServiceHelper.LogEvent(string.Format("{0} завершила работу...", JobName()));
                }
                catch (OperationCanceledException)
                {
                    ServiceHelper.LogEvent(string.Format("{0} была остановлена...", JobName()));
                }
            }
        }

        public abstract void DoWork();
        public abstract string JobName();
    }
}

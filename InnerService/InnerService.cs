using System.ServiceProcess;
using System.Threading;

namespace InnerService
{
    class InnerService : ServiceBase
    {
        // Интервал работы сервиса
        const int INTERVAL = 1000 * 60 * 40;
        // токен отмены
        CancellationToken token;
        // сигнал токена отмены
        CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        public InnerService()
        {
            // название сервиса
            this.ServiceName = "Inner Service";
            // уровень логирования
            this.EventLog.Log = "Application";

            // флаги возможностей сервиса
            this.CanHandlePowerEvent = false;
            this.CanHandleSessionChangeEvent = false;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;

            // инициализация токена
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
        }

        /// <summary>
        /// Точка входа для запуска сервиса
        /// </summary>
        static void Main()
        {
            ServiceBase.Run(new InnerService());
        }

        /// <summary>
        /// Деструктор(финализатор) сервиса
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Обработчик старта сервиса
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            // создать новую задачу с повторением(да) и интервалом повторения(INTERVAL)
            ProcessJob processJob = new ProcessJob(true, INTERVAL);
            // новый поток выполнения задачи
            Thread thread = new Thread(new ParameterizedThreadStart(processJob.ExecuteJob));
            // запуск нового потока с передачей токена отмены
            thread.Start(token);
        }

        /// <summary>
        /// Обработчик останова сервиса
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
        }

        /// <summary>
        /// Обработчик завершения сервиса
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();
        }
    }
}
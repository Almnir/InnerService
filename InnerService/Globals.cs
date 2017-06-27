using System.IO;

namespace InnerService
{
    class Globals
    {
        // сервер БД
        private static string ServerText = "MAINSERVER";
        // название БД
        private static string DatabaseText = "MAINDB";
        // логин
        private static string LoginText = "innerservice";
        // пароль
        private static string PasswordText = "Innerpassword";

        // сервер открытой сети
        public static string RemoteServer = "OPENSERVER";
        // домен
        public static string DomainName = "FTC_DOMAIN";
        // имя
        public static string UserName = "PSERVER";
        // пароль
        public static string Password = "PaSsWoRd";

        /// <summary>
        /// Метод, возвращающий строку соединения с базой данных
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return string.Format("Server={0};Database={1};User Id={2};Password={3};", ServerText, DatabaseText, LoginText, PasswordText);
        }

        /// <summary>
        /// Метод, возвращающий путь к файлу во временном каталоге
        /// </summary>
        /// <returns></returns>
        public static string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), "auditoriums.xml");
        }
    }
}

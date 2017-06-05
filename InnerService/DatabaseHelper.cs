using System;
using System.Data.SqlClient;

namespace InnerService
{
    /// <summary>
    /// Вспомогательный класс для работы с БД
    /// </summary>
    class DatabaseHelper
    {
        /// <summary>
        /// Проверить соединение с БД
        /// </summary>
        /// <returns></returns>
        public static bool CheckConnection()
        {
            // переменная результата
            bool result = false;
            try
            {
                // соединение
                using (var connection = new SqlConnection(Globals.GetConnectionString()))
                {
                    // фиктивный запрос
                    var query = "select 1";
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    command.ExecuteScalar();
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

    }
}

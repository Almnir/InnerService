namespace InnerService
{
    class Globals
    {
        private static string ServerText = "MAINSERVER";
        private static string DatabaseText = "MAINDB";
        private static string LoginText = "innerservice";
        private static string PasswordText = "Innerpassword";

        public static string GetConnectionString()
        {
            return string.Format("Server={0};Database={1};User Id={2};Password={3};", ServerText, DatabaseText, LoginText, PasswordText);
        }
    }
}

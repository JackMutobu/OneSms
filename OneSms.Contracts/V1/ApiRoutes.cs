namespace OneSms.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";

        public const string Version = "v1";

        public const string Base = Root + "/" + Version;

        public static class Auth
        {
            public const string Controller = "/auth";

            public const string App = Base + Controller + "/app";

            public const string User = Base + Controller + "/user";

            public const string Server = Base + Controller + "/server";
        }

        public static class Message
        {
            public const string Controller = "/messages";
            public const string Send = Base + Controller + "/send";
            public const string GetAll = Base + Controller;
            public const string GetAllByTransactionId = Base + Controller + "/transaction/{transactionId}";
            public const string GetAllByAppId = Base + Controller + "/app/{appId}";
        }

        public static class Sms
        {
            public const string Controller = "/sms";
            public const string Send = Base + Controller + "/send";
            public const string GetAll = Base + Controller;
            public const string GetAllByTransactionId = Base + Controller + "/transaction/{transactionId}";
            public const string GetAllByAppId = Base + Controller + "/app/{appId}";
            public const string SmsReceived = Base + Controller + "/received";
            public const string StatusChanged = Base + Controller + "/status";
        }

        public static class Whatsapp
        {
            public const string Controller = "/whatsapp";
            public const string Send = Base + Controller + "/send";
            public const string GetAll = Base + Controller;
            public const string GetAllByTransactionId = Base + Controller + "/transaction/{transactionId}";
            public const string GetAllByAppId = Base + Controller + "/app/{appId}";
            public const string WhatsappReceived = Base + Controller + "/received";
            public const string StatusChanged = Base + Controller + "/status";
            public const string NumberNotFound = Base + Controller + "/numbernotfound";
        }

        public static class Upload
        {
            public const string Controller = "/upload";

            public const string Image = Base + Controller + "/image";
        }
    }
}

namespace OneSms.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";

        public const string Version = "v1";

        public const string Base = Root + "/" + Version;

        public static class Posts
        {
            public const string GetAll = Base + "/posts";

            public const string Update = Base + "/posts/{postId}";

            public const string Delete = Base + "/posts/{postId}";

            public const string Get = Base + "/posts/{postId}";

            public const string Create = Base + "/posts";
        }

        public static class Auth
        {
            public const string App = Base + "/auth/app";

            public const string User = Base + "/auth/user";

            public const string Server = Base + "/auth/server";
        }

        public static class Message
        {
            public const string Send = Base + "/messages/send";
            public const string GetAllByTransactionId = Base + "/messages/{transactionId}";
        }

        public static class Sms
        {
            public const string Send = Base + "/sms/send";
            public const string GetAllByTransactionId = Base + "/sms/{transactionId}";
        }

        public static class Whatsapp
        {
            public const string Send = Base + "/whatsapp/send";
            public const string GetAllByTransactionId = Base + "/whatsapp/{transactionId}";
        }
    }
}

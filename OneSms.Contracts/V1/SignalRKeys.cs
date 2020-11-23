﻿namespace OneSms.Contracts.V1
{
    public class SignalRKeys
    {
        public const string SendSms = nameof(SendSms);
        public const string ReceiveSms = nameof(ReceiveSms);
        public const string SendUssd = nameof(SendUssd);

        public const string SmsSent = nameof(SmsSent);
        public const string CancelUssdSession = nameof(CancelUssdSession);

        public const string SendWhatsapp = nameof(SendWhatsapp);
        public const string ResetToActive = nameof(ResetToActive);
        public const string CheckClientAlive = nameof(CheckClientAlive);

        public const string ShareContact = nameof(ShareContact);

        public const string OnException = nameof(OnException);

        public const string OnMessageReceived = nameof(OnMessageReceived);
        public const string OnMessageSentStatusChanged = nameof(OnMessageSentStatusChanged);
        public const string Ping = nameof(Ping);
    }
}

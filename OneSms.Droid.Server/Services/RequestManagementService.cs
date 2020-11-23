using OneSms.Contracts.V1;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Droid.Server.Enumerations;
using Priority_Queue;
using Splat;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.SignalR.Client;
using OneSms.Contracts.V1.Dtos;
using System;

namespace OneSms.Droid.Server.Services
{
    public interface IRequestManagementService
    {
        TransactionExecution CurrentTransaction { get; }
        SimplePriorityQueue<TransactionExecution> NextTransactions { get; }
        Subject<object> OnTransactionCompleted { get; }
        Subject<WhastappMessageReceived> OnWhatsappMessageReceived { get; }
    }
    public class RequestManagementService : IRequestManagementService
    {
        private IWhatsappService _whatsappService;
        private ISmsService _smsService;
        private IUssdService _ussdService;
        private ISignalRService _signalRService;

        public RequestManagementService()
        {
            _whatsappService = Locator.Current.GetService<IWhatsappService>();
            _smsService = Locator.Current.GetService<ISmsService>();
            _ussdService = Locator.Current.GetService<IUssdService>();
            _signalRService = Locator.Current.GetService<ISignalRService>();
            CurrentTransaction = TransactionExecution.SmsWhatsapp;

            _signalRService
               .Connection
               .On<WhatsappRequest>(SignalRKeys.SendWhatsapp, async transaction => 
                    await _whatsappService.Execute(transaction));
            _signalRService
                .Connection
                .On<ShareContactRequest>(SignalRKeys.ShareContact, async contact => 
                await _whatsappService.Execute(contact));

            OnWhatsappMessageReceived = new Subject<WhastappMessageReceived>();
            OnWhatsappMessageReceived
                .Subscribe(async request => 
                await _whatsappService.Execute(request));

            OnTransactionCompleted = new Subject<object>();
            OnTransactionCompleted.Subscribe(result =>
            {
                switch(result)
                {
                    case BaseMessageRequest request:
                        _whatsappService.OnRequestCompleted.OnNext(request);
                        break;
                    case UssdRequest ussdRequest:
                        if (((int)CurrentTransaction) < 3)
                            _whatsappService.OnRequestCompleted.OnNext(true);
                        _ussdService.OnUssdCompleted.OnNext(ussdRequest);
                        break;
                    default:
                        _whatsappService.OnRequestCompleted.OnNext(true);
                        break;
                }
            });


            NextTransactions = new SimplePriorityQueue<TransactionExecution>();
        }

        public TransactionExecution CurrentTransaction { get;  }

        public SimplePriorityQueue<TransactionExecution> NextTransactions { get; private set; }

        public Subject<object> OnTransactionCompleted { get; }

        public Subject<WhastappMessageReceived> OnWhatsappMessageReceived { get; }
    }
}
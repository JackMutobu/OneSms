using Android.Content;
using OneUssd;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Splat;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.Requests;

namespace OneSms.Droid.Server.Services
{
    public interface IUssdService
    {
        bool IsBusy { get; }
        Subject<UssdRequest> OnUssdCompleted { get; }
        Subject<UssdRequest> OnUssdStarted { get; }
        Subject<UssdRequest> OnUssdReceived { get; }
        UssdRequest UssdRequest { get; }

        void Execute(string ussdNumber, int sim, Dictionary<string, HashSet<string>> keyMpas, List<string> inputData);
        Task SendUssdStateChanged(UssdApiRequest ussd);
    }

    public class UssdService : IUssdService
    {
        private UssdController _ussdController;
        private ISignalRService _signalRService;
        private IHttpClientService _httpClientService;
        private Queue<string> _inputData;
        private Queue<UssdRequest> _pendingUssdTransactions;
        private Subject<Unit> _ussdTimer;
        private string _lastResponse;
        private Context _context;

        public UssdService(Context context, IWhatsappService whatsappService)
        {
            _context = context;
            _ussdController = UssdController.GetInstance(context);
            _pendingUssdTransactions = new Queue<UssdRequest>();
            _ussdTimer = new Subject<Unit>();
            OnUssdCompleted = new Subject<UssdRequest>();
            OnUssdStarted = new Subject<UssdRequest>();
            OnUssdReceived = new Subject<UssdRequest>();

            _signalRService = Locator.Current.GetService<ISignalRService>();
            _httpClientService = Locator.Current.GetService<IHttpClientService>();

            _signalRService
                .Connection
                .On(SignalRKeys.SendUssd, (Action<UssdRequest>)(ussd =>
                {
                    OnUssdReceived.OnNext(ussd);
                    if (ussd != null && UssdRequest == null && _pendingUssdTransactions.Count == 0)
                        Execute(ussd);
                    else
                        _pendingUssdTransactions.Enqueue(ussd);
                }));

            _signalRService
                .Connection
                .On(SignalRKeys.CancelUssdSession, () => CancelSession());

            _ussdTimer.Select(_ => Observable.Timer(TimeSpan.FromSeconds(30)))
                .Switch()
                .Subscribe(_ => CancelSession());


        }
        public UssdRequest UssdRequest { get; private set; }

        public Subject<UssdRequest> OnUssdCompleted { get; }

        public Subject<UssdRequest> OnUssdStarted { get; }

        public Subject<UssdRequest> OnUssdReceived { get; }

        public bool IsBusy => UssdRequest != null && _pendingUssdTransactions.Count > 0;

        private void Execute(UssdRequest ussd)
        {
            var map = new Dictionary<string, HashSet<string>>
                     {
                         { UssdController.KeyError, new HashSet<string>(ussd.KeyProblems) },
                         { UssdController.KeyLogin, new HashSet<string>(ussd.KeyWelcomes) }
                     };
            UssdRequest = ussd;
            Execute(ussd.UssdNumber, ussd.SimSlot, map, ussd.UssdInputs);
            OnUssdStarted.OnNext(ussd);
            SendTransactionState("Started", UssdTransactionState.Executing);
            _ussdTimer.OnNext(Unit.Default);
        }

        public void Execute(string ussdNumber, int sim, Dictionary<string, HashSet<string>> keyMpas, List<string> inputData)
        {
            RegisterEvents();
            _inputData = new Queue<string>(inputData);
            _ussdController.CallUSSDOverlayInvoke(ussdNumber, sim, keyMpas);
        }

        private void RegisterEvents()
        {
            _ussdController.ResponseRecieved += OnResponseRecieved;
            _ussdController.SessionAborted += OnSessionAborted;
            _ussdController.SessionCompleted += OnSessionCompleted;
        }

        private void UnRegisterEvents()
        {
            _ussdController.ResponseRecieved -= OnResponseRecieved;
            _ussdController.SessionAborted -= OnSessionAborted;
            _ussdController.SessionCompleted -= OnSessionCompleted;
        }

        private void OnSessionCompleted(object sender, UssdEventArgs e)
        {
            SendTransactionState(e.ResponseMessage, UssdTransactionState.Completed);
            UnRegisterEvents();
            OnUssdCompleted.OnNext(UssdRequest);
            ExecuteNextTransactionOrReset();
        }

        private void OnSessionAborted(object sender, UssdEventArgs e)
        {
            SendTransactionState(_lastResponse, UssdTransactionState.Aborted);
            UnRegisterEvents();
            OnUssdCompleted.OnNext(UssdRequest);
            ExecuteNextTransactionOrReset();
        }

        private void CancelSession()
        {
            if (_ussdController.IsRunning)
            {
                var lastMessage = _ussdController.StopOperation();
                SendTransactionState(lastMessage, UssdTransactionState.Aborted);
            }
            MainActivity.GoToHomeScreen(_context);
        }

        private void OnResponseRecieved(object sender, UssdEventArgs e)
        {
            if (_inputData.Count != 0)
            {
                var input = _inputData.Dequeue();
                _ussdController.SendData(input);
                _lastResponse = e.ResponseMessage;
            }
            _ussdTimer.OnNext(Unit.Default);//reset timer
        }

        private void ExecuteNextTransactionOrReset()
        {
            if (_pendingUssdTransactions.Count > 0)
                Execute(_pendingUssdTransactions.Dequeue());
            else
                UssdRequest = null;
        }

        private void SendTransactionState(string lastResponse, UssdTransactionState transactionState)
        {
            if (UssdRequest != null)
            {
                var transactionReport = new UssdApiRequest
                {
                    LastMessage = lastResponse,
                    MobileServerId = UssdRequest.MobileServerId,
                    NetworkAction = UssdRequest.NetworkAction,
                    SimId = UssdRequest.SimId,
                    SimSlot = UssdRequest.SimSlot,
                    TransactionState = transactionState,
                    TransactionId = UssdRequest.TransactionId,
                    UssdId = UssdRequest.UssdId
                };
                SendUssdStateChanged(transactionReport);
            }
        }

        public Task SendUssdStateChanged(UssdApiRequest ussd) => _httpClientService.PutAsync<string>(ussd, ApiRoutes.Ussd.StatusChanged);

    }
}
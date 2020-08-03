using Android.Content;
using Android.Widget;
using OneSms.Web.Shared.Constants;
using OneSms.Web.Shared.Dtos;
using OneUssd;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using OneSms.Web.Shared.Enumerations;

namespace OneSms.Droid.Server.Services
{
    public class UssdService
    {
        private Context _context;
        private UssdController _ussdController;
        private SignalRService _signalRService;
        private HttpClientService _httpClientService;
        private Queue<string> _inputData;
        private string _currentInput;
        private UssdTransactionDto _currentTransactionDto;
        private Queue<UssdTransactionDto> _pendingUssdTransactions;
        
        public UssdService(Context context,SignalRService signalRService,HttpClientService httpClientService)
        {
            _context = context;
            _ussdController = UssdController.GetInstance(context);
            _pendingUssdTransactions = new Queue<UssdTransactionDto>();
            _signalRService = signalRService;
            _httpClientService = httpClientService;

            _signalRService.Connection.On(SignalRKeys.SendUssd, (Action<UssdTransactionDto>)(ussd =>
             {
                 if (_currentTransactionDto == null && _pendingUssdTransactions.Count == 0)
                     Execute(ussd);
                 else
                     _pendingUssdTransactions.Enqueue(ussd);
             }));
        }

        private void Execute(UssdTransactionDto ussd)
        {
            var map = new Dictionary<string, HashSet<string>>
                     {
                         { UssdController.KeyError, new HashSet<string>(ussd.KeyProblems) },
                         { UssdController.KeyLogin, new HashSet<string>(ussd.KeyWelcomes) }
                     };
            _currentTransactionDto = ussd;
            Execute(ussd.UssdNumber, ussd.SimSlot, map, ussd.UssdInputs);
        }

        public void Execute(string ussdNumber,int sim,Dictionary<string,HashSet<string>> keyMpas,List<string> inputData)
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
            SendTransactionState(e, UssdTransactionState.Done);
            UnRegisterEvents();
            if (_pendingUssdTransactions.Count > 0)
                Execute(_pendingUssdTransactions.Dequeue());
            else
                _currentTransactionDto = null;
        }

        private void SendTransactionState(UssdEventArgs e,UssdTransactionState transactionState)
        {
            if (_currentTransactionDto != null)
            {
                _currentTransactionDto.TimeStamp = DateTime.UtcNow;
                _currentTransactionDto.TransactionState = transactionState;
                _currentTransactionDto.LastMessage = e.ResponseMessage;
                SendUssdStateChanged(_currentTransactionDto);
            }
        }

        private void OnSessionAborted(object sender, UssdEventArgs e)
        {
            SendTransactionState(e, UssdTransactionState.Canceled);
            UnRegisterEvents();
            if (_pendingUssdTransactions.Count > 0)
                Execute(_pendingUssdTransactions.Dequeue());
            else
                _currentTransactionDto = null;

        }

        private void OnResponseRecieved(object sender, UssdEventArgs e)
        {
            if(_inputData.Count != 0)
            {
                var input = _inputData.Dequeue();
                _ussdController.SendData(input);
                _currentInput = input;
            }
            SendTransactionState(e, UssdTransactionState.Executing);
        }

        public Task SendUssdStateChanged(UssdTransactionDto ussd) => _httpClientService.PutAsync<string>(ussd, "Ussd/StatusChanged");

    }
}
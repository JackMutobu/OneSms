using Android.Content;
using Android.Widget;
using OneUssd;
using System.Collections.Generic;

namespace OneSms.Droid.Server.Services
{
    public class UssdService
    {
        private Context _context;
        private UssdController _ussdController;
        private Queue<string> _inputData;
        private string _currentInput;
        public UssdService(Context context)
        {
            _context = context;
            _ussdController = UssdController.GetInstance(context); 
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
            UnRegisterEvents();
            Toast.MakeText(_context, e.ResponseMessage, ToastLength.Long).Show();
        }

        private void OnSessionAborted(object sender, UssdEventArgs e)
        {
            UnRegisterEvents();
            Toast.MakeText(_context, e.ResponseMessage, ToastLength.Long).Show();
        }

        private void OnResponseRecieved(object sender, UssdEventArgs e)
        {
            if(_inputData.Count != 0)
            {
                var input = _inputData.Dequeue();
                _ussdController.SendData(input);
                _currentInput = input;
            }
        }


    }
}
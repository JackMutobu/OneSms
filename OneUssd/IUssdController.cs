using Android.Content;
using System;
using System.Collections.Generic;

namespace OneUssd
{
    public interface IUssdController
    {
        public  event EventHandler<UssdEventArgs> ResponseRecieved;
        public  event EventHandler<UssdEventArgs> SessionCompleted;
        public  event EventHandler<UssdEventArgs> SessionAborted;

        Context Context { get; }
        bool IsRunning { get; set; }
        Dictionary<string, HashSet<string>> Map { get; }

        void CallUSSDInvoke(string ussdPhoneNumber, int simSlot, Dictionary<string, HashSet<string>> map);
        void CallUSSDOverlayInvoke(string ussdPhoneNumber, int simSlot, Dictionary<string, HashSet<string>> map);
        void SendData(string text);
    }
}
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using OneUssd;
using System.Collections.Generic;

namespace OneSms
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        TextView textMessage;
        Button btnExecute;
        EditText editText;
        HashSet<string> keyLogins = new HashSet<string>();
        HashSet<string> keyProblems = new HashSet<string>();
        Dictionary<string, HashSet<string>> map = new Dictionary<string, HashSet<string>>();
        private IUssdController _ussdController;
        Queue<string> _data = new Queue<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            btnExecute = FindViewById<Button>(Resource.Id.btnExecute);
            editText = FindViewById<EditText>(Resource.Id.txtUssd);
            
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            btnExecute.Click += BtnExecute_Click;
            keyProblems.Add("enjoy");
            map.Add(UssdController.KeyError, keyProblems);
            map.Add(UssdController.KeyLogin, keyLogins);
            _ussdController = UssdController.GetInstance(this);
            _ussdController.ResponseRecieved += OnResponseRecieved;
            _ussdController.SessionAborted += OnSessionAborted;
            _ussdController.SessionCompleted += OnSessionCompleted;
            _data.Enqueue("*");
            _data.Enqueue("12");
        }

        private void OnResponseRecieved(object sender, UssdEventArgs e)
        {
            _ussdController.SendData(_data.Dequeue());
            Toast.MakeText(this, e.ResponseMessage, ToastLength.Short).Show();
        }
        private void OnSessionAborted(object sender, UssdEventArgs e)
        {
            Toast.MakeText(this, e.ResponseMessage, ToastLength.Short).Show();
        }
        private void OnSessionCompleted(object sender, UssdEventArgs e)
        {
            Toast.MakeText(this, "Completed " + e.ResponseMessage, ToastLength.Short).Show();
        }

        private void BtnExecute_Click(object sender, System.EventArgs e)
        {
            if(!string.IsNullOrEmpty(editText.Text))
            {
                _ussdController.CallUSSDInvoke(editText.Text, 0, map);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    textMessage.SetText(Resource.String.title_home);
                    return true;
                case Resource.Id.navigation_dashboard:
                    textMessage.SetText(Resource.String.title_dashboard);
                    return true;
                case Resource.Id.navigation_notifications:
                    textMessage.SetText(Resource.String.title_notifications);
                    return true;
            }
            return false;
        }
    }
}


using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using OneSms.Droid.Server.Adapters;
using OneSms.Droid.Server.Constants;
using OneSms.Droid.Server.Receivers;
using OneSms.Droid.Server.ViewModels;
using OneSms.Droid.Server.Views;
using Syncfusion.Android.TabView;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace OneSms.Droid.Server
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private SfTabView tabView;
        private FrameLayout allContactsGrid;
        private SettingsView settingsView;
        private ContactViewModel contactViewModel;
        private SmsReceiver smsReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk2NzM0QDMxMzgyZTMyMmUzMENBcnhhYldQMkZMbGorVlI4OXhBWUlYOFk1RVV6K0cvNHI2UFFvUGsyVHc9");
            tabView = new SfTabView(this.ApplicationContext);
            smsReceiver = new SmsReceiver();
            CheckForIncommingSMS();
            contactViewModel = new ContactViewModel();
            settingsView = new SettingsView(this);
            SetContentView(tabView);
            InitializeTab();
            var listView = new ListView(this);
            TabContentListAdapter tabContentListAdapter = new TabContentListAdapter(this,contactViewModel.ContactList);
            listView.Adapter = tabContentListAdapter;
            allContactsGrid.AddView(listView);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        public void InitializeTab()
        {
            allContactsGrid = new FrameLayout(ApplicationContext);
            var contactsGrid = new FrameLayout(ApplicationContext);
            allContactsGrid.SetBackgroundColor(Color.Red);
            contactsGrid.SetBackgroundColor(Color.Blue);
            var tabItems = new TabItemCollection
                {
                    new SfTabItem()
                    {
                        Title = "Calls",
                        Content = allContactsGrid
                    },
                    new SfTabItem()
                    {
                        Title = "Contacts",
                        Content = contactsGrid
                    },
                     new SfTabItem()
                    {
                        Title = "Settings",
                        Content = settingsView
                    }
                };
            tabView.Items = tabItems;
            tabView.VisibleHeaderCount = 3;
            tabView.ContentTransitionDuration = 200;
            tabView.EnableSwiping = true;
        }
        private void CheckForIncommingSMS()
        {
            System.Diagnostics.Debug.WriteLine("Sms Receiver registered");
            var intent = new IntentFilter();
            intent.AddAction(OneSmsAction.SmsSent);
            intent.AddAction(OneSmsAction.SmsDelivered);
            this.RegisterReceiver(smsReceiver, intent);
        }

    }
}


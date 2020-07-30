using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using OneSms.Droid.Server.Models;
using System.Collections.ObjectModel;

namespace OneSms.Droid.Server.Adapters
{
    public class TabContentListAdapter : BaseAdapter
    {
        private ObservableCollection<ContactInfo> _contacts;
        private Context _context;

        public override int Count => 7;

        public TabContentListAdapter(Context context,ObservableCollection<ContactInfo> contacts)
        {
            _contacts = contacts;
            _context = context;
        }
        public override Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return -1;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var mainlayout = new LinearLayout(_context)
            {
                Orientation = Orientation.Vertical
            };
            TextView text = new TextView(_context);
            text.SetBackgroundColor(Color.Transparent);
            text.Text = _contacts[position].Name;
            text.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            TextView text1 = new TextView(_context)
            {
                Text = _contacts[position].Number.ToString(),
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            text1.SetBackgroundColor(Color.Transparent);
            mainlayout.AddView(text);
            mainlayout.AddView(text1);
            mainlayout.SetMinimumHeight(200);
            return mainlayout;
        }
    }
}
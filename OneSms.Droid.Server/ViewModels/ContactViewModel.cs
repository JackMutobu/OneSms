using OneSms.Droid.Server.Models;
using System.Collections.ObjectModel;

namespace OneSms.Droid.Server.ViewModels
{
    public class ContactViewModel:ViewModelBase
    {
        public ContactViewModel()
        {
            ContactList = new ObservableCollection<ContactInfo>
            {
                new ContactInfo { Name = "Aaron", Number = 7363750 },
                new ContactInfo { Name = "Adam", Number = 7323250 },
                new ContactInfo { Name = "Adrian", Number = 7239121 },
                new ContactInfo { Name = "Alwin", Number = 2329823 },
                new ContactInfo { Name = "Alex", Number = 8013481 },
                new ContactInfo { Name = "Alexander", Number = 7872329 },
                new ContactInfo { Name = "Barry", Number = 7317750 }
            };
        }

 
        public ObservableCollection<ContactInfo> ContactList { get; set; }
    }

   
}
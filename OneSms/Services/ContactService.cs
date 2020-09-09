using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneSms.Data;
using OneSms.Domain;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IContactService
    {
        Task<string> AddContactToApp(string appId, string number);
        bool IsAlreadyContacted(string appId, string number);
    }

    public class ContactService : IContactService
    {
        private DataContext _dbContext;
        public ContactService(DataContext context)
        {
            _dbContext = context;
        }

        public bool IsAlreadyContacted(string appId, string number)
        {
            var contact = _dbContext.Contacts.SingleOrDefault(x => x.Mobile == number || x.Phone == number);
            if (contact != null)
                return _dbContext.AppContacts.Any(x => x.AppId.ToString() == appId && x.ContactId == contact.Id);
            return false;
        }

        public async Task<string> AddContactToApp(string appId, string number)
        {
            try
            {
                var contact = _dbContext.Contacts.SingleOrDefault(x => x.Mobile == number || x.Phone == number);
                var app = _dbContext.Apps.Include(x => x.Sims).ThenInclude(x => x.Sim).SingleOrDefault(x => x.Id.ToString() == appId);

                if (app != null)
                {
                    var appContact = new Contact
                    {
                        FirstName = app.Organization,
                        Mobile = app.Sims.FirstOrDefault()?.Sim?.Number,
                        Phone = app.Sims.LastOrDefault()?.Sim?.Number,
                        Organization = app.Organization
                    };

                    if (contact != null)
                    {
                        _dbContext.AppContacts.Add(new AppContact
                        {
                            AppId = app.Id,
                            ContactId = contact.Id
                        });

                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        var newContact = new Contact
                        {
                            Mobile = number,
                            Apps = new Collection<AppContact>
                            {
                                new AppContact
                                {
                                    AppId = app.Id
                                }
                            }
                        };
                        _dbContext.Contacts.Add(newContact);
                        await _dbContext.SaveChangesAsync();
                    }

                    return appContact.ToString();
                }
            }
            catch(Exception ex)
            {
                var mess = ex.Message;
            }
            return string.Empty;
        }
    }
}

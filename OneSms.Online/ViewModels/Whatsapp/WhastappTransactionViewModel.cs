using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Services;
using OneSms.Web.Shared.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace OneSms.Online.ViewModels.Whatsapp
{
    public class WhastappTransactionViewModel:ReactiveObject
    {
        private OneSmsDbContext _oneSmsDbContext;

        public WhastappTransactionViewModel(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;

            Transactions = new ObservableCollection<WhatsappTransaction>();
            LoadTransactions = ReactiveCommand.CreateFromTask(() => _oneSmsDbContext.WhatsappTransactions.Include(x => x.MobileServer).OrderByDescending(x => x.CompletedTime).ToListAsync());
            LoadTransactions.Do(transaction => Transactions = new ObservableCollection<WhatsappTransaction>(transaction)).Subscribe();

            DeleteTransaction = ReactiveCommand.CreateFromTask<WhatsappTransaction, int>(whatsapp =>
            {
                _oneSmsDbContext.Remove(whatsapp);
                return _oneSmsDbContext.SaveChangesAsync();
            });
            DeleteTransaction.Select(_ => Unit.Default).InvokeCommand(LoadTransactions);
        }

        [Reactive]
        public ObservableCollection<WhatsappTransaction> Transactions { get; set; }

        public ReactiveCommand<Unit, List<WhatsappTransaction>> LoadTransactions { get; }

        public ReactiveCommand<WhatsappTransaction, int> DeleteTransaction { get; }
    }
}

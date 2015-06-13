using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Controls
{
    public class HistoryViewModel<T> : ViewModelBase where T : class
    {

        History<T> history;

        public HistoryViewModel(History<T> history)
        {
            Contract.Requires(history != null);
            this.history = history;
        }

        public RelayCommand NextCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    history.MoveForward();
                    OnPropertyChanged(() => Current);
                }, () => !history.CurrentIsLast);
            }
        }

        public RelayCommand PrevCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    history.MoveBack();
                    OnPropertyChanged(() => Current);
                }, () => !history.CurrentIsFirst);
            }
        }
        public T Current
        {
            get
            {
                return history.CurrentState;
            }
        }

        public void Memo(T opt)
        {
            history.Memorize(opt);
        }
    }
}
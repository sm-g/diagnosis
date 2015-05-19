using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class SearchHistoryViewModel : ViewModelBase
    {

        History<SearchOptions> history;

        public SearchHistoryViewModel(History<SearchOptions> history)
        {
            Contract.Requires(history != null);
            this.history = history;
        }

        public RelayCommand NextOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    history.MoveForward();
                    OnPropertyChanged(() => CurrentOptions);
                }, () => !history.CurrentIsLast);
            }
        }

        public RelayCommand PrevOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    history.MoveBack();
                    OnPropertyChanged(() => CurrentOptions);
                }, () => !history.CurrentIsFirst);
            }
        }
        public SearchOptions CurrentOptions
        {
            get
            {
                return history.CurrentState;
            }
        }

        public void Memo(SearchOptions opt)
        {
            history.Memorize(opt);
        }
    }
}
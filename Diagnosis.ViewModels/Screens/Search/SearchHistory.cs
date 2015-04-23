using Diagnosis.Common;
using Diagnosis.ViewModels.Screens;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Search
{
    public class SearchHistory : ViewModelBase
    {
        private SearchOptions _currentOptions;
        private int currnetPos;

        public SearchHistory()
        {
            History = new ObservableCollection<SearchOptions>();
        }

        public RelayCommand NextOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    currnetPos++;
                    OnPropertyChanged(() => CurrentOptions);
                }, () => currnetPos < History.Count - 1);
            }
        }

        public RelayCommand PrevOptionsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    currnetPos--;
                    OnPropertyChanged(() => CurrentOptions);
                }, () => currnetPos > 0);
            }
        }

        ObservableCollection<SearchOptions> History { get; set; }

        public SearchOptions CurrentOptions
        {
            get
            {
                return History[currnetPos];
            }
        }

        public void Memorize(SearchOptions opt)
        {
            var ind = History.IndexOf(opt);
            if (ind >= 0)
            {
                currnetPos = ind;
            }
            else
            {
                History.Add(opt);
                currnetPos = History.Count - 1;
            }
            OnPropertyChanged(() => CurrentOptions);
        }
    }
}
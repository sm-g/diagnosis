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
        private HrSearchOptions _currentOptions;
        private int currnetPos;

        public SearchHistory()
        {
            History = new ObservableCollection<HrSearchOptions>();
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

        public ObservableCollection<HrSearchOptions> History { get; private set; }

        public HrSearchOptions CurrentOptions
        {
            get
            {
                return History[currnetPos];
            }
        }

        public void AddOptions(HrSearchOptions opt)
        {
            if (History.Contains(opt))
                return;

            History.Add(opt);
            currnetPos = History.Count - 1;
            OnPropertyChanged(() => CurrentOptions);
        }
    }
}
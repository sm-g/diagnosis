using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class SymptomExplorerViewModel : ViewModelBase
    {
        private RelayCommand<SymptomViewModel> _clickItem;
        private SymptomViewModel _current;
        private RelayCommand _goUp;
        private List<SymptomViewModel> symptoms;

        public SymptomExplorerViewModel(List<SymptomViewModel> ss)
        {
            Contract.Requires(ss != null);
            symptoms = ss;

            Symptoms = new ObservableCollection<SymptomViewModel>(symptoms);
        }

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        public SymptomViewModel CurrentSymptom
        {
            get
            {
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;

                    Symptoms = new ObservableCollection<SymptomViewModel>(_current.Children);
                    OnPropertyChanged(() => CurrentSymptom);
                    OnPropertyChanged(() => Symptoms);

                    CreateBreadcrumbs();
                }
            }
        }

        private void CreateBreadcrumbs()
        {
            Breadcrumbs = new ObservableCollection<SymptomViewModel>();
            SymptomViewModel s = CurrentSymptom;
            do
            {
                Breadcrumbs.Add(s);
                s = s.Parent;
            }
            while (s != null);
            Breadcrumbs.Reverse();

            OnPropertyChanged(() => Breadcrumbs);
        }

        public ObservableCollection<SymptomViewModel> Breadcrumbs
        {
            get;
            private set;
        }

        public RelayCommand GoUp
        {
            get
            {
                return _goUp ?? (_goUp = new RelayCommand(
                        () => CurrentSymptom = CurrentSymptom.Parent,
                        () => CurrentSymptom != null && !CurrentSymptom.IsRoot
                        ));
            }
        }

        public RelayCommand<SymptomViewModel> ClickItem
        {
            get
            {
                return _clickItem
                    ?? (_clickItem = new RelayCommand<SymptomViewModel>(
                                          p =>
                                          {
                                              CurrentSymptom = p;
                                          },
                                          p => p != null && !p.IsTerminal
                                          ));
            }
        }
    }
}
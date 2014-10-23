using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.Core;

namespace Diagnosis.ViewModels.Screens
{
    public class AsideViewModel : ViewModelBase
    {
        public AsideViewModel()
        {
            Panels = new ObservableCollection<PanelViewModel>();
            Panels.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (PanelViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += (s1, e1) =>
                        {
                            if (e1.PropertyName == "Opened")
                            {
                                OnPropertyChanged(() => AllClosed);
                            }
                        };
                    }
                }
            };

            this.Subscribe(Events.SendToSearch, (e) =>
            {
                SearchPanel.Opened = true;
            });

            SearchPanel = new PanelViewModel(new SearchViewModel()) { Title = "Поиск" };
            Panels.Add(SearchPanel);
        }
        public ObservableCollection<PanelViewModel> Panels { get; private set; }

        public PanelViewModel SearchPanel { get; private set; }

        public bool AllClosed
        {
            get
            {
                return Panels.All(p => !p.Opened);
            }
        }

    }
}

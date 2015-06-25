using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Diagnosis.ViewModels.Screens
{
    public class UomsListViewModel : ScreenBaseViewModel, IFilterableList
    {
        private FilterViewModel<Uom> _filter;
        private ObservableCollection<UomViewModel> _uoms;
        private bool _noUoms;
        private UomViewModel _current;
        private FilterableListHelper<Uom, UomViewModel> filterHelper;
        private EventMessageHandler handler;

        public UomsListViewModel()
        {
            SelectedUoms = new ObservableCollection<UomViewModel>();
            CreateFilter();
            handler = this.Subscribe(Event.NewSession, (e) => CreateFilter());
            Filter.Filtered += (s, e) =>
            {
                MakeVms(Filter.Results);
            };

            filterHelper = new FilterableListHelper<Uom, UomViewModel>(this, (v) => v.uom);
            filterHelper.AddAfterEntitySavedAction(() => NoUoms = false);

            Title = "Единицы измерения";
            Filter.Clear(); // показываем все
            NoUoms = !EntityQuery<Uom>.Any(Session)();
        }

        public FilterViewModel<Uom> Filter
        {
            get { return _filter; }
        }

        public ObservableCollection<UomViewModel> Uoms
        {
            get
            {
                if (_uoms == null)
                {
                    _uoms = new ObservableCollection<UomViewModel>();
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(_uoms);
                    SortDescription sort1 = new SortDescription("Description", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort1);
                }
                return _uoms;
            }
        }

        public UomViewModel SelectedUom
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
                    OnPropertyChanged(() => SelectedUom);
                }
            }
        }

        public ObservableCollection<UomViewModel> SelectedUoms { get; private set; }

        public RelayCommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    string abbr = "";
                    double factor = 0;
                    var type = EntityQuery<UomType>.FirstOrDefault(Session)();

                    if (type != null)
                    {
                        var uom = new Uom(abbr, factor, type);
                        this.Send(Event.EditUom, uom.AsParams(MessageKeys.Uom));
                    }
                });
            }
        }

        public RelayCommand CopyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var abbr = SelectedUom.Abbr;
                    var descr = SelectedUom.Description;
                    var factor = SelectedUom.Factor;
                    var type = SelectedUom.Type;

                    var uom = new Uom(abbr, factor, type) { Description = descr };
                    this.Send(Event.EditUom, uom.AsParams(MessageKeys.Uom));
                }, () => SelectedUom != null);
            }
        }

        /// <summary>
        /// В БД нет единиц.
        /// </summary>
        public bool NoUoms
        {
            get
            {
                return _noUoms;
            }
            set
            {
                if (_noUoms != value)
                {
                    _noUoms = value;
                    OnPropertyChanged(() => NoUoms);
                }
            }
        }
        private void CreateFilter()
        {
            _filter = new FilterViewModel<Uom>(UomQuery.Contains(Session));
        }


        private void MakeVms(ObservableCollection<Uom> results)
        {
            var vms = results.Select(u => Uoms
                .Where(vm => vm.uom == u)
                .FirstOrDefault() ?? new UomViewModel(u));

            Uoms.SyncWith(vms);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _filter.Dispose();
                filterHelper.Dispose();
                handler.Dispose();
            }
            base.Dispose(disposing);
        }

        IFilter IFilterableList.Filter
        {
            get { return Filter; }
        }

        IEnumerable<CheckableBase> IFilterableList.Items
        {
            get { return Uoms.Cast<CheckableBase>(); }
        }
    }
}
using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class UomsListViewModel : ScreenBaseViewModel, IFilterableList
    {
        private FilterViewModel<Uom> _filter;
        private ObservableCollection<UomViewModel> _uoms;
        private bool _noUoms;
        private UomViewModel _current;
        private EventMessageHandlersManager emhManager;
        private Saver saver;

        public UomsListViewModel()
        {
            _filter = new FilterViewModel<Uom>(UomQuery.Contains(Session));
            saver = new Saver(Session);
            SelectedUoms = new ObservableCollection<UomViewModel>();

            Filter.Filtered += (s, e) =>
            {
                MakeVms(Filter.Results);
            };
            Filter.Clear(); // показываем все

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Event.UomSaved, (e) =>
                {
                    // новое или изменившееся с учетом фильтра
                    Filter.Filter();

                    var saved = e.GetValue<Uom>(MessageKeys.Uom);
                    
                    var vm = Uoms.Where(x => x.uom == saved).FirstOrDefault();
                    if (vm != null)
                        vm.IsSelected = true;

                    NoUoms = false;
                }),

            });

            Title = "Единицы измерения";
            NoUoms = !Session.Query<Uom>().Any();
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
                    UomType type = Session.QueryOver<UomType>().List().FirstOrDefault();

                    var uom = new Uom(abbr, factor, type);
                    this.Send(Event.EditUom, uom.AsParams(MessageKeys.Uom));
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
                    var factor = SelectedUom.Factor;
                    var type = SelectedUom.Type;

                    var uom = new Uom(abbr, factor, type);
                    this.Send(Event.EditUom, uom.AsParams(MessageKeys.Uom));
                }, () => SelectedUom != null);
            }
        }

        //public ICommand EditCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            this.Send(Event.EditWord, SelectedUom.uom.AsParams(MessageKeys.Word));
        //        }, () => SelectedUom != null);
        //    }
        //}

        //public ICommand DeleteCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            var toDel = SelectedUoms
        //                .Select(w => w.word)
        //                .Where(w => w.IsEmpty())
        //                .ToArray();

        //            saver.Delete(toDel);

        //            // убираем удаленных из списка
        //            Filter.Filter();

        //            NoUoms = !Session.Query<Uom>().Any();
        //        }, () => SelectedUoms.Any(w => w.word.IsEmpty()));
        //    }
        //}


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
                emhManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
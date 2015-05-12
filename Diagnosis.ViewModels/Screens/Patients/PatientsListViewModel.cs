using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Controls;
using Diagnosis.ViewModels.Search;
using EventAggregator;
using NHibernate.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientsListViewModel : ScreenBaseViewModel, IFilterableList, IFocusable
    {
        private Patient _current;
        private bool _noPatients;
        private EventMessageHandlersManager emhManager;
        private FilterViewModel<Patient> _filter;
        private bool _focused;
        private ObservableCollection<Patient> _patients;
        private PatientsViewSortingColumn _sorting;
        private ListSortDirection _direction;
        private bool _ageVis;
        private bool _isMaleVis;
        private bool _isLastUpdatedVis;
        private Saver saver;
        private ListCollectionView view;
        private Doctor doctor;

        public PatientsListViewModel()
        {
            doctor = AuthorityController.CurrentDoctor;

            _filter = new FilterViewModel<Patient>(PatientQuery.StartingWith(Session));
            saver = new Saver(Session);
            SelectedPatients = new ObservableCollection<Patient>();

            Filter.Filtered += (s, e) =>
            {
                Patients.SyncWith(Filter.Results);
            };

            Title = "Пациенты";
            NoPatients = !Session.Query<Patient>().Any();

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Event.PatientSaved, (e) =>
                {
                    // выбираем нового пациента или изменившегося с учетом фильтра
                    Filter.Filter();
                    var saved = e.GetValue<Patient>(MessageKeys.Patient);
                    var visible = Patients.Where(x => x == saved).FirstOrDefault();
                    if (visible != null)
                        SelectedPatient = saved;

                    NoPatients = false;
                })
            });

            PatientsViewSortingColumn sort;
            if (Enum.TryParse<PatientsViewSortingColumn>(doctor.Settings.PatientsListSorting, true, out sort))
                Sorting = sort;
            else
                Sorting = PatientsViewSortingColumn.LastHrUpdatedAt;

            ListSortDirection dir;
            if (Enum.TryParse<ListSortDirection>(doctor.Settings.PatientsListSortingDirection, true, out dir))
                SortDirection = dir;
            else
                SortDirection = ListSortDirection.Descending;

            PatientsViewSortingColumn visCols;
            if (Enum.TryParse<PatientsViewSortingColumn>(doctor.Settings.PatientsListVisibleColumns, true, out visCols))
            {
                IsAgeColumnVisible = visCols.HasFlag(PatientsViewSortingColumn.Age);
                IsMaleColumnVisible = visCols.HasFlag(PatientsViewSortingColumn.IsMale);
                IsLastHrUpdatedAtColumnVisible = visCols.HasFlag(PatientsViewSortingColumn.LastHrUpdatedAt);
            }
            else
            {
                IsAgeColumnVisible = true;
                IsMaleColumnVisible = true;
                IsLastHrUpdatedAtColumnVisible = true;
            }

            Filter.Clear(); // показываем всех
            SelectLastPatient();
        }

        public ObservableCollection<Patient> Patients
        {
            get
            {
                if (_patients == null)
                {
                    _patients = new ObservableCollection<Patient>();
                    view = (ListCollectionView)CollectionViewSource.GetDefaultView(_patients);
                }
                return _patients;
            }
        }

        public FilterViewModel<Patient> Filter { get { return _filter; } }

        public PatientsViewSortingColumn Sorting
        {
            get
            {
                return _sorting;
            }
            set
            {
                if (_sorting != value)
                {
                    var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Patients);
                    using (view.DeferRefresh())
                    {
                        var old = view.SortDescriptions.FirstOrDefault();
                        view.SortDescriptions.Clear();
                        if (value != PatientsViewSortingColumn.None)
                        {
                            var sort = new SortDescription(value.ToString(), old.Direction);
                            view.SortDescriptions.Add(sort);
                        }
                    }

                    _sorting = value;
                    OnPropertyChanged(() => Sorting);
                }
            }
        }

        public ListSortDirection SortDirection
        {
            get
            {
                return _direction;
            }
            set
            {
                if (_direction != value)
                {
                    var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Patients);
                    using (view.DeferRefresh())
                    {
                        var old = view.SortDescriptions.FirstOrDefault();
                        view.SortDescriptions.Clear();

                        var sort = new SortDescription(old.PropertyName ?? PatientsViewSortingColumn.LastHrUpdatedAt.ToString(), value);
                        view.SortDescriptions.Add(sort);
                    }

                    _direction = value;
                    OnPropertyChanged(() => SortDirection);
                }
            }
        }

        public bool IsAgeColumnVisible
        {
            get
            {
                return _ageVis;
            }
            set
            {
                if (_ageVis != value)
                {
                    _ageVis = value;
                    OnPropertyChanged(() => IsAgeColumnVisible);
                }
            }
        }

        public bool IsMaleColumnVisible
        {
            get
            {
                return _isMaleVis;
            }
            set
            {
                if (_isMaleVis != value)
                {
                    _isMaleVis = value;
                    OnPropertyChanged(() => IsMaleColumnVisible);
                }
            }
        }

        public bool IsLastHrUpdatedAtColumnVisible
        {
            get
            {
                return _isLastUpdatedVis;
            }
            set
            {
                if (_isLastUpdatedVis != value)
                {
                    _isLastUpdatedVis = value;
                    OnPropertyChanged(() => IsLastHrUpdatedAtColumnVisible);
                }
            }
        }

        public Patient SelectedPatient
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
                    OnPropertyChanged(() => SelectedPatient);
                }
            }
        }

        public ObservableCollection<Patient> SelectedPatients { get; private set; }

        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Patient pat = new Patient(Filter.Query);

                    this.Send(Event.EditPatient, pat.AsParams(MessageKeys.Patient));
                });
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var toDel = SelectedPatients
                        .Where(p => p.IsEmpty())
                        .ToArray();
                    saver.Delete(toDel);

                    // убираем удаленных из списка
                    Filter.Filter();

                    // оставляем выделение тех, кто не удаляется
                    //toDel.Where(p => Patients.Contains(p))
                    //    .ForEach(p => SelectedPatients.Add(p));

                    NoPatients = !Session.Query<Patient>().Any();
                }, () => SelectedPatients.Any(p => p.IsEmpty()));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.OpenPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                        }, () => SelectedPatient != null);
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                }, () => SelectedPatient != null);
            }
        }

        /// <summary>
        /// В БД нет пациентов.
        /// </summary>
        public bool NoPatients
        {
            get
            {
                return _noPatients;
            }
            set
            {
                if (_noPatients != value)
                {
                    _noPatients = value;
                    OnPropertyChanged(() => NoPatients);
                }
            }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    OnPropertyChanged(() => IsFocused);
                }
            }
        }

        public void SelectLastPatient()
        {
            if (Patients.Count > 0)
            {
                SelectedPatient = PatientViewer.LastOpenedPatient ?? (Patient)view.GetItemAt(0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                emhManager.Dispose();
                _filter.Dispose();

                if (view.SortDescriptions.Count > 0)
                {
                    var sort = view.SortDescriptions.FirstOrDefault();
                    doctor.Settings.PatientsListSorting = sort.PropertyName;
                    doctor.Settings.PatientsListSortingDirection = sort.Direction.ToString();

                }
                else
                {
                    // no sort applied
                    // sort cannot be removed, no need to remove setting
                }
                var visCols = PatientsViewSortingColumn.FullNameOrCreatedAt;
                if (IsAgeColumnVisible) visCols |= PatientsViewSortingColumn.Age;
                if (IsMaleColumnVisible) visCols |= PatientsViewSortingColumn.IsMale;
                if (IsLastHrUpdatedAtColumnVisible) visCols |= PatientsViewSortingColumn.LastHrUpdatedAt;

                doctor.Settings.PatientsListVisibleColumns = visCols.ToString();

                new Saver(Session).Save(doctor);
            }
            base.Dispose(disposing);
        }

        IFilter IFilterableList.Filter
        {
            get { return Filter; }
        }
    }
}
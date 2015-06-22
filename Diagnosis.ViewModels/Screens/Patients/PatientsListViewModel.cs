using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Controls;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientsListViewModel : ScreenBaseViewModel, IFilterableList, IFocusable
    {
        private PatientViewModel _current;
        private bool _noPatients;
        private FilterViewModel<Patient> _filter;
        private bool _focused;
        private ObservableCollection<PatientViewModel> _patients;
        private PatientsViewSortingColumn _sorting;
        private ListSortDirection _direction;
        private bool _ageVis;
        private bool _isMaleVis;
        private bool _isLastUpdatedVis;
        private ListCollectionView view;
        private FilterableListHelper<Patient, PatientViewModel> filterHelper;

        public PatientsListViewModel()
        {
            _filter = new FilterViewModel<Patient>(PatientQuery.StartingWith(Session));
            Filter.Filtered += (s, e) =>
            {
                MakeVms(Filter.Results);
            };

            filterHelper = new FilterableListHelper<Patient, PatientViewModel>(this, (v) => v.patient);
            filterHelper.AddAfterEntitySavedAction(() => NoPatients = false);

            SetupSorting();
            SetupColumnsVisibility();

            Title = "Пациенты";
            Filter.Clear(); // показываем всех
            NoPatients = !EntityQuery<Patient>.Any(Session)();
            SelectLastPatient();
        }

        public ObservableCollection<PatientViewModel> Patients
        {
            get
            {
                if (_patients == null)
                {
                    _patients = new ObservableCollection<PatientViewModel>();
                    view = (ListCollectionView)CollectionViewSource.GetDefaultView(_patients);
                }
                return _patients;
            }
        }

        public PatientViewModel SelectedPatient
        {
            get
            {
                Contract.Ensures(Contract.Result<CheckableBase>() == null || Contract.Result<CheckableBase>().IsSelected);
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    if (_current != null)
                        _current.IsSelected = true;
                    OnPropertyChanged(() => SelectedPatient);
                }
            }
        }

        public IEnumerable<PatientViewModel> SelectedPatients { get { return Patients.Where(x => x.IsSelected); } }

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
                        .Where(x => x.patient.IsEmpty())
                        .Select(x => x.patient)
                        .ToArray();
                    Session.DoDelete(toDel);

                    // убираем удаленных из списка
                    Filter.Filter();

                    NoPatients = !EntityQuery<Patient>.Any(Session)();
                }, () => SelectedPatients.Any(x => x.patient.IsEmpty()));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.OpenHolder, SelectedPatient.patient.AsParams(MessageKeys.Holder));
                        }, () => SelectedPatient != null);
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditPatient, SelectedPatient.patient.AsParams(MessageKeys.Patient));
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

        IFilter IFilterableList.Filter
        {
            get { return Filter; }
        }

        IEnumerable<CheckableBase> IFilterableList.Items
        {
            get { return Patients.Cast<CheckableBase>(); }
        }

        public void SelectLastPatient()
        {
            var lastOpened = HierViewer<Patient, Course, Appointment, IHrsHolder>.LastOpenedRoot;
            var vm = Patients.Where(x => x.patient == lastOpened).FirstOrDefault();

            if (Patients.Count > 0)
            {
                SelectedPatient = vm ?? (PatientViewModel)view.GetItemAt(0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _filter.Dispose();
                filterHelper.Dispose();

                SaveDoctorSettings();
            }
            base.Dispose(disposing);
        }

        private void MakeVms(IEnumerable<Patient> results)
        {
            var vms = results.Select(w => Patients
                .Where(vm => vm.patient == w)
                .FirstOrDefault() ?? new PatientViewModel(w));

            Patients.SyncWith(vms);
        }

        private void SetupColumnsVisibility()
        {
            var doctor = AuthorityController.CurrentDoctor;

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
        }

        private void SetupSorting()
        {
            var doctor = AuthorityController.CurrentDoctor;

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
        }

        private void SaveDoctorSettings()
        {
            var doctor = AuthorityController.CurrentDoctor;

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

            Session.DoSave(doctor);
        }
    }
}
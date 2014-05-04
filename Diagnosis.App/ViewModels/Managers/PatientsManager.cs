﻿using Diagnosis.Data.Repositories;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class PatientsManager : ViewModelBase
    {
        private SearchBase<PatientViewModel> _search;
        private PatientViewModel _current;
        private ICommand _addPatient;
        private IPatientRepository patientRepo;

        public ObservableCollection<PatientViewModel> Patients { get; private set; }

        public PatientViewModel CurrentPatient
        {
            get
            {
                return _current;
            }
            set
            {
                if (_current != value)
                {
                    if (_current != null && value != null && !(value is UnsavedPatientViewModel))
                    {
                        // save editor state between patients
                        value.Editable.IsEditorActive = _current.Editable.IsEditorActive;
                    }

                    _current = value;

                    OnPropertyChanged(() => CurrentPatient);

                    SetSubscriptions(value);

                    this.Send((int)EventID.CurrentPatientChanged, new CurrentPatientChangedParams(CurrentPatient).Params);
                }
            }
        }
        public SearchBase<PatientViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new SearchBase<PatientViewModel>(new PatientSearcher(Patients));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public void RemoveCurrent()
        {
            if (CurrentPatient != null &&
                CurrentPatient.CoursesManager.SelectedCourse != null &&
                CurrentPatient.CoursesManager.SelectedCourse.SelectedAppointment != null &&
                CurrentPatient.CoursesManager.SelectedCourse.SelectedAppointment.SelectedHealthRecord != null)
            {
                CurrentPatient.CoursesManager.SelectedCourse.SelectedAppointment.SelectedHealthRecord.Unsubscribe();
            }
            CurrentPatient = null;
        }
        public ICommand AddPatientCommand
        {
            get
            {
                return _addPatient
                    ?? (_addPatient = new RelayCommand(
                                          () =>
                                          {
                                              var newPatientVM = new UnsavedPatientViewModel();
                                              CurrentPatient = newPatientVM;
                                              newPatientVM.PatientCreated += (s, e) =>
                                              {
                                                  patientRepo.SaveOrUpdate(e.patientVM.patient);
                                                  e.patientVM.AfterPatientLoaded();
                                                  Patients.Add(e.patientVM);
                                                  CurrentPatient = e.patientVM;
                                                  Subscribe(CurrentPatient);
                                              };
                                          }));
            }
        }

        public PatientsManager(IPatientRepository patientRepo)
        {
            Contract.Requires(patientRepo != null);
            this.patientRepo = patientRepo;

            var patientVMs = patientRepo.GetAll().Select(p => new PatientViewModel(p)).ToList();
            foreach (var pvm in patientVMs)
            {
                Subscribe(pvm);
            }
            patientVMs.Sort(PatientViewModel.CompareByFullName);
            Patients = new ObservableCollection<PatientViewModel>(patientVMs);

            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }

        private void SetSubscriptions(PatientViewModel newPatient)
        {
            foreach (var patient in Patients)
            {
                if (patient != newPatient)
                {
                    patient.Unsubscribe();
                }
            }

            if (newPatient != null)
                newPatient.Subscribe();
        }

        private void Subscribe(PatientViewModel pvm)
        {
            pvm.Editable.Committed += p_Committed;
        }

        private void _search_ResultItemSelected(object sender, EventArgs e)
        {
            var patientVM = Search.SelectedItem as PatientViewModel;
            if (patientVM != null)
            {
                CurrentPatient = patientVM;
                Search.Clear();
            }
        }

        private void p_Committed(object sender, EditableEventArgs e)
        {
            var patientVM = e.viewModel as PatientViewModel;
            if (patientVM != null)
            {
                patientRepo.SaveOrUpdate(patientVM.patient);
            }
        }
    }
}
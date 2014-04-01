using Diagnosis.Data.Repositories;
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
        private PatientSearch _search;
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
                    if (_current != null && value != null && !(value is NewPatientViewModel))
                    {
                        value.Editable.IsEditorActive = _current.Editable.IsEditorActive;
                    }

                    _current = value;

                    OnPropertyChanged(() => CurrentPatient);

                    foreach (var patient in Patients)
                    {
                        if (patient != value)
                        {
                            patient.Unsubscribe();
                        }
                    }

                    if (value != null && !(value is NewPatientViewModel))
                        value.Subscribe();

                    this.Send((int)EventID.CurrentPatientChanged, new CurrentPatientChangedParams(CurrentPatient).Params);
                }
            }
        }

        public PatientSearch Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PatientSearch(Patients);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public void NoCurrent()
        {
            if (CurrentPatient.CoursesManager.SelectedCourse.SelectedAppointment != null &&
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
                                              var newPatientVM = new NewPatientViewModel();
                                              newPatientVM.Editable.IsEditorActive = true;
                                              CurrentPatient = newPatientVM;
                                              newPatientVM.PatientCreated += (s, e) =>
                                              {
                                                  patientRepo.SaveOrUpdate(e.patientVM.patient);
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
            patientVMs.Sort(PatientViewModel.CompareByName);
            Patients = new ObservableCollection<PatientViewModel>(patientVMs);

            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }

        private void Subscribe(PatientViewModel pvm)
        {
            pvm.Editable.Committed += p_Committed;
        }

        private void _search_ResultItemSelected(object sender, EventArgs e)
        {
            var patientVM = Search.SelectedItem;
            if (patientVM != null)
            {
                if (Patients.SingleOrDefault(p => p == patientVM) == null)
                {
                    Patients.Add(patientVM);
                }
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
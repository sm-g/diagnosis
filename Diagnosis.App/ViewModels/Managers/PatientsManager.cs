using Diagnosis.App.Messaging;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
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
        private PopupSearch<PatientViewModel> _search;
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
                    if (_current != null)
                    {
                        _current.Unsubscribe();
                        if (value != null)
                        {
                            if (!(value is UnsavedPatientViewModel))
                            {
                                // сохраняем состояние редактора при смене пациента
                                value.Editable.IsEditorActive = _current.Editable.IsEditorActive;
                            }

                            // у старого пациента был открыт редактор — сохраняем изменения в нем
                            if (_current.Editable.IsEditorActive)
                            {
                                _current.Editable.Commit();
                            }
                            value.Subscribe();
                        }
                        else
                        {
                            CurrentPatient.CoursesManager.UnsubscribeSelectedHr();
                            Console.WriteLine("current patient removed");
                        }
                    }

                    _current = value;

                    OnPropertyChanged("CurrentPatient");
                    this.Send((int)EventID.CurrentPatientChanged, new CurrentPatientChangedParams(CurrentPatient).Params);
                }
            }
        }

        public PopupSearch<PatientViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<PatientViewModel>(new PatientSearcher(Patients));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public ICommand AddPatientCommand
        {
            get
            {
                return _addPatient
                    ?? (_addPatient = new RelayCommand(AddPatient));
            }
        }

        public void RemoveCurrent()
        {
            CurrentPatient = null;
        }

        public void SetCurrentToLast()
        {
            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }

        public PatientViewModel GetByModel(Patient patient)
        {
            return Patients.Where(p => p.patient == patient).FirstOrDefault();
        }

        public void OpenLastAppointment(PatientViewModel patient)
        {
            // последний курс или новый, если курсов нет
            var lastCourse = patient.CoursesManager.Courses.FirstOrDefault();
            if (lastCourse == null)
            {
                patient.CurrentDoctor.StartCourse(patient);
            }
            else
            {
                patient.CoursesManager.SelectedCourse = lastCourse;
            }

            // последняя встреча в течение часа или новая
            var lastApp = patient.CoursesManager.SelectedCourse.LastAppointment;
            if (DateTime.UtcNow - lastApp.DateTime > TimeSpan.FromHours(1))
            {
                patient.CoursesManager.SelectedCourse.AddAppointment();
            }
            else
            {
                patient.CoursesManager.SelectedCourse.SelectedAppointment = lastApp;
            }
        }

        public PatientsManager(IPatientRepository patientRepo)
        {
            Contract.Requires(patientRepo != null);
            this.patientRepo = patientRepo;

            var patientVMs = patientRepo.GetAll().Select(p => new PatientViewModel(p)).ToList();
            foreach (var pvm in patientVMs)
            {
                SubscribeEditable(pvm);
            }
            patientVMs.Sort(PatientViewModel.CompareByFullName);
            Patients = new ObservableCollection<PatientViewModel>(patientVMs);

            SetCurrentToLast();
        }

        private void AddPatient()
        {
            var newPatientVM = new UnsavedPatientViewModel();
            CurrentPatient = newPatientVM;
            newPatientVM.PatientCreated += OnPatientCreated;
        }

        private void OnPatientCreated(object s, PatientEventArgs e)
        {
            var unsaved = e.patientVM as UnsavedPatientViewModel;
            unsaved.PatientCreated -= OnPatientCreated;

            patientRepo.SaveOrUpdate(unsaved.patient);
            var modelFromRepo = patientRepo.GetById(unsaved.patient.Id);

            var saved = new PatientViewModel(modelFromRepo);
            saved.CanAddFirstHr = !e.addFirstHr;
            Patients.Add(saved);
            CurrentPatient = saved;
            SubscribeEditable(saved);
            if (e.addFirstHr)
            {
                saved.Editable.IsEditorActive = false;
                OpenLastAppointment(saved);
            }
        }

        private void SubscribeEditable(PatientViewModel pvm)
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
using Diagnosis.Data.Repositories;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class PatientsManager : ViewModelBase
    {
        private PatientSearch _search;
        private PatientViewModel _current;
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
                    _current = value;

                    OnPropertyChanged(() => CurrentPatient);

                    foreach (var patient in Patients)
                    {
                        if (patient != value)
                        {
                            patient.Unsubscribe();
                        }
                    }
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

        public PatientsManager(IPatientRepository patientRepo)
        {
            Contract.Requires(patientRepo != null);
            this.patientRepo = patientRepo;

            var patientVMs = patientRepo.GetAll().Select(p => new PatientViewModel(p)).ToList();
            foreach (var pvm in patientVMs)
            {
                pvm.Editable.Committed += p_Committed;
            }

            Patients = new ObservableCollection<PatientViewModel>(patientVMs);
            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
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

        private void p_Committed(object sender, EventArgs e)
        {
            var patientVM = (sender as PatientViewModel);
            if (patientVM != null)
            {
                patientRepo.SaveOrUpdate(patientVM.patient);
            }
        }
    }
}
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.Data.Repositories;

namespace Diagnosis.ViewModels
{
    public class PatientsListVewModel : ViewModelBase
    {
        private PatientSearch _search;
        private PatientViewModel _current;
        IPatientRepository repo;

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
                    _search = new PatientSearch(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        private void _search_ResultItemSelected(object sender, EventArgs e)
        {
            var patientVM = Search.SelectedItem;
            if (Patients.SingleOrDefault(p => p == patientVM) == null)
            {
                Patients.Add(patientVM);
                patientVM.ModelPropertyChanged += patientModelPropertyChanged;
                repo.Add(patientVM.patient);
            }
            CurrentPatient = patientVM;
            Search.Clear();
        }

        public PatientsListVewModel(IPatientRepository repo)
        {
            Contract.Requires(repo != null);
            this.repo = repo;

            var patients = repo.GetAll().Select(p => new PatientViewModel(p)).ToList();
            foreach (var p in patients)
            {
                p.ModelPropertyChanged += patientModelPropertyChanged;
            }

            Patients = new ObservableCollection<PatientViewModel>(patients);
            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }

        void patientModelPropertyChanged(object sender, EventArgs e)
        {
            var patient = (sender as PatientViewModel);
            if (patient != null)
            {
                repo.Update(patient.patient);
            }
        }
    }
}
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

        public PatientsListVewModel(IPatientRepository repo)
        {
            Contract.Requires(repo != null);
            this.repo = repo;

            var patientVMs = patientRepo.GetAll().Select(p => new PatientViewModel(p, propManager)).ToList();
            foreach (var pvm in patientVMs)
            {
                pvm.Committed += p_Committed;
            }

            Patients = new ObservableCollection<PatientViewModel>(patientVMs);
            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }

        void p_Committed(object sender, EventArgs e)
        {
            var patientVM = (sender as PatientViewModel);
            if (patientVM != null)
            {
                patientRepo.SaveOrUpdate(patientVM.patient);
            }
        }
    }
}
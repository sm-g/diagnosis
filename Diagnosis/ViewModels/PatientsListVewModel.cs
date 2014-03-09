using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientsListVewModel : ViewModelBase
    {
        private PatientSearchViewModel _search;
        private PatientViewModel _current;

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

        public PatientSearchViewModel Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PatientSearchViewModel(this);
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        private void _search_ResultItemSelected(object sender, EventArgs e)
        {
            if (Patients.SingleOrDefault(p => p == Search.SelectedItem) == null)
            {
                Patients.Add(Search.SelectedItem);
            }
            CurrentPatient = Search.SelectedItem;
            Search.Clear();
        }

        public PatientsListVewModel(IEnumerable<PatientViewModel> patients)
        {
            Contract.Requires(patients != null);

            Patients = new ObservableCollection<PatientViewModel>(patients);
            if (Patients.Count > 0)
            {
                CurrentPatient = Patients[0];
            }
        }
    }
}
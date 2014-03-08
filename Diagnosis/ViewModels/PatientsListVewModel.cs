using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public class PatientsListVewModel : ViewModelBase
    {
        private int _curPatientIndex;
        PatientSearchViewModel _search;

        public ObservableCollection<PatientViewModel> Patients { get; private set; }

        public PatientViewModel CurrentPatient
        {
            get
            {
                if (CurrentPatientIndex >= 0)
                {
                    return Patients[CurrentPatientIndex];
                }
                return null;
            }
        }

        public int CurrentPatientIndex
        {
            get
            {
                return _curPatientIndex;
            }
            set
            {
                if (_curPatientIndex != value)
                {
                    _curPatientIndex = value;
                    OnPropertyChanged(() => CurrentPatientIndex);

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

        void _search_ResultItemSelected(object sender, EventArgs e)
        {
            CurrentPatientIndex = Search.SelectedIndex;
            Search.Clear();
        }

        public PatientsListVewModel(IEnumerable<PatientViewModel> patients)
        {
            Contract.Requires(patients != null);

            Patients = new ObservableCollection<PatientViewModel>(patients);
        }
    }
}

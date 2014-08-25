using Diagnosis.App.Messaging;
using Diagnosis.Core;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class PatientsListViewModel : ViewModelBase
    {
        private PatientViewModel _current;
        private PatientsProducer _producer;
        private ICommand _open;
        private PopupSearch<PatientViewModel> _search;

        public PatientsListViewModel(PatientsProducer manager)
        {
            _producer = manager;
            this.Subscribe((int)EventID.OpenedPatientChanged, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(Messages.Patient);
                SelectedPatient = pat;
            });
        }

        public ObservableCollection<PatientViewModel> Patients { get { return _producer.Patients; } }

        public PatientViewModel SelectedPatient
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

        public ICommand AddPatientCommand
        {
            get
            {
                return _producer.AddPatientCommand;
            }
        }

        public ICommand OpenPatientCommand
        {
            get
            {
                return _open
                   ?? (_open = new RelayCommand(() =>
                        {
                            this.Send((int)EventID.OpenPatient, new PatientParams(SelectedPatient).Params);
                        }, () => SelectedPatient != null));
            }
        }

        public PopupSearch<PatientViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<PatientViewModel>(new PatientSearcher(_producer.Patients));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public void SelectLastPatient()
        {
            if (Patients.Count > 0)
            {
                SelectedPatient = Patients[0];
            }
        }
        private void _search_ResultItemSelected(object sender, EventArgs e)
        {
            var patientVM = Search.SelectedItem as PatientViewModel;
            if (patientVM != null)
            {
                SelectedPatient = patientVM;
                Search.Clear();
            }
        }
    }
}
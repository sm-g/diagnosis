using Diagnosis.App.Messaging;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Diagnosis.App.ViewModels
{
    public class PatientsAsideViewModel : ViewModelBase
    {
        private PatientViewModel _current;
        PatientsProducer _producer;
        private PopupSearch<PatientViewModel> _search;

        public PatientsAsideViewModel(PatientsProducer manager)
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
                    this.Send((int)EventID.OpenPatient, new PatientParams(value).Params);
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

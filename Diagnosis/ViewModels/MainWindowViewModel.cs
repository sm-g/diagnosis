using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using EventAggregator;
using Diagnosis.Data.Repositories;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientsListViewModel _patientsVM;
        private PatientViewModel _patientVM;
        private ICommand _logout;

        public bool IsLoginActive
        {
            get
            {
                return _loginActive;
            }
            set
            {
                if (_loginActive != value)
                {
                    _loginActive = value;
                    if (value)
                    {
                        MakeLoginDataContext();
                    }

                    OnPropertyChanged(() => IsLoginActive);
                }
            }
        }

        private bool _isPatientsVisible;
        public bool IsPatientsVisible
        {
            get
            {
                return _isPatientsVisible;
            }
            set
            {
                if (_isPatientsVisible != value)
                {
                    _isPatientsVisible = value;
                    OnPropertyChanged(() => IsPatientsVisible);
                }
            }
        }

        public LoginViewModel LoginVM
        {
            get
            {
                return _loginVM;
            }
            set
            {
                if (_loginVM != value)
                {
                    _loginVM = value;
                    OnPropertyChanged(() => LoginVM);
                }
            }
        }
        public PatientsListViewModel PatientsVM
        {
            get
            {
                return _patientsVM;
            }
            set
            {
                if (_patientsVM != value)
                {
                    _patientsVM = value;
                    OnPropertyChanged(() => PatientsVM);
                }
            }
        }
        public PatientViewModel CardVM
        {
            get
            {
                return _patientVM;
            }
            set
            {
                if (_patientVM != value)
                {
                    _patientVM = value;
                    OnPropertyChanged(() => CardVM);
                }
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                return _logout ?? (_logout = new RelayCommand(
                                          () =>
                                          {
                                              IsLoginActive = true;
                                          },
                                          () => !IsLoginActive));
            }
        }

        private void MakeLoginDataContext()
        {
            if (LoginVM != null)
            {
                LoginVM.LoggedIn -= OnLoggedIn;
            }

            var doctorRepo = new DoctorRepository();
            var doctorVMs = doctorRepo.GetAll().Select(d => new DoctorViewModel(d));

            LoginVM = new LoginViewModel(doctorVMs);
            LoginVM.LoggedIn += OnLoggedIn;
        }

        void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
            IsPatientsVisible = true;
        }

        public MainWindowViewModel()
        {
#if RELEASE
            IsLoginActive = true;
#endif

            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                CardVM = patient;
            });

            PatientsVM = new PatientsListViewModel(new PatientRepository(), new PropertyManager(new PropertyRepository()));
        }
    }
}

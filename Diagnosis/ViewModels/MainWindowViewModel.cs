using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using EventAggregator;

namespace Diagnosis.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientsListVewModel _patientsVM;
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
        public PatientsListVewModel PatientsVM
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
            LoginVM = new LoginViewModel();
            LoginVM.LoggedIn += OnLoggedIn;
        }

        void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
        }

        public MainWindowViewModel()
        {
#if RELEASE
            IsLoginActive = true;
#endif

            PatientsVM = new PatientsListVewModel(DataCreator.Patients);
            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                CardVM = patient;
            });
        }
    }
}

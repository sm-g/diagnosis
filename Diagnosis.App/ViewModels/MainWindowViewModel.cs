﻿using EventAggregator;
using System.Windows.Input;
using Diagnosis.Data.Repositories;

namespace Diagnosis.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _loginActive;
        private LoginViewModel _loginVM;
        private PatientViewModel _patientVM;
        private ICommand _logout;
        private ICommand _editDiagnosisDirectory;
        private ICommand _editSymptomsDirectory;
        private bool _fastAddingMode;
        private bool _isPatientsVisible;
        private bool _isWordsEditing;
        private object _directoryExplorer;

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

                    OnPropertyChanged(() => IsLoginActive);
                }
            }
        }

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

        public bool IsWordsEditing
        {
            get
            {
                return _isWordsEditing;
            }
            set
            {
                if (_isWordsEditing != value)
                {
                    _isWordsEditing = value;

                    if (value)
                    {
                        PatientsVM.RemoveCurrent();
                    }

                    this.Send((int)EventID.WordsEditingModeChanged, new DirectoryEditingModeChangedParams(value).Params);
                    OnPropertyChanged(() => IsWordsEditing);
                }
            }
        }
        public WordsManager WordsManager
        {
            get
            {
                return EntityManagers.WordsManager;
            }
        }

        public bool FastAddingMode
        {
            get
            {
                return _fastAddingMode;
            }
            set
            {
                if (_fastAddingMode != value)
                {
                    _fastAddingMode = value;
                    OnPropertyChanged(() => FastAddingMode);
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

        public PatientsManager PatientsVM
        {
            get
            {
                return EntityManagers.PatientsManager;
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

        public object DirectoryExplorer
        {
            get
            {
                return _directoryExplorer;
            }
            set
            {
                if (_directoryExplorer != value)
                {
                    _directoryExplorer = value;
                    OnPropertyChanged(() => DirectoryExplorer);
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

        public ICommand EditSymptomsDirectoryCommand
        {
            get
            {
                return _editSymptomsDirectory
                    ?? (_editSymptomsDirectory = new RelayCommand(
                                          () =>
                                          {
                                              IsWordsEditing = true;
                                          }));
            }
        }

        public ICommand EditDiagnosisDirectoryCommand
        {
            get
            {
                return _editDiagnosisDirectory
                    ?? (_editDiagnosisDirectory = new RelayCommand(
                                          () =>
                                          {
                                              DirectoryExplorer = new HierarchicalExplorer<DiagnosisViewModel>(EntityManagers.DiagnosisManager.Diagnoses);
                                          }));
            }
        }

        public MainWindowViewModel()
        {
#if RELEASE
            IsLoginActive = true;
#endif

            this.Subscribe((int)EventID.CurrentPatientChanged, (e) =>
            {
                var patient = e.GetValue<PatientViewModel>(Messages.Patient);
                SetCurrentPatient(patient);
            });

            LoginVM = new LoginViewModel(EntityManagers.DoctorsManager);

            LoginVM.LoggedIn += OnLoggedIn;
        }

        private void SetCurrentPatient(PatientViewModel patient)
        {
            if (patient != null)
            {
                patient.SetDoctorVM(LoginVM.DoctorsManager.CurrentDoctor);
                if (FastAddingMode && !(patient is UnsavedPatientViewModel))
                {
                    patient.OpenLastAppointment();
                }
            }
            IsWordsEditing = false;
            CardVM = patient;
        }

        private void OnLoggedIn(object sender, LoggedEventArgs e)
        {
            IsLoginActive = false;
            IsPatientsVisible = true;
        }
    }
}
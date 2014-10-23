using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class LoginViewModel : ScreenBase
    {
        private SecureString _password;
        private bool _wrongpassword;
        private DoctorViewModel _current;

        public DoctorViewModel CurrentDoctor
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

                    OnPropertyChanged("CurrentDoctor");
                }
            }
        }

        public ReadOnlyObservableCollection<DoctorViewModel> Doctors { get; private set; }

        public SecureString Password
        {
            private get { return _password; }
            set
            {
                _password = value;
            }
        }

        public bool IsLoginEnabled
        {
            get
            {
                return true; //!String.IsNullOrWhiteSpace(Username ?? "") && Password != null && Password.Length > 0;
            }
        }

        public bool IsWrongPassword
        {
            get { return _wrongpassword; }
            set
            {
                if (_wrongpassword != value)
                {
                    _wrongpassword = value;
                    OnPropertyChanged(() => IsWrongPassword);
                };
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // Password.MakeReadOnly();
                    if (CurrentDoctor == null)
                    {
                        AuthorityController.LogIn(null);

                    }
                    else
                    {
                        AuthorityController.LogIn(CurrentDoctor.doctor);
                    }
                }, () => IsLoginEnabled);
            }
        }

        public LoginViewModel()
        {
            var doctorVMs = Session.QueryOver<Doctor>().List().Select(d => new DoctorViewModel(d)).ToList();
            Doctors = new ReadOnlyObservableCollection<DoctorViewModel>(new ObservableCollection<DoctorViewModel>(doctorVMs));

            if (Doctors.Count > 0)
            {
                CurrentDoctor = Doctors[0];
            }

            Title = "Вход";
        }

        public class DoctorViewModel : ViewModelBase
        {
            internal readonly Doctor doctor;

            public string FirstName
            {
                get
                {
                    return doctor.FirstName;
                }
                set
                {
                    if (doctor.FirstName != value)
                    {
                        doctor.FirstName = value;
                        OnPropertyChanged(() => FirstName);
                    }
                }
            }

            public string MiddleName
            {
                get
                {
                    return doctor.MiddleName ?? "";
                }
                set
                {
                    if (doctor.MiddleName != value)
                    {
                        doctor.MiddleName = value;
                        OnPropertyChanged(() => MiddleName);
                    }
                }
            }

            public string LastName
            {
                get
                {
                    return doctor.LastName;
                }
                set
                {
                    if (doctor.LastName != value)
                    {
                        doctor.LastName = value;
                        OnPropertyChanged(() => LastName);
                    }
                }
            }

            public bool IsMale
            {
                get
                {
                    return doctor.IsMale;
                }
                set
                {
                    if (doctor.IsMale != value)
                    {
                        doctor.IsMale = value;
                        OnPropertyChanged(() => IsMale);
                    }
                }
            }

            public string Speciality
            {
                get
                {
                    return doctor.Speciality.Title;
                }
            }

            public DoctorViewModel(Doctor d)
            {
                Contract.Requires(d != null);
                doctor = d;
            }

            public override string ToString()
            {
                return doctor.ToString();
            }
        }
    }


}
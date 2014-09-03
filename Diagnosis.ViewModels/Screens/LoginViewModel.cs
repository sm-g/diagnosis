using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private SecureString _password;
        private bool _wrongpassword;
        private DoctorViewModel _current;

        private IDoctorRepository repository;


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

                    AuthorityController.LogIn(CurrentDoctor.doctor);
                }, () => IsLoginEnabled);
            }
        }

        public LoginViewModel()
        {
            repository = new DoctorRepository();
            var doctorVMs = repository.GetAll().Select(d => new DoctorViewModel(d)).ToList();
            Doctors = new ReadOnlyObservableCollection<DoctorViewModel>(new ObservableCollection<DoctorViewModel>(doctorVMs));

            if (Doctors.Count > 0)
            {
                CurrentDoctor = Doctors[0];
            }
        }
    }


}
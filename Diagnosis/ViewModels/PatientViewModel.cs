using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels
{
    public class PatientViewModel : CheckableBase, ISearchable
    {
        private Patient patient;
        private List<EventMessageHandler> msgHandlers;

        public string FirstName
        {
            get
            {
                return patient.FirstName;
            }
            set
            {
                if (patient.FirstName != value)
                {
                    patient.FirstName = value;
                    OnPropertyChanged(() => FirstName);
                    OnPropertyChanged(() => ShortName);
                }
            }
        }

        public string MiddleName
        {
            get
            {
                return patient.MiddleName;
            }
            set
            {
                if (patient.MiddleName != value)
                {
                    patient.MiddleName = value;
                    OnPropertyChanged(() => MiddleName);
                    OnPropertyChanged(() => ShortName);
                }
            }
        }

        public string LastName
        {
            get
            {
                return patient.LastName;
            }
            set
            {
                if (patient.LastName != value)
                {
                    patient.LastName = value;
                    OnPropertyChanged(() => LastName);
                    OnPropertyChanged(() => ShortName);
                }
            }
        }

        public int Age
        {
            get
            {
                int age = DateTime.Today.Year - patient.BirthDate.Year;
                if (!patient.OnlyBirthYear && patient.BirthDate > DateTime.Today.AddYears(-age))
                    age--;
                return age;
            }
            set
            {
                int year = DateTime.Today.Year - value;
                if (!patient.OnlyBirthYear && new DateTime(value, patient.BirthDate.Month, patient.BirthDate.Day) < DateTime.Today.AddYears(-value)) // TODO
                    year--;
                BirthYear = year;
            }
        }

        public int BirthYear
        {
            get
            {
                return patient.BirthDate.Year;
            }
            set
            {
                if (patient.BirthDate.Year != value && value >= 0 && value <= DateTime.Today.Year)
                {
                    patient.BirthDate = new DateTime(value, patient.BirthDate.Month, patient.BirthDate.Day);
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthYear);
                }
            }
        }

        public int BirthMonth
        {
            get
            {
                if (patient.OnlyBirthYear)
                    return 13;
                return patient.BirthDate.Month;
            }
            set
            {
                if (patient.BirthDate.Month != value && value >= 1 && value <= 13)
                {
                    if (value == 13)
                    {
                        patient.OnlyBirthYear = true;
                    }
                    else
                    {
                        patient.OnlyBirthYear = false;
                        patient.BirthDate = new DateTime(patient.BirthDate.Year, value, patient.BirthDate.Day);
                    }
                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthMonth);
                }
            }
        }

        public int BirthDay
        {
            get
            {
                if (patient.OnlyBirthYear)
                    return 0;
                return patient.BirthDate.Day;
            }
            set
            {
                if (patient.BirthDate.Day != value && value >= 1 && value <= 31)
                {
                    patient.BirthDate = new DateTime(patient.BirthDate.Year, patient.BirthDate.Month, value);

                    OnPropertyChanged(() => Age);
                    OnPropertyChanged(() => BirthDay);
                }
            }
        }

        public bool IsMale
        {
            get
            {
                return patient.IsMale;
            }
            set
            {
                if (patient.IsMale != value)
                {
                    patient.IsMale = value;
                    OnPropertyChanged(() => IsMale);
                }
            }
        }

        public string ShortName
        {
            get
            {
                return LastName + (FirstName.Length > 0 ? " " + FirstName[0] + "." + (MiddleName.Length > 0 ? " " + MiddleName[0] + "." : "") : "");
            }
        }

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        #region CheckableBase

        public override bool IsReady
        {
            get
            {
                return base.IsReady && !IsSearchActive;
            }
        }

        public override string Name
        {
            get
            {
                return ShortName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnCheckedChanged()
        {

        }

        #endregion CheckableBase

        #region ISearchable

        private ICommand _searchCommand;
        private bool _searchActive;
        private bool _searchFocused;

        public string Representation
        {
            get
            {
                return ShortName;
            }
        }

        public bool IsSearchActive
        {
            get
            {
                return _searchActive;
            }
            set
            {
                if (_searchActive != value && (IsReady || !value))
                {
                    _searchActive = value;
                    OnPropertyChanged(() => IsSearchActive);
                }
            }
        }

        public bool IsSearchFocused
        {
            get
            {
                return _searchFocused;
            }
            set
            {
                if (_searchFocused != value)
                {
                    _searchFocused = value;
                    OnPropertyChanged(() => IsSearchFocused);
                }
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand
                    ?? (_searchCommand = new RelayCommand(
                                          () =>
                                          {
                                              IsSearchActive = !IsSearchActive;
                                          }
                                          ));
            }
        }

        #endregion ISearchable

        public PatientViewModel(Patient p)
        {
            Contract.Requires(p != null);
            patient = p;

            Symptoms = new ObservableCollection<SymptomViewModel>();
            msgHandlers = Subscribe();
        }

        private List<EventMessageHandler> Subscribe()
        {
            return new List<EventMessageHandler>()
            {
                this.Subscribe((int)EventID.SymptomCheckedChanged, (e) =>
                {
                    var symptom = e.GetValue<SymptomViewModel>(Messages.Symptom);
                    var isChecked = e.GetValue<bool>(Messages.CheckedState);

                    OnSymptomCheckedChanged(symptom, isChecked);
                })
            };
        }

        private void OnSymptomCheckedChanged(SymptomViewModel symptom, bool isChecked)
        {
            if (isChecked)
            {
                Symptoms.Add(symptom);
            }
            else
            {
                Symptoms.Remove(symptom);
            }
            Symptoms = new ObservableCollection<SymptomViewModel>(Symptoms.OrderBy(s => s.SortingOrder));
            OnPropertyChanged(() => Symptoms);
        }

        ~PatientViewModel()
        {
            foreach (var h in msgHandlers)
            {
                h.Dispose();
            }
        }
    }
}
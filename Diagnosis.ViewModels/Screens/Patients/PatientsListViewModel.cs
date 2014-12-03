using System.Linq;
using Diagnosis.Common;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using Diagnosis.Data;
using Diagnosis.ViewModels.Search;
using NHibernate;
using Diagnosis.Data.Queries;
using System.Windows.Data;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientsListViewModel : ScreenBase
    {
        private Patient _current;
        private FilterViewModel<Patient> _filter;
        private ObservableCollection<Patient> _patients;

        public PatientsListViewModel()
        {
            _filter = new FilterViewModel<Patient>(PatientQuery.StartingWith(Session));

            _filter.Results.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (Patient item in e.OldItems)
                    {
                        var deleted = Patients.Where(w => w == item).ToList();
                        deleted.ForEach((w) => Patients.Remove(w));
                    }
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    foreach (Patient item in e.NewItems)
                    {
                        Patients.Add(item);
                    }
            };
            _filter.Clear(); // показываем всех

            SelectLastPatient();

            Title = "Пациенты";
        }

        public ObservableCollection<Patient> Patients
        {
            get
            {
                if (_patients == null)
                {
                    _patients = new ObservableCollection<Patient>();
                    var patientsView = (CollectionView)CollectionViewSource.GetDefaultView(_patients);
                    SortDescription sort1 = new SortDescription("LastName", ListSortDirection.Ascending);
                    SortDescription sort2 = new SortDescription("FirstName", ListSortDirection.Ascending);
                    SortDescription sort3 = new SortDescription("MiddleName", ListSortDirection.Ascending);
                    patientsView.SortDescriptions.Add(sort1);
                    patientsView.SortDescriptions.Add(sort2);
                    patientsView.SortDescriptions.Add(sort3);
                }
                return _patients;
            }
        }

        public FilterViewModel<Patient> Filter { get { return _filter; } }

        public Patient SelectedPatient
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
                return new RelayCommand(() =>
                {
                    this.Send(Events.CreatePatient);
                });
            }
        }

        public ICommand OpenPatientCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Events.OpenPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                        }, () => SelectedPatient != null);
            }
        }
        public ICommand EditPatientCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EditPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                }, () => SelectedPatient != null);
            }
        }
        public void SelectLastPatient()
        {
            if (Patients.Count > 0)
            {
                SelectedPatient = Patients[0];
            }
        }

    }
}
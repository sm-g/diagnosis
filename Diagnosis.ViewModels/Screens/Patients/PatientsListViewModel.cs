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

namespace Diagnosis.ViewModels.Screens
{
    public class PatientsListViewModel : ScreenBase
    {
        private Patient _current;
        private NewFilterViewModel<Patient> _filter;
        private ObservableCollection<Patient> _patients;

        public PatientsListViewModel()
        {
            _patients = new ObservableCollection<Patient>();

            _filter = new NewFilterViewModel<Patient>(PatientQuery.StartingWith(Session));

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

        public ObservableCollection<Patient> Patients { get { return _patients; } }

        public NewFilterViewModel<Patient> Filter { get { return _filter; } }

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
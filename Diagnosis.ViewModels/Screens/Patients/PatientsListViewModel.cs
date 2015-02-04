﻿using System.Linq;
using Diagnosis.Common;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Diagnosis.Data.Specs;
using Diagnosis.Models;
using Diagnosis.Data;
using Diagnosis.ViewModels.Search;
using NHibernate;
using NHibernate.Linq;
using Diagnosis.Data.Queries;
using System.Windows.Data;
using System.ComponentModel;

namespace Diagnosis.ViewModels.Screens
{
    public class PatientsListViewModel : ScreenBaseViewModel
    {
        private Patient _current;
        private bool _noPatients;
        EventMessageHandlersManager emhManager;
        private FilterViewModel<Patient> _filter;
        private bool _focused;
        private ObservableCollection<Patient> _patients;
        private Saver saver;
        private ListCollectionView view;

        public PatientsListViewModel()
        {

            _filter = new FilterViewModel<Patient>(PatientQuery.StartingWith(Session));
            saver = new Saver(Session);
            SelectedPatients = new ObservableCollection<Patient>();

            Filter.Filtered += (s, e) =>
            {
                Patients.SyncWith(Filter.Results);
            };

            Title = "Пациенты";
            NoPatients = !Session.Query<Patient>().Any();

            emhManager = new EventMessageHandlersManager(new[] {
                this.Subscribe(Event.PatientSaved, (e) =>
                {
                    // выбираем нового пациента или изменившегося с учетом фильтра
                    Filter.Filter();
                    // TODO filter
                    SelectedPatient = e.GetValue<Patient>(MessageKeys.Patient);
                    NoPatients = false;
                })
            });

            Filter.Clear(); // показываем всех
            SelectLastPatient();
        }

        public ObservableCollection<Patient> Patients
        {
            get
            {
                if (_patients == null)
                {
                    _patients = new ObservableCollection<Patient>();
                    view = (ListCollectionView)CollectionViewSource.GetDefaultView(_patients);
                    SortDescription sort0 = new SortDescription("LastHrUpdatedAt", ListSortDirection.Descending);
                    SortDescription sort1 = new SortDescription("LastName", ListSortDirection.Ascending);
                    SortDescription sort2 = new SortDescription("FirstName", ListSortDirection.Ascending);
                    SortDescription sort3 = new SortDescription("MiddleName", ListSortDirection.Ascending);
                    view.SortDescriptions.Add(sort0);
                    view.SortDescriptions.Add(sort1);
                    view.SortDescriptions.Add(sort2);
                    view.SortDescriptions.Add(sort3);
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

        public ObservableCollection<Patient> SelectedPatients { get; private set; }

        public ICommand AddPatientCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.CreatePatient);
                });
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var toDel = SelectedPatients
                        .Where(p => p.IsEmpty())
                        .ToArray();
                    saver.Delete(toDel);

                    // убираем удаленных из списка
                    Filter.Filter();

                    // оставляем выделение тех, кто не удаляется
                    //toDel.Where(p => Patients.Contains(p))
                    //    .ForEach(p => SelectedPatients.Add(p));

                    NoPatients = !Session.Query<Patient>().Any();
                }, () => SelectedPatients.Any(p => p.IsEmpty()));
            }
        }
        public ICommand OpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                        {
                            this.Send(Event.OpenPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                        }, () => SelectedPatient != null);
            }
        }
        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditPatient, SelectedPatient.AsParams(MessageKeys.Patient));
                }, () => SelectedPatient != null);
            }
        }

        /// <summary>
        /// В БД нет пациентов.
        /// </summary>
        public bool NoPatients
        {
            get
            {
                return _noPatients;
            }
            set
            {
                if (_noPatients != value)
                {
                    _noPatients = value;
                    OnPropertyChanged(() => NoPatients);
                }
            }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    OnPropertyChanged(() => IsFocused);
                }
            }
        }
        public void SelectLastPatient()
        {
            if (Patients.Count > 0)
            {
                SelectedPatient = (Patient)view.GetItemAt(0);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                emhManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
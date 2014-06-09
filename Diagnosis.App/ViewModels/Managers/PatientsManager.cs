﻿using Diagnosis.App.Messaging;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class PatientsManager : ViewModelBase
    {
        private PopupSearch<PatientViewModel> _search;
        private PatientViewModel _current;
        private ICommand _addPatient;
        private IPatientRepository patientRepo;

        public ObservableCollection<PatientViewModel> Patients { get; private set; }

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

        public PopupSearch<PatientViewModel> Search
        {
            get
            {
                if (_search == null)
                {
                    _search = new PopupSearch<PatientViewModel>(new PatientSearcher(Patients));
                    _search.ResultItemSelected += _search_ResultItemSelected;
                }
                return _search;
            }
        }

        public ICommand AddPatientCommand
        {
            get
            {
                return _addPatient ?? (_addPatient = new RelayCommand(AddPatient));
            }
        }

        public PatientViewModel GetByModel(Patient patient)
        {
            return Patients.Where(p => p.patient == patient).FirstOrDefault();
        }

        public PatientsManager(IPatientRepository patientRepo)
        {
            Contract.Requires(patientRepo != null);
            this.patientRepo = patientRepo;

            var patientVMs = patientRepo.GetAll().Select(p => new PatientViewModel(p)).ToList();
            foreach (var pvm in patientVMs)
            {
                SubscribeEditable(pvm);
            }
            patientVMs.Sort(PatientViewModel.CompareByFullName);
            Patients = new ObservableCollection<PatientViewModel>(patientVMs);

            this.Subscribe((int)EventID.OpenedPatientChanged, (e) =>
            {
                var pat = e.GetValue<PatientViewModel>(Messages.Patient);
                SelectedPatient = pat;
            });
        }

        private void AddPatient()
        {
            var newPatientVM = new UnsavedPatientViewModel();

            this.Send((int)EventID.PatientAdded, new PatientParams(newPatientVM).Params);

            newPatientVM.PatientCreated += OnPatientCreated;
        }

        private void OnPatientCreated(object s, PatientEventArgs e)
        {
            var unsaved = e.patientVM as UnsavedPatientViewModel;
            unsaved.PatientCreated -= OnPatientCreated;

            patientRepo.SaveOrUpdate(unsaved.patient);
            var modelFromRepo = patientRepo.GetById(unsaved.patient.Id);

            var saved = new PatientViewModel(modelFromRepo);
            saved.CanAddFirstHr = !e.addFirstHr;
            Patients.Add(saved);

            this.Send((int)EventID.PatientCreated, new PatientParams(saved).Params);

            SubscribeEditable(saved);
            if (e.addFirstHr)
            {
                saved.Editable.IsEditorActive = false;
                //OpenLastAppointment(saved);
            }
        }

        private void SubscribeEditable(PatientViewModel pvm)
        {
            pvm.Editable.Committed += p_Committed;
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

        private void p_Committed(object sender, EditableEventArgs e)
        {
            var patientVM = e.viewModel as PatientViewModel;
            if (patientVM != null)
            {
                patientRepo.SaveOrUpdate(patientVM.patient);
            }
        }
    }
}
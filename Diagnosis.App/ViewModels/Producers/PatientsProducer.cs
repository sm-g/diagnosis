using Diagnosis.App.Messaging;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using Diagnosis.Core;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Управляет всеми пациентами.
    /// </summary>
    public class PatientsProducer
    {
        private ICommand _addPatient;
        private IPatientRepository patientRepo;

        public ObservableCollection<PatientViewModel> Patients { get; private set; }

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

        public PatientsProducer(IPatientRepository patientRepo)
        {
            Contract.Requires(patientRepo != null);
            this.patientRepo = patientRepo;

            var patientVMs = patientRepo.GetAll()
                .OrderBy(p => p.FullName, new EmptyStringsAreLast())
                .Select(p => new PatientViewModel(p))
                .ToList();
            foreach (var pvm in patientVMs)
            {
                SubscribeEditable(pvm);
            }
            Patients = new ObservableCollection<PatientViewModel>(patientVMs);
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
            saved.CanAddFirstHr = !e.addFirstHr; // больше нельзя добавлять первую запись
            Patients.Add(saved);

            this.Send((int)EventID.PatientCreated, new PatientParams(saved).Params);

            SubscribeEditable(saved);
            if (e.addFirstHr)
            {
                // переходим к созданной записи
                saved.Editable.IsEditorActive = false;
            }
        }

        private void SubscribeEditable(PatientViewModel pvm)
        {
            pvm.Editable.Committed += p_Committed;
        }

        private void p_Committed(object sender, EditableEventArgs e)
        {
            var patient = e.entity as Patient;
            patientRepo.SaveOrUpdate(patient);
        }
    }
}
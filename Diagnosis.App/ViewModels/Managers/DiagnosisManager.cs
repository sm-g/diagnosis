using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Содержит ViewModels всех диагнозов.
    /// </summary>
    public class DiagnosisManager
    {
        private IDiagnosisRepository repository;

        public ObservableCollection<DiagnosisViewModel> Diagnoses
        {
            get;
            private set;
        }

        public DiagnosisViewModel GetHealthRecordDiagnosis(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            if (Diagnoses.Count > 0)
            {
                return Diagnoses[0].Parent.AllChildren.Where(d => d.diagnosis == hr.Diagnosis).SingleOrDefault();
            }

            return null;
        }

        public void Check(DiagnosisViewModel diagnosis)
        {
            Contract.Requires(diagnosis != null);
            UnCheckAll();
            diagnosis.IsChecked = true;
        }

        public DiagnosisManager(IDiagnosisRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var allDiagnoses = repository.GetAll().Select(d => new DiagnosisViewModel(d)).ToList();

            foreach (var item in allDiagnoses)
            {
                item.Add(allDiagnoses.Where(d => d.diagnosis.Parent == item.diagnosis));
            }

            var root = new DiagnosisViewModel("root");
            root.Add(allDiagnoses.Where(d => d.IsRoot));

            root.Initialize();

            Diagnoses = new ObservableCollection<DiagnosisViewModel>(root.Children);

            this.Subscribe((int)EventID.DirectoryEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                foreach (var item in Diagnoses)
                {
                    item.Editable.SwitchedOn = isEditing;
                }

                UnCheckAll();
            });
        }

        private void UnCheckAll()
        {
            Contract.Assume(Diagnoses.Count > 0);

            foreach (var item in Diagnoses[0].Parent.AllChildren)
            {
                item.IsChecked = false;
            }
        }
    }
}
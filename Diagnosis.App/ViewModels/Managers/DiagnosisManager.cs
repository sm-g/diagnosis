using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Содержит ViewModels всех диагнозов.
    /// </summary>
    public class DiagnosisManager : ViewModelBase
    {
        private IcdChapterRepository repository;
        ObservableCollection<DiagnosisViewModel> _diagnoses;
        DiagnosisFiltratingSearcher _diagnosisSearcher;

        public ObservableCollection<DiagnosisViewModel> Diagnoses
        {
            get { return _diagnoses; }
            private set
            {
                _diagnoses = value;
                OnPropertyChanged(() => Diagnoses);
            }
        }

        public DiagnosisFiltratingSearcher FiltratingSearcher
        {
            get
            {
                return _diagnosisSearcher ?? (_diagnosisSearcher =
                  new DiagnosisFiltratingSearcher(EntityManagers.DiagnosisManager.Diagnoses[0].Parent));
            }
        }

        public DiagnosisViewModel GetHealthRecordDiagnosis(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            if (hr.Disease == null)
                return null;

            if (Diagnoses.Count > 0)
            {
                return Diagnoses[0].Parent.AllChildren.Where(d => d.diagnosis.Code == hr.Disease.Code).SingleOrDefault();
            }

            return null;
        }

        public void Check(DiagnosisViewModel diagnosis)
        {
            Contract.Requires(diagnosis != null);
            UnCheckAll();
            diagnosis.IsChecked = true;
        }

        public DiagnosisManager(IcdChapterRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var chapters = repository.GetAll().ToList();
            var chapterDiagnoses = chapters.Select(ch =>
                new Diagnosis.Models.Diagnosis(ch.Code, ch.Title)).ToList();
            var chapterVms = chapterDiagnoses.Select(ch => new DiagnosisViewModel(ch)).ToList();

            SetDiagnosesForDoctor(chapterVms, EntityManagers.DoctorsManager.CurrentDoctor);


            this.Subscribe((int)EventID.CurrentDoctorChanged, (e) =>
            {
                var doctorVM = e.GetValue<DoctorViewModel>(Messages.Doctor);

                SetDiagnosesForDoctor(chapterVms, doctorVM);
            });

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnDirectoryEditingModeChanged(isEditing);
            });
        }

        private void SetDiagnosesForDoctor(
            IEnumerable<DiagnosisViewModel> chapterVms,
            DoctorViewModel doctorVM)
        {
            var blocks = doctorVM.doctor.Speciality.IcdBlocks;
            var blockDiagnoses = blocks.Select(b =>
                new Diagnosis.Models.Diagnosis(b.Code, b.Title, chapterVms.Select(ch => ch.diagnosis).Where(ch =>
                    ch.Code == b.IcdChapter.Code).SingleOrDefault())).ToList();
            var blockVms = blockDiagnoses.Select(b => new DiagnosisViewModel(b)).ToList();

            foreach (var item in chapterVms)
            {
                item.Add(blockVms.Where(b => b.diagnosis.Parent == item.diagnosis));
            }

            var diseases = blocks.SelectMany(b => b.IcdDiseases).ToList(); ;
            var diseaseDiagnoses = diseases.Select(d =>
                new Diagnosis.Models.Diagnosis(d.Code, d.Title, blockDiagnoses.Where(b =>
                    b.Code == d.IcdBlock.Code).SingleOrDefault())).ToList();
            var diseaseVms = diseaseDiagnoses.Select(d => new DiagnosisViewModel(d)).ToList();

            foreach (var item in blockVms)
            {
                item.Add(diseaseVms.Where(d => d.diagnosis.Parent == item.diagnosis));
            }

            chapterVms = chapterVms.Where(ch => ch.AllChildren.Count() > ch.Children.Count);
            foreach (var item in chapterVms)
            {
                item.Remove(blockVms.Where(b => b.Children.Count == 0));
            }
            var dia = new Diagnosis.Models.Diagnosis("code", "root");
            var root = new DiagnosisViewModel(dia);
            root.Add(chapterVms);
            root.Initialize();

            Diagnoses = new ObservableCollection<DiagnosisViewModel>(root.Children);
        }

        private void OnDirectoryEditingModeChanged(bool isEditing)
        {
            Diagnoses.ForBranch((dvm) =>
            {
                dvm.Editable.SwitchedOn = isEditing;
                dvm.Search.SwitchedOn = !isEditing;
            });

            UnCheckAll();
        }

        private void UnCheckAll()
        {
            Diagnoses.ForBranch((dvm) => dvm.IsChecked = false);
        }
    }
}
using Diagnosis.App.Messaging;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class DiagnosisProducer : ViewModelBase
    {
        private readonly IcdChapterRepository repository;
        private DiagnosisViewModel _root;
        private ObservableCollection<DiagnosisViewModel> _diagnoses;
        private DiagnosisFiltratingSearcher _diaFiltratingSearcher;
        private DiagnosisSearcher _diaRootSearcher;

        public event EventHandler RootChanged;

        public DiagnosisViewModel Root
        {
            get
            {
                return _root;
            }
            private set
            {
                if (_root != value)
                {
                    _root = value;
                    _diaFiltratingSearcher = new DiagnosisFiltratingSearcher(_root);
                    _diaRootSearcher = new DiagnosisSearcher(_root, new SimpleSearcherSettings() { WithChecked = true, AllChildren = true });

                    OnPropertyChanged("RootSearcher");
                    OnPropertyChanged("RootFiltratingSearcher");
                    OnRootChanged();
                }
            }
        }

        public ObservableCollection<DiagnosisViewModel> Diagnoses
        {
            get { return _diagnoses; }
            private set
            {
                _diagnoses = value;
                OnPropertyChanged("Diagnoses");
            }
        }

        /// <summary>
        /// Поисковик по всем диагнозам, кроме групп.
        /// </summary>
        public DiagnosisSearcher RootSearcher
        {
            get
            {
                return _diaRootSearcher;
            }
        }

        /// <summary>
        /// Поисковик по всем диагнозам, кроме групп. Изменяет значение IsFiltered.
        /// </summary>
        public DiagnosisFiltratingSearcher RootFiltratingSearcher
        {
            get
            {
                return _diaFiltratingSearcher;
            }
        }

        public DiagnosisViewModel GetHealthRecordDiagnosis(HealthRecord hr)
        {
            Contract.Requires(hr != null);
            if (hr.Disease == null)
                return null;

            if (Diagnoses.Count > 0)
            {
                return Root.AllChildren.Where(d => d.diagnosis.Code == hr.Disease.Code).SingleOrDefault();
            }

            return null;
        }

        public void Check(DiagnosisViewModel diagnosis)
        {
            UnCheckAll();
            if (diagnosis != null)
            {
                diagnosis.IsChecked = true;
            }
        }

        public DiagnosisProducer(IcdChapterRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var chapters = repository.GetAll().ToList();
            var chapterDiagnoses = chapters.Select(ch =>
                new Diagnosis.Models.Diagnosis(ch.Code, ch.Title)).ToList();
            var chapterVms = chapterDiagnoses.Select(ch => new DiagnosisViewModel(ch, RootChanged)).ToList();

            SetDiagnosesForDoctor(chapterVms, EntityProducers.DoctorsProducer.CurrentDoctor.doctor);

            Subscribe(chapterVms);
        }

        private void Subscribe(List<DiagnosisViewModel> chapterVms)
        {
            this.Subscribe((int)EventID.CurrentDoctorChanged, (e) =>
            {
                var doctor = e.GetValue<Doctor>(Messages.Doctor);

                SetDiagnosesForDoctor(chapterVms, doctor);
            });

            this.Subscribe((int)EventID.WordsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnDirectoryEditingModeChanged(isEditing);
            });

            this.Subscribe((int)EventID.DiagnosisCheckedChanged, (e) =>
            {
                var diagnosis = e.GetValue<DiagnosisViewModel>(Messages.Diagnosis);

                OnDiagnosisCheckedChanged(diagnosis, diagnosis.IsChecked);
            });
        }

        private void OnDiagnosisCheckedChanged(DiagnosisViewModel diagnosis, bool isChecked)
        {
            // снимаем флаг со всех диагнозов, кроме отмеченного
            if (isChecked)
                Diagnoses.ForBranch(d =>
                {
                    if (d.IsChecked && d != diagnosis)
                        d.IsChecked = false;
                });
        }

        /// <summary>
        /// Set Diagnoses collection in accordance to doctor's speciality.
        /// </summary>
        private void SetDiagnosesForDoctor(
            IEnumerable<DiagnosisViewModel> chapterVms,
            Doctor doctor)
        {
            // создаем диагнозы-блоки
            var blocks = doctor.Speciality.IcdBlocks; // блоки для специальности доктора
            var blockDiagnoses = blocks.Select(b =>
                new Diagnosis.Models.Diagnosis(b.Code, b.Title, chapterVms.Select(ch => ch.diagnosis).Where(ch =>
                    ch.Code == b.IcdChapter.Code).SingleOrDefault())).ToList();
            var blockVms = blockDiagnoses.Select(b => new DiagnosisViewModel(b, RootChanged)).ToList();

            // добавляем нужные блоки в классы
            foreach (var ch in chapterVms)
            {
                ch.ClearChildren();
                ch.Add(blockVms.Where(b => b.diagnosis.Parent == ch.diagnosis));
            }

            Func<IcdDisease, bool> whereClause = d => true;
            if (doctor.DoctorSettings.HasFlag(DoctorSettings.OnlyTopLevelIcdDisease))
            {
                // без уточненных болезней
                whereClause = d => d.Code.IndexOf('.') == -1;
            }

            // создаем диагнозы-болезни
            var diseases = blocks.SelectMany(b => b.IcdDiseases).Where(whereClause).ToList();
            var diseaseDiagnoses = diseases.Select(d =>
                new Diagnosis.Models.Diagnosis(d.Code, d.Title, blockDiagnoses.Where(b =>
                    b.Code == d.IcdBlock.Code).SingleOrDefault(), d)).ToList();
            var diseaseVms = diseaseDiagnoses.Select(d => new DiagnosisViewModel(d, RootChanged)).ToList();

            // добавляем нужные болезни в блоки
            foreach (var item in blockVms)
            {
                item.Add(diseaseVms.Where(d => d.diagnosis.Parent == item.diagnosis));
            }

            // убираем классы, в которых блоки без болезней (не нужны специальности доктора)
            chapterVms = chapterVms.Where(ch => ch.AllChildren.Count() > ch.Children.Count);
            foreach (var item in chapterVms)
            {
                item.Remove(blockVms.Where(b => b.Children.Count == 0));
            }

            var dia = new Diagnosis.Models.Diagnosis("code", "root");
            var root = new DiagnosisViewModel(dia, RootChanged);
            Root = root.Add(chapterVms);
            // IEnumarable

            if (Diagnoses != null)
                Diagnoses.ForBranch(dvm => dvm.Unsubscribe());
            Diagnoses = new ObservableCollection<DiagnosisViewModel>(Root.Children);
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

        protected virtual void OnRootChanged()
        {
            var h = RootChanged;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }
    }
}
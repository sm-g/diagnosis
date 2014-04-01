using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Содержит ViewModels всех симптомов.
    /// </summary>
    public class SymptomsManager : ViewModelBase
    {
        private ISymptomRepository repository;
        private RelayCommand _commit;

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        private IEnumerable<SymptomViewModel> DirtySymptoms
        {
            get
            {
                return Symptoms.Where(s => s.Editable.IsDirty);
            }
        }

        public ICommand CommitCommand
        {
            get
            {
                return _commit
                    ?? (_commit = new RelayCommand(
                                          () =>
                                          {
                                              foreach (var item in DirtySymptoms)
                                              {
                                                  if (item.IsChecked)
                                                      item.Editable.CommitCommand.Execute(null);
                                              }
                                          },
                                          () => DirtySymptoms.Count() > 0));
            }
        }

        public IEnumerable<SymptomViewModel> GetHealthRecordSymptoms(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            if (Symptoms.Count > 0)
            {
                var intersect = Symptoms[0].Parent.AllChildren.Select(s => s.symptom).Intersect(hr.Symptoms);

                return Symptoms[0].Parent.AllChildren.Where(s => intersect.Contains(s.symptom));
            }

            return Enumerable.Empty<SymptomViewModel>();
        }

        public void CheckThese(IEnumerable<SymptomViewModel> symptoms)
        {
            UnCheckAll();

            foreach (var item in symptoms)
            {
                item.IsChecked = true;
            }
        }

        public SymptomViewModel Create(string title)
        {
            var vm = new SymptomViewModel(new Symptom(title));
            vm.Editable.MarkDirty();
            Subscribe(vm);
            return vm;
        }

        public SymptomsManager(ISymptomRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var allSymptoms = repository.GetAll().Select(s => new SymptomViewModel(s)).ToList();

            foreach (var item in allSymptoms)
            {
                Subscribe(item);
            }

            var root = new SymptomViewModel("root") { IsNonCheckable = true };
            root.Add(allSymptoms);
            root.Initialize();

            Symptoms = new ObservableCollection<SymptomViewModel>(root.Children);

            this.Subscribe((int)EventID.SymptomsEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnDirectoryEditingModeChanged(isEditing);
            });
        }

        private void Subscribe(SymptomViewModel item)
        {
            item.Editable.Committed += (s, e) =>
            {
                repository.SaveOrUpdate((e.viewModel as SymptomViewModel).symptom);
            };
        }

        private void OnDirectoryEditingModeChanged(bool isEditing)
        {
            Symptoms.ForAll((svm) =>
            {
                svm.Editable.SwitchedOn = isEditing;
                svm.Search.SwitchedOn = !isEditing;
            });

            UnCheckAll();
        }

        private void UnCheckAll()
        {
            Symptoms.ForAll((svm) => svm.IsChecked = false);
        }
    }
}
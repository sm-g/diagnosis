﻿using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Содержит ViewModels всех симптомов.
    /// </summary>
    public class SymptomsManager
    {
        private ISymptomRepository repository;

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
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
            return vm;
        }

        public SymptomsManager(ISymptomRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var allSymptoms = repository.GetAll().Select(s => new SymptomViewModel(s));
            var root = new SymptomViewModel("root");
            root.Add(new SymptomViewModel("Симптомы") { IsNonCheckable = true }.Add(allSymptoms.Take(3)));
            root.Add(new SymptomViewModel("Условия") { IsNonCheckable = true }.Add(allSymptoms.Skip(3)));
            root.Initialize();

            Symptoms = new ObservableCollection<SymptomViewModel>(root.Children);

            this.Subscribe((int)EventID.DirectoryEditingModeChanged, (e) =>
            {
                var isEditing = e.GetValue<bool>(Messages.Boolean);

                OnDirectoryEditingModeChanged(isEditing);
            });
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
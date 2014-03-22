using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Diagnosis.Models;
using Diagnosis.Data.Repositories;
using System.Diagnostics.Contracts;

namespace Diagnosis.App.ViewModels
{
    /// <summary>
    /// Содержит ViewModels всех симптомов.
    /// </summary>
    public class SymptomsManager
    {
        ISymptomRepository repository;

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        public IEnumerable<SymptomViewModel> GetHealthRecordSymptoms(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            var intersect = Symptoms.Select(s => s.symptom).Intersect(hr.Symptoms);

            return Symptoms.Where(s => intersect.Contains(s.symptom));
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
        }
    }
}

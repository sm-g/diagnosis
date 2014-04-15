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
    public class SymptomsManager : ViewModelBase
    {
        private ISymptomRepository repository;

        public ObservableCollection<SymptomViewModel> Symptoms
        {
            get;
            private set;
        }

        public SymptomViewModel GetSymptomForWords(IEnumerable<WordViewModel> words)
        {
            var comparator = new CompareWord();
            var existing = Symptoms.FirstOrDefault(
                s => s.Words.OrderBy(w => w.word, comparator).SequenceEqual(
                       words.OrderBy(w => w.word, comparator)));
            if (existing != null)
                return existing;

            var sym = new Symptom(words.Select(w => w.word));
            var svm = new SymptomViewModel(sym);
            Symptoms.Add(svm);

            return svm;
        }

        public SymptomsManager(ISymptomRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var all = repository.GetAll().Select(s => new SymptomViewModel(s)).ToList();


            Symptoms = new ObservableCollection<SymptomViewModel>(all);

        }
    }
}
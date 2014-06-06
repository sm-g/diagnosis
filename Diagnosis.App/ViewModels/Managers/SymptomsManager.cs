using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

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

        /// <summary>
        /// Возвращает симптомы, содержащие указанные слова.
        /// </summary>
        public IEnumerable<SymptomViewModel> GetSymptomsWithWords(IEnumerable<WordViewModel> words)
        {
            Contract.Requires(words != null);

            return Symptoms.Where(
                s => words.IsSubsetOf(s.Words));
        }
        /// <summary>
        /// Создает симптом и добавляет в коллекцию Symptoms, если его там ещё нет.
        /// </summary>
        public SymptomViewModel Create(IEnumerable<WordViewModel> words)
        {
            var existing = GetSymptomForWords(words);
            if (existing != null)
                return existing;

            var sym = new Symptom(words.Select(w => w.word));
            var svm = new SymptomViewModel(sym);
            svm.Editable.MarkDirty();

            Symptoms.Add(svm);
            System.Console.WriteLine("new symptom: {0}", svm);
            return svm;
        }
        /// <summary>
        /// Возвращает симптом, содержащий только указанные слова.
        /// </summary>
        public SymptomViewModel GetSymptomForWords(IEnumerable<WordViewModel> words)
        {
            Contract.Requires(words != null);

            var comparator = new CompareWord();
            return Symptoms.FirstOrDefault(
                s => s.Words.OrderBy(w => w.word, comparator).SequenceEqual(
                       words.OrderBy(w => w.word, comparator)));
        }
        public void WipeUnsaved()
        {
            var toRemove = Symptoms.Where(sym => sym.Unsaved).ToList();
            toRemove.ForAll((sym) => Symptoms.Remove(sym));
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
using Diagnosis.Core;
using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SymptomsManager
    {
        private ISymptomRepository repository;
        private List<SymptomViewModel> symptoms;

        /// <summary>
        /// Возвращает симптомы, содержащие указанные слова.
        /// </summary>
        public IEnumerable<SymptomViewModel> GetSymptomsWithWords(IEnumerable<WordViewModel> words)
        {
            Contract.Requires(words != null);

            return symptoms.Where(
                s => words.IsSubsetOf(s.Words));
        }

        /// <summary>
        /// Создает симптом и добавляет в коллекцию Symptoms, если его там ещё нет.
        /// Возвращает симптом, содержащий только указанные слова.
        /// </summary>
        public SymptomViewModel Create(IEnumerable<WordViewModel> words)
        {
            var existing = GetSymptomForWords(words);
            if (existing != null)
                return existing;

            var sym = new Symptom(words.Select(w => w.word));
            var svm = new SymptomViewModel(sym);
            svm.Editable.MarkDirty();

            symptoms.Add(svm);
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
            return symptoms.FirstOrDefault(
                s => s.Words.OrderBy(w => w.word, comparator).SequenceEqual(
                       words.OrderBy(w => w.word, comparator)));
        }

        internal SymptomViewModel GetByModel(Symptom symptom)
        {
            return symptoms.Where(svm => svm.symptom == symptom).FirstOrDefault();
        }

        public void WipeUnsaved()
        {
            var toRemove = symptoms.Where(sym => sym.Unsaved).ToList();
            toRemove.ForAll((sym) => symptoms.Remove(sym));
        }

        public SymptomsManager(ISymptomRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var all = repository.GetAll().Select(s => new SymptomViewModel(s)).ToList();

            symptoms = new List<SymptomViewModel>(all);
        }
    }
}
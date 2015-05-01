using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.Models;

namespace Diagnosis.ViewModels.Autocomplete
{
    public static class Extensions
    {
        public static ConfindenceHrItemObject ToChio(this TagViewModel t)
        {
            Contract.Requires(t.BlankType != BlankType.None);
            return new ConfindenceHrItemObject(t.Blank, t.Confidence);
        }

        public static IEnumerable<Confindencable<Word>> GetCWords(this AutocompleteViewModel a)
        {
            return a.GetCHIOs()
                .Where(x => x.HIO is Word)
                .Select(x => new Confindencable<Word>(x.HIO as Word, x.Confidence));
        }
        /// <summary>
        /// Возвращает сущности из завершенных тегов по порядку.
        /// </summary>
        public static IEnumerable<ConfindenceHrItemObject> GetCHIOsOfCompleted(this AutocompleteViewModel a)
        {
            var completed = a.Tags.Where(t => t.State == State.Completed);

            var hios = completed
                 .Select(t => t.ToChio())
                 .ToList();
            return hios;
        }
    }
}

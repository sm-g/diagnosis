using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class SymptomSearch : SearchBase<SymptomViewModel>
    {
        public SymptomSearch(IEnumerable<SymptomViewModel> symptoms)
            : base(withCreatingNew: false)
        {
            Collection = symptoms;
            InitQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="query">titles of words, separated by comma</param>
        /// <returns></returns>
        protected override bool Filter(SymptomViewModel item, string query)
        {
            var words = query.Split(',')
                .Select(q => EntityManagers.WordsManager.Find(q));

            return words.IsSubsetOf(item.Words);
        }
    }
}
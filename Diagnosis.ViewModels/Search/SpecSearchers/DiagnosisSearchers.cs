using Diagnosis.Data.Specs;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Diag = Diagnosis.Models.Diagnosis;

namespace Diagnosis.ViewModels.Search
{
    public class NewDiagnosisSearcher : ISimpleSearcher<Diag>
    {
        readonly Diag parent;
        readonly NHibernateRepository<Diag> repo;
        public NewDiagnosisSearcher(Diag parent, NHibernateRepository<Diag> repo)
        {
            this.parent = parent;
            this.repo = repo;
        }
        /// <summary>
        /// Возвращает все диагнозы - детей родителя, у которых заголовок содержит строку или код начинается на строку.
        /// </summary>
        public IEnumerable<Diag> Search(string query)
        {
            return repo.FindAll(DiagSpecs.ChildrenOf(parent)
                & (
                    DiagSpecs.TitleContains(query)
                  | DiagSpecs.CodeStartsWith(query)));
        }
    }
}

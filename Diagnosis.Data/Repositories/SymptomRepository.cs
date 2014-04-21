using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System;

namespace Diagnosis.Data.Repositories
{
    public class SymptomRepository : ModelRepository<Symptom>, ISymptomRepository
    {
        public IEnumerable<Symptom> GetByWord(Word word)
        {
            throw new NotImplementedException();
            //ISession session = NHibernateHelper.GetSession();
            //{
            //    var symptoms = session
            //        .CreateCriteria<Symptom>()
            //        .CreateCriteria("Words")
            //        .Add(Expression.In("Id", word.Id))
            //        .List<Symptom>();
            //    return symptoms;
            //}
        }
    }
}

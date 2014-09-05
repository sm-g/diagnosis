using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Models;
using LinqSpecs;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace Diagnosis.Data.Specs
{
    class PatientsStartingWith : Specification<Patient>
    {
        private readonly string query;

        public PatientsStartingWith(string q)
        {
            query = q.ToLower();
        }

        public override Expression<Func<Patient, bool>> IsSatisfiedBy()
        {
            return m => m.FirstName.ToLower().IsLike(query + "%");// ||
            // m.MiddleName.ToLower().StartsWith(query) ||
            // m.LastName.ToLower().StartsWith(query);
        }
    }
    class PatientsContains : Specification<Patient>
    {
        private readonly string query;


        public PatientsContains(string q)
        {
            query = q.ToLower();
        }

        public override Expression<Func<Patient, bool>> IsSatisfiedBy()
        {
            return m => m.FullName.ToLower().Contains(query);
        }
    }
    public static class PatientSpecs
    {
        public static Specification<Patient> StartingWith(string q)
        {
            return new PatientsStartingWith(q);
        }
        public static Specification<Patient> Contains(string q)
        {
            return new PatientsContains(q);
        }
    }
}

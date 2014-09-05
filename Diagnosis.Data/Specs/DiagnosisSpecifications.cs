using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Models;
using LinqSpecs;
using System.Linq.Expressions;

using Diag = Diagnosis.Models.Diagnosis;

namespace Diagnosis.Data.Specs
{
    class DiagsTItleContains : Specification<Diag>
    {
        private readonly string query;

        public DiagsTItleContains(string q)
        {
            query = q.ToLower();
        }

        public override Expression<Func<Diag, bool>> IsSatisfiedBy()
        {
            return m => m.Title.ToLower().Contains(query);
        }
    }
    class DiagsChilrenOf : Specification<Diag>
    {
        private readonly Diag parent;

        public DiagsChilrenOf(Diag parent)
        {
            this.parent = parent;
        }

        public override Expression<Func<Diag, bool>> IsSatisfiedBy()
        {
            return m => m.Parent == parent;
        }
    }
    class DiagsCodeStartsWith : Specification<Diag>
    {
        private readonly string query;


        public DiagsCodeStartsWith(string q)
        {
            query = q.ToLower();
        }

        public override Expression<Func<Diag, bool>> IsSatisfiedBy()
        {
            return m => m.Code.StartsWith(query, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public static class DiagSpecs
    {
        public static Specification<Diag> TitleContains(string q)
        {
            return new DiagsTItleContains(q);
        }
        public static Specification<Diag> ChildrenOf(Diag p)
        {
            return new DiagsChilrenOf(p);
        }
        public static Specification<Diag> CodeStartsWith(string q)
        {
            return new DiagsCodeStartsWith(q);
        }
    }
}

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
    class WordsStartingWith : Specification<Word>
    {
        private readonly string query;

        public WordsStartingWith(string q)
        {
            query = q.ToLower();
        }

        public override Expression<Func<Word, bool>> IsSatisfiedBy()
        {
            return m => m.Title.StartsWith(query);
        }
    }
    class WordsChilrenOf : Specification<Word>
    {
        private readonly Word parent;

        public WordsChilrenOf(Word parent)
        {
            this.parent = parent;
        }

        public override Expression<Func<Word, bool>> IsSatisfiedBy()
        {
            return m => m.Parent == parent;
        }
    }
    public static class WordSpecs
    {
        public static Specification<Word> StartingWith(string q)
        {
            return new WordsStartingWith(q);
        }
        public static Specification<Word> ChildrenOf(Word p)
        {
            return new WordsChilrenOf(p);
        }
    }
}

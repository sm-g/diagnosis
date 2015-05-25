using Diagnosis.Common;
using Diagnosis.Data.Search;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Tests
{
    public static class OptExtensions
    {
        public static SearchOptions AddChild(this SearchOptions parent, Action<SearchOptions> onChild)
        {
            var child = new SearchOptions();
            parent.Children.Add(child);
            onChild(child);
            return parent;
        }

        public static SearchOptions All(this SearchOptions qb)
        {
            qb.GroupOperator = QueryGroupOperator.All;
            return qb;
        }

        public static SearchOptions Any(this SearchOptions qb)
        {
            qb.GroupOperator = QueryGroupOperator.Any;
            return qb;
        }

        public static SearchOptions NotAny(this SearchOptions qb)
        {
            qb.GroupOperator = QueryGroupOperator.NotAny;
            return qb;
        }

        public static SearchOptions WithConf(this SearchOptions qb)
        {
            qb.WithConf = true;
            return qb;
        }

        public static SearchOptions Scope(this SearchOptions qb, SearchScope scope)
        {
            qb.SearchScope = scope;
            return qb;
        }

        public static SearchOptions SetAll(this SearchOptions qb, params IDomainObject[] all)
        {
            qb.CWordsAll = new List<Confindencable<Word>>(
                all.OfType<Word>().Select(x => x.AsConfidencable())
                .Union(all.OfType<Confindencable<Word>>()));
            qb.MeasuresAll = new List<MeasureOp>(all.OfType<MeasureOp>());
            return qb;
        }

        public static SearchOptions SetAny(this SearchOptions qb, params IDomainObject[] any)
        {
            qb.CWordsAny = new List<Confindencable<Word>>(
                any.OfType<Word>().Select(x => x.AsConfidencable())
                .Union(any.OfType<Confindencable<Word>>()));
            qb.MeasuresAny = new List<MeasureOp>(any.OfType<MeasureOp>());
            return qb;
        }

        public static SearchOptions SetNot(this SearchOptions qb, params IDomainObject[] not)
        {
            qb.CWordsNot = new List<Confindencable<Word>>(
                not.OfType<Word>().Select(x => x.AsConfidencable())
                .Union(not.OfType<Confindencable<Word>>()));
            return qb;
        }

        public static SearchOptions MinAny(this SearchOptions qb, int min)
        {
            qb.MinAny = min;
            return qb;
        }

        public static SearchOptions Check(this SearchOptions qb, params HrCategory[] cats)
        {
            qb.Categories.AddRange(cats);
            return qb;
        }

        public static IEnumerable<HealthRecord> Search(this SearchOptions qb, ISession session)
        {
            return Searcher.GetResult(session, qb);
        }

        public static bool IsSuperSetOf(this IEnumerable<HealthRecord> s, params HealthRecord[] hrs)
        {
            return hrs.All(x => s.Contains(x));
        }

        public static bool NotContains(this IEnumerable<HealthRecord> s, params HealthRecord[] hrs)
        {
            return hrs.All(x => !s.Contains(x));
        }
    }
}
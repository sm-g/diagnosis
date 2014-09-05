using Diagnosis.Data.Repositories;
using Diagnosis.Data.Specs;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Queries
{
    public static class WordQuery
    {
        /// <summary>
        /// Возвращает все слова, которые начинаются на строку.
        /// </summary>
        public static Func<string, IEnumerable<Word>> StartingWith(ISession session)
        {
            return (str) =>
            {
                using (var tr = session.BeginTransaction())
                {
                    return session.QueryOver<Word>().WhereRestrictionOn(w => w.Title).IsLike(str, MatchMode.Start).List();
                }
            };
        }
        /// <summary>
        /// Возвращает все слова, которые начинаются на строку. 
        /// Если у слова есть родитель, вместо этого слова возвращается самый верхний предок слова.
        /// 
        /// Например, слова:
        /// сидя
        /// образование
        ///     высшее
        ///     среднее
        ///     
        /// Ищем «с», возвращаются «сидя, образование».
        /// 
        /// TODO withnoncheckable = true
        /// </summary>
        public static Func<string, IEnumerable<Word>> TopParentStartingWith(ISession session)
        {
            return (str) =>
            {
                var result = new List<Word>();
                var found = StartingWith(session).Invoke(str);
                foreach (var item in found)
                {
                    var w = item;
                    while (w.Parent.Parent != null) // пока не корень
                    {
                        w = w.Parent;
                    }
                    result.Add(w);
                }
                return result;
            };
        }
        /// <summary>
        /// Возвращает все слова, которые начинаются на строку.
        /// Сначала слова - дети родителя, потом остальные.
        /// </summary>
        public static Func<Word, string, IEnumerable<Word>> StartingWithChildrenFirst(ISession session)
        {
            return (parent, str) =>
            {
                var result = new List<Word>();
                var found = StartingWith(session).Invoke(str);

                result.AddRange(found.Where(w => w.Parent == parent));
                result.AddRange(found.Where(w => w.Parent != parent));
                return result;
            };
        }
    }
}
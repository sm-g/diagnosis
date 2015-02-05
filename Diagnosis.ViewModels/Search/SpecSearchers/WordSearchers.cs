using Diagnosis.Data.Specs;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels.Search
{
    public class NewWordSearcher : ISimpleSearcher<Word>
    {
        readonly NHibernateRepository<Word> repo;
        public NewWordSearcher(NHibernateRepository<Word> repo)
        {
            this.repo = repo;
        }
        /// <summary>
        /// Возвращает все слова, которые начинаются на строку.
        /// </summary>
        public IEnumerable<Word> Search(string query)
        {
            return repo.FindAll(WordSpecs.StartingWith(query));
        }
    }

    // добавить withnoncheckable = true
    public class NewWordTopParentSearcher : ISimpleSearcher<Word>
    {
        readonly NHibernateRepository<Word> repo;
        public NewWordTopParentSearcher(NHibernateRepository<Word> repo)
        {
            this.repo = repo;
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
        /// </summary>
        public IEnumerable<Word> Search(string query)
        {
            var found = repo.FindAll(WordSpecs.StartingWith(query));
            foreach (var item in found)
            {
                var w = item;
                while (w.Parent.Parent != null) // пока не корень
                {
                    w = w.Parent;
                }
                yield return w;
            }
        }
    }

    public class NewWordCompositeSearcher : ISimpleSearcher<Word>
    {
        readonly Word parent;
        readonly NHibernateRepository<Word> repo;
        public NewWordCompositeSearcher(Word parent, NHibernateRepository<Word> repo)
        {
            this.parent = parent;
            this.repo = repo;
        }
        /// <summary>
        /// Возвращает все слова, которые начинаются на строку.
        /// Сначала слова - дети родителя, потом остальные.
        /// </summary>
        public IEnumerable<Word> Search(string query)
        {
            var found = repo.FindAll(WordSpecs.StartingWith(query));
            var childrens = found.Where(w => w.Parent == parent);
            foreach (var item in childrens)
            {
                yield return item;
            }

            var others = found.Where(w => w.Parent != parent);
            foreach (var item in others)
            {
                yield return item;
            }
        }

    }

}

using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.ViewModels
{
    public static class Extensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Extensions));
        /// <summary>
        /// После вставки десериализованных hio создается другой объект,
        /// нужно использовать существующие сущности (Word, IcdDisease),
        /// Comment - valueobject, в Measure меняем Word, а Uom не может быть удален между копированием и вставкой
        /// </summary>
        /// <param name="hios"></param>
        /// <param name="session"></param>
        public static void SyncAfterPaste(this IList<ConfWithHio> hios, ISession session)
        {
            Func<Word, Word> syncWord = (word) =>
            {
                Word res = null;
                if (word.IsTransient)
                    // новое может быть в автокомплите
                    res = SuggestionsMaker.GetSameWordFromCreated(word);

                else  // не новое может быть в БД
                    using (var tr = session.BeginTransaction())
                        res = session.Get<Word>(word.Id);

                if (res == null)
                    // сохраненное после копирования будет в БД
                    res = WordQuery.ByTitle(session)(word.Title);

                if (res == null)
                {
                    logger.WarnFormat("Word not synced: {0}, recreate", word);
                    // сменился доктор - нет в автокомплите (почему?) 
                    // сохраненое было удалено из БД

                    // вставили запись/тег с этим словом - которого нет в БД и в created
                    res = new Word(word.Title);
                }
                return res;
            };

            for (int i = 0; i < hios.Count; i++)
            {
                if (hios[i].HIO is Word)
                {
                    hios[i].HIO = syncWord(hios[i].HIO as Word);
                }
                else if (hios[i].HIO is Measure)
                {
                    var m = hios[i].HIO as Measure;
                    if (m.Word != null)
                    {
                        m.Word = syncWord(m.Word);
                    }
                }
                else if (hios[i].HIO is IcdDisease)
                {
                    var icd = hios[i].HIO as IcdDisease;
                    using (var tr = session.BeginTransaction())
                    {
                        hios[i].HIO = session.Get<IcdDisease>(icd.Id); // МКБ точно есть в БД
                    }
                }
            }
        }

        /// <summary>
        /// Свойство для сортировки по колонке.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [Pure]
        public static string ToSortingProperty(this HrViewColumn col)
        {
            switch (col)
            {
                case HrViewColumn.Category:
                case HrViewColumn.CreatedAt:
                case HrViewColumn.DescribedAt:
                case HrViewColumn.Ord:
                    return col.ToString();

                case HrViewColumn.Date:
                    return "SortingDate";

                case HrViewColumn.None:
                default:
                    return null;
            }
        } /// <summary>
        /// Свойство для группировки по колонке.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        [Pure]
        public static string ToGroupingProperty(this HrViewColumn col)
        {
            switch (col)
            {
                case HrViewColumn.Category:
                    return col.ToString();

                case HrViewColumn.CreatedAt:
                    return "GroupingCreatedAt";

                case HrViewColumn.None:
                default:
                    return null;
            }
        }

        public static T FindHolderKeeperOf<T>(this IEnumerable<T> root, IHrsHolder holder)
            where T : HierarchicalBase<T>, IHolderKeeper
        {
            holder = holder.Actual as IHrsHolder;
            T vm;
            foreach (var item in root)
            {
                if (item.Holder == holder)
                    return item;
                vm = item.AllChildren.Where(x => x.Holder == holder).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }
        public static T FindCritKeeperOf<T>(this IEnumerable<T> root, ICrit crit)
           where T : HierarchicalBase<T>, ICritKeeper
        {
            crit = crit.Actual as ICrit;
            T vm;
            foreach (var item in root)
            {
                if (item.Crit == crit)
                    return item;
                vm = item.AllChildren.Where(x => x.Crit == crit).FirstOrDefault();
                if (vm != null)
                    return vm;
            }
            return null;
        }
    }
}

using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
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
        /// 
        /// </summary>
        /// <param name="hios"></param>
        /// <param name="session"></param>
        /// <param name="syncTransientWord"></param>
        public static void Sync(this IList<ConfindenceHrItemObject> hios, ISession session, Func<Word, Word> syncTransientWord)
        {
            Func<Word, Word> syncWord = (word) =>
            {
                Word res = null;
                if (word.IsTransient)
                    res = syncTransientWord(word);
                else
                    res = session.Get<Word>(word.Id);
                if (res == null)
                {
                    // скопировано и не сохранено / удалено
                    // скопированно новое в поиск - после WordPersisted можно будет найти
                    logger.WarnFormat("Word not synced: {0}, recreate", word);
                    res = new Word(word.Title); // добавляется в словарь при сохранении записи
                }
                return res;
            };

            for (int i = 0; i < hios.Count; i++)
            {
                if (hios[i].HIO is Word)
                {
                    hios[i].HIO = syncWord(hios[i].HIO as Word);
                    // Console.WriteLine((hios[i] as Diagnosis.Models.Word).Equals(word.Actual)); false!
                }
                else if (hios[i].HIO is Measure)
                {
                    var m = hios[i].HIO as Measure;
                    if (m.Word != null)
                    {
                        m.Word = syncWord(m.Word);
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
    }
}

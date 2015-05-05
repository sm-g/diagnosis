﻿using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.Models.Enums;
using Diagnosis.ViewModels.Autocomplete;
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
        /// После вставки десериализованных hio
        /// нужно использовать существующие сущности (Word, IcdDisease),
        /// Comment - valueobject, в Measure меняем Word, а Uom не может быть удален между копированием и вставкой
        /// </summary>
        /// <param name="hios"></param>
        /// <param name="session"></param>
        public static void SyncAfterPaste(this IList<ConfindenceHrItemObject> hios, ISession session)
        {
            Func<Word, Word> syncWord = (word) =>
            {
                Word res = null;
                if (word.IsTransient) // новое может быть в автокомплите
                    res = SuggestionsMaker.GetWordFromCreated(word);

                if (res == null) // пробуем достать из БД
                    using (var tr = session.BeginTransaction())
                    {
                        res = session.Get<Word>(word.Id);
                    }
                if (res == null)
                {
                    logger.WarnFormat("Word not synced: {0}, recreate", word);
                    // новое было сохранено после копирования / сменился доктор - нет в автокомплите
                    // или сохраненое было удалено из БД

                    // вставили запись/тег с этим словом - которого нет нигде

                    // если вставлено в редактор записи - будет сохранено
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
    }
}

using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.DataTransfer;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels
{
    public static class ClipboardHelper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(ClipboardHelper));

        /// <summary>
        /// После вставки десериализованных hio создается другой объект,
        /// нужно использовать существующие сущности (Word, IcdDisease),
        /// Comment - valueobject, в Measure меняем Word, а Uom не может быть удален между копированием и вставкой
        /// </summary>
        /// <param name="hios"></param>
        /// <param name="session"></param>
        public static void SyncAfterPaste(this IList<ConfWithHio> hios, ISession session)
        {
            for (int i = 0; i < hios.Count; i++)
            {
                if (hios[i].HIO is Word)
                {
                    hios[i].HIO = SyncWord(session, hios[i].HIO as Word);
                }
                else if (hios[i].HIO is Measure)
                {
                    var m = hios[i].HIO as Measure;
                    if (m.Word != null)
                    {
                        m.Word = SyncWord(session, m.Word);
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

        private static Word SyncWord(ISession session, Word word)
        {
            Word res = null;
            if (word.IsTransient)
                // новое может быть создано в автокомплите
                res = CreatedWordsManager.GetSameWordFromCreated(word);
            else  // не новое может быть в БД
                using (var tr = session.BeginTransaction())
                    res = session.Get<Word>(word.Id);

            if (res == null)
                // сохраненное после копирования будет в БД
                res = Diagnosis.Data.Queries.WordQuery.ByTitle(session)(word.Title);

            if (res == null)
            {
                logger.WarnFormat("Word not synced: {0}, recreate", word);
                // сменился доктор - нет в автокомплите (почему?)
                // сохраненое было удалено из БД

                // вставили запись/тег с этим словом - которого нет в БД и в created
                res = new Word(word.Title);
            }
            return res;
        }

        /// <summary>
        /// Формат {[id] ToString()[,] ...}
        /// </summary>
        public static void LogHrItemObjects(this log4net.ILog logger, string action, IEnumerable<IHrItemObject> hios)
        {
            logger.DebugFormat("{0} hios: {1}", action, hios.FlattenString());
        }

        public static void LogHrItemObjects(this log4net.ILog logger, string action, IEnumerable<ConfWithHio> chios)
        {
            logger.DebugFormat("{0} hios: {1}", action, chios.FlattenString());
        }

        public static void LogHrs(this log4net.ILog logger, string action, IEnumerable<HrData.HrInfo> hrs)
        {
            logger.DebugFormat("{0} hrs with hios: {1}", action, string.Join("\n", hrs.Select((hr, i) => string.Format("{0} {1}", i, hr.Chios.FlattenString()))));
        }
    }
}
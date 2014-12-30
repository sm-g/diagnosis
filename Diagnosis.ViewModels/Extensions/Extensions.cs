using Diagnosis.Data;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
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
        public static void Sync(this IList<IHrItemObject> hios, ISession session, Func<Word, Word> syncTransientWord)
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
                    res = new Word(word.Title);
                }
                return res;
            };

            for (int i = 0; i < hios.Count; i++)
            {
                if (hios[i] is Word)
                {
                    hios[i] = syncWord(hios[i] as Word);
                    // Console.WriteLine((hios[i] as Diagnosis.Models.Word).Equals(word.Actual)); false!
                }
                else if (hios[i] is Measure)
                {
                    var m = hios[i] as Measure;
                    if (m.Word != null)
                    {
                        m.Word = syncWord(m.Word);
                    }
                }
            }
        }
    }
}

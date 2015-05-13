using Diagnosis.Common;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    public static class SyncerExtensions
    {
        private static void FakeUpdate(this ISession s, Type type, Guid id)
        {
            var table = Names.GetTblByType(type);
            s.CreateSQLQuery(string.Format("UPDATE {0} SET Id = Id WHERE Id = '{1}'", table, id)).ExecuteUpdate();
        }

        /// <summary>
        /// Загружать только выбранные словари и всё для них с сервера.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="remote"></param>
        /// <param name="vocsToLoad"></param>
        /// <returns></returns>
        public static Syncer OnlySelectedVocs(this Syncer s, ISession remote, IEnumerable<Vocabulary> vocsToLoad)
        {
            var selectedVocIds = vocsToLoad.Select(x => x.Id).ToList(); // for session
            var selectedWtIds = vocsToLoad.SelectMany(x => x.WordTemplates.Select(y => y.Id));
            var selectedSpecIds = vocsToLoad.SelectMany(x => x.Specialities.Select(y => y.Id));
            var selectedSpecVocIds = vocsToLoad.SelectMany(x => x.SpecialityVocabularies.Select(y => y.Id));

            using (var tr = remote.BeginTransaction())
            {
                // повторно загружаем даже если не было изменений на сервере
                foreach (var id in selectedVocIds)
                    remote.FakeUpdate(typeof(Vocabulary), id);
                foreach (var id in selectedWtIds)
                    remote.FakeUpdate(typeof(WordTemplate), id);
                foreach (var id in selectedSpecVocIds)
                    remote.FakeUpdate(typeof(SpecialityVocabularies), id);
                // специальность не удаляется при удалении словаря
                tr.Commit();
            }
            s.IdsForSyncPerType.Add(typeof(Vocabulary), selectedVocIds.Cast<object>());
            s.IdsForSyncPerType.Add(typeof(WordTemplate), selectedWtIds.Cast<object>());
            s.IdsForSyncPerType.Add(typeof(Speciality), selectedSpecIds.Cast<object>());
            s.IdsForSyncPerType.Add(typeof(SpecialityVocabularies), selectedSpecVocIds.Cast<object>());
            // слова словаря не загружаются с сервера

            return s;
        }

        /// <summary>
        /// Не синхронизируем область словарей
        /// но загружаем новые шаблоны для установленных словарей
        /// вызвать LoadOrUpdateVoc после синхронизации
        /// </summary>
        /// <param name="s"></param>
        /// <param name="installedVocs"></param>
        /// <returns></returns>
        public static Syncer WithInstalledVocs(this Syncer s, IEnumerable<Vocabulary> installedVocs)
        {
            var installedVocsIds = installedVocs.Select(x => x.Id).ToList().Cast<object>();

            foreach (var table in Scopes.GetVocOnlyTablesToDownload())
            {
                if (table == Names.WordTemplate)
                    s.IgnoreAddingFilterPerType.Add(Names.tblToTypeMap[table], (row) => !installedVocsIds.Contains(row[Names.Id.Vocabulary]));
                else
                    s.IgnoreAddingFilterPerType.Add(Names.tblToTypeMap[table], (row) => true);
            }
            return s;
        }

        /// <summary>
        /// Не синхронизируем врачей при отправке только словарей с сервера.
        /// Таблица Doctors нужна для словарей.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Syncer WithoutDoctors(this Syncer s)
        {
            s.IgnoreAddingFilterPerType.Add(Names.tblToTypeMap[Names.Doctor], (row) => true);

            return s;
        }

        /// <summary>
        /// Не синхронизируем ссылку на пользовательский словарь при отправке на сервер.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Syncer WithoutCustomVocsInDoc(this Syncer s)
        {
            s.ShaperPerType.Add(Names.tblToTypeMap[Names.Doctor], (row) =>
            {
                row[Names.Col.DoctorCustomVocabulary] = DBNull.Value;
            });

            return s;
        }
    }
}
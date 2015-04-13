using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    public static class Helpers
    {
        public static void FakeUpdate(this ISession s, Type type, Guid id)
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

            var IdsForSyncPerType = new Dictionary<Type, IEnumerable<object>>(){
                       {typeof(Vocabulary),             selectedVocIds.Cast<object>()},
                       {typeof(WordTemplate),           selectedWtIds.Cast<object>()},
                       {typeof(Speciality),             selectedSpecIds.Cast<object>()},
                       {typeof(SpecialityVocabularies), selectedSpecVocIds.Cast<object>()},
                        // слова словаря не загружаются с сервера
                    };

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
            s.IdsForSyncPerType = IdsForSyncPerType;

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

            var filter = new Dictionary<Type, Func<DataRow, bool>>();
            foreach (var table in Scopes.GetVocOnlyTablesToDownload())
            {
                if (table == Names.WordTemplate)
                    filter.Add(Names.tblToTypeMap[table], (row) => !installedVocsIds.Contains(row[Names.Id.Vocabulary]));
                else
                    filter.Add(Names.tblToTypeMap[table], (row) => true);
            }
            s.IgnoreAddingFilterPerType = filter;
            return s;
        }
    }
}
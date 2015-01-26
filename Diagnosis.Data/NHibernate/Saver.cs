using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using NHibernate;
using System.Diagnostics;

namespace Diagnosis.Data
{
    public class Saver
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Saver));

        private Patient savingPatient;
        private ISession session;

        public Saver(ISession session)
        {
            this.session = session;
        }

        public void SaveHealthRecord(HealthRecord hr)
        {
            if (hr.GetPatient() == savingPatient) return;

            logger.DebugFormat("saving hr {0}", hr);

            Save(hr);
        }

        /// <summary>
        /// Сохраняет пациента, его курсы, осмотры и все записи.
        /// <param name="deleteEmptyHrs">Удалить все пустые записи.</param>
        /// </summary>
        public void SaveWithCleanup(Patient patient, bool deleteEmptyHrs = true)
        {
            if (savingPatient == patient) return;

            logger.DebugFormat("saving patient {0}", patient);

            savingPatient = patient;

            // удаляем пустые и удаленные
            if (deleteEmptyHrs)
            {
                patient.Courses.ForAll(x =>
                {
                    x.Appointments.ForAll(a => a.DeleteEmptyHrs());
                    x.DeleteEmptyHrs();
                });
                patient.DeleteEmptyHrs();
            }

            Save(patient);

            savingPatient = null;
        }

        public bool Delete(params IDomainObject[] domainObjects)
        {
            if (domainObjects.Length == 0) return true;
            logger.DebugFormat("deleting {0} IDomainObject", domainObjects.Length);
            Debug.Assert(session.IsOpen);

            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in domainObjects)
                    {
                        session.Delete(item);
                    }
                    t.Commit();
                    return true;
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
#if DEBUG
                    throw;
#endif
                }
            }
            return false;
        }

        public bool Save(params IEntity[] entities)
        {
            if (entities.Length == 0) return true;
            logger.DebugFormat("saving {0} IEntity", entities.Length);
            Debug.Assert(session.IsOpen);

            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in entities)
                    {
                        session.SaveOrUpdate(item);
                    }
                    t.Commit();
                    return true;
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
#if DEBUG
                    throw;
#endif
                }
            }
            return false;
        }
    }
}
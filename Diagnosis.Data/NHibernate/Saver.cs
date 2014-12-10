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
            Debug.Assert(session.IsOpen);

            session.SaveOrUpdate(hr);
            using (var t = session.BeginTransaction())
            {
                try
                {
                    t.Commit();
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
        }

        /// <summary>
        /// Сохраняет пациента, его курсы, осмотры и все записи.
        /// <param name="deleteEmptyHrs">Удалить все пустые записи.</param>
        /// </summary>
        public void SaveAll(Patient patient, bool deleteEmptyHrs = false)
        {
            if (savingPatient == patient) return;

            logger.DebugFormat("saving patient {0}", patient);
            Debug.Assert(session.IsOpen);

            savingPatient = patient;

            if (deleteEmptyHrs)
            {
                patient.Courses.ForAll(x =>
                {
                    x.Appointments.ForAll(a => a.DeleteEmptyHrs());
                    x.DeleteEmptyHrs();
                });
                patient.DeleteEmptyHrs();
            }

            session.SaveOrUpdate(patient);
            using (var t = session.BeginTransaction())
            {
                try
                {
                    t.Commit();
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

            savingPatient = null;
        }

        public bool Delete(params IDomainObject[] domainObjects)
        {
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
        public bool Save(params IDomainObject[] domainObjects)
        {
            using (var t = session.BeginTransaction())
            {
                try
                {
                    foreach (var item in domainObjects)
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
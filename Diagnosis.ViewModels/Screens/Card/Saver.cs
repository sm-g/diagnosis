using Diagnosis.Common;
using Diagnosis.Models;
using log4net;
using NHibernate;

namespace Diagnosis.ViewModels.Screens
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
                }
            }

            savingPatient = null;
        }

        internal void Delete(IHrsHolder holder)
        {
            using (var t = session.BeginTransaction())
            {
                try
                {
                    session.Delete(holder);
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }
            }
        }
    }
}
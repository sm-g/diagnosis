using Diagnosis.Models;
using Diagnosis.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using Diagnosis.ViewModels;

namespace Tests
{
    /// <summary>
    /// Основа для тестирования записей, осмотров, курсов.
    /// </summary>
    public abstract class DbTestBase : InMemoryDatabaseTest
    {
        protected Word w1 = new Word("x");
        protected Word w2 = new Word("y");
        protected Word w3 = new Word("z");
        protected Symptom s1;
        protected Symptom s2;
        protected Patient p1 = new Patient("Test");
        protected Doctor d1 = new Doctor("Test", "Name");
        protected Course course1;
        protected Course course2;
        protected Appointment app1;
        protected Appointment app2;
        protected HealthRecord hr1;
        protected HealthRecord hr2;

        [TestInitialize]
        public void Prepare()
        {
            s1 = new Symptom(new Word[] { w1, w2 });
            s2 = new Symptom(new Word[] { w1 });

            course1 = d1.StartCourse(p1);
            course2 = d1.StartCourse(p1);
            app1 = new Appointment(course1, d1);
            app2 = new Appointment(course1, d1);
            hr1 = new HealthRecord(app1) { Comment = "test" };
            hr2 = new HealthRecord(app1) { Comment = "test" };

            using (var tx = session.BeginTransaction())
            {
                session.Save(w1);
                session.Save(w2);
                session.Save(w3);

                session.Save(s1);
                session.Save(s2);

                session.Save(p1);
                session.Save(d1);
                session.Save(course1);
                session.Save(course2);
                session.Save(app1);
                session.Save(app2);
                session.Save(hr1);
                session.Save(hr2);
                tx.Commit();
            }

            AuthorityController.LogIn(d1);
        }

        [TestCleanup]
        public void Cleanup()
        {
            using (var tx = session.BeginTransaction())
            {
                session.Delete(w1);
                session.Delete(w2);
                session.Delete(w3);

                session.Delete(s1);
                session.Delete(s2);

                session.Delete(p1);
                session.Delete(d1);
                session.Delete(course1);
                session.Delete(course2);
                session.Delete(app1);
                session.Delete(app2);
                session.Delete(hr1);
                session.Delete(hr2);
                tx.Commit();
            }
        }
    }
}
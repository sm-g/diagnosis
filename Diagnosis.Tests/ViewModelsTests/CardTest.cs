using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests.CardTests
{
    [TestClass]
    public class CardTest : InMemoryDatabaseTest
    {
        protected Word w1;
        protected Word w2;
        protected Word w3;
        protected HrItem i1;
        protected HrItem i2;
        protected HrItem i3;
        protected Patient p1;
        protected Doctor d1;
        protected Course course1;
        protected Course course2;
        protected Appointment app1;
        protected Appointment app2;
        protected HealthRecord hr1;
        protected HealthRecord hr2;

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(1);
            p1 = session.Get<Patient>(1);
            AuthorityController.LogIn(d1);

            app1 = course1.Appointments.First();
            app2 = course1.Appointments.Last();

            Assert.IsTrue(course1.Patient == p1);
            Assert.IsTrue(course2.Patient == p1);
        }

        [TestMethod]
        public void OpenLastAppInCourse()
        {
            // открываем курс
            var card = new CardViewModel(course1, true);
            Assert.AreEqual(course1, card.Navigator.Current.Holder);

            // открывается последний осмотр
            Assert.AreEqual(app2, card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void CreateCourseWithFirstHr()
        {
            var card = new CardViewModel(p1, true);
            d1.StartCourse(p1);
            var newCourse = p1.Courses.LastOrDefault();

            // в курсе есть осмотр с записью
            Assert.AreEqual(1, newCourse.Appointments.Count());
            var newApp = newCourse.Appointments.LastOrDefault();
            Assert.AreEqual(1, newApp.HealthRecords.Count());

            // запись открыта в редакторе
            var newHr = newApp.HealthRecords.LastOrDefault();

            Assert.AreEqual(newHr, card.HrEditor.HealthRecord.healthRecord);
        }

        [TestMethod]
        public void SelectNewHr()
        {
            var card = new CardViewModel(p1, true);

            Assert.IsNull(card.HrList.SelectedHealthRecord);

            card.HrList.AddHealthRecordCommand.Execute(null);
            Assert.AreEqual(p1.HealthRecords.Last(), card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void OpenNewHrInEditor()
        {
            var card = new CardViewModel(p1, true);

            card.HrList.AddHealthRecordCommand.Execute(null);
            Assert.AreEqual(p1.HealthRecords.Last(), card.HrEditor.HealthRecord.healthRecord);
        }

        [TestMethod]
        public void OpenNewCourse()
        {
            // открываем пациента
            var card = new CardViewModel(p1, true);

            Assert.AreEqual(p1, card.Navigator.Current.Holder);

            // начинаем курс
            d1.StartCourse(p1);

            // курс открывается
            Assert.AreEqual(p1.Courses.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewApp()
        {
            // открываем курс
            var card = new CardViewModel(course2, true);
            Assert.IsFalse(course2.IsEnded); // можно добавить осмотр

            // добавляем осмотр
            card.Navigator.Current.AddAppointmentCommand.Execute(null);

            // осмотр открывается
            Assert.AreEqual(course2.Appointments.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenPatient()
        {
            var card = new CardViewModel(p1, true);
            Assert.AreEqual(p1, card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenCourse()
        {
            var card = new CardViewModel(course1, true);
            Assert.AreEqual(course1, card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenApp()
        {
            var card = new CardViewModel(app1, true);

            Assert.AreEqual(app1, card.Navigator.Current.Holder);
        }
    }
}
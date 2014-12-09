﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class CardTest : InMemoryDatabaseTest
    {
        private Dictionary<int, Patient> p = new Dictionary<int, Patient>();
        private Dictionary<int, Course> c = new Dictionary<int, Course>();
        private Dictionary<int, Appointment> a = new Dictionary<int, Appointment>();

        private Doctor d1;

        private static int[] pIds = new[] { 1, 2, 3, 4, 5 };
        private static int[] cIds = new[] { 1, 2, 3, 4 };
        private static int[] aIds = new[] { 1, 2, 3, 4, 5 };

        [TestInitialize]
        public void Init()
        {
            d1 = session.Get<Doctor>(IntToGuid<Doctor>(1));
            AuthorityController.LogIn(d1);

            pIds.ForAll((id) => p[id] = session.Get<Patient>(IntToGuid<Patient>(id)));
            cIds.ForAll((id) => c[id] = session.Get<Course>(IntToGuid<Course>(id)));
            aIds.ForAll((id) => a[id] = session.Get<Appointment>(IntToGuid<Appointment>(id)));

            // p[3] c[4] a[5] are empty, for deletions
        }

        [TestMethod]
        public void OpenPatient()
        {
            var card = new CardViewModel(p[1], true);
            Assert.AreEqual(p[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenPatientWithLastApp()
        {
            var card = new CardViewModel(true);
            card.Open(p[1], lastAppOrCourse: true);

            Assert.AreEqual(a[4], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenCourse()
        {
            var card = new CardViewModel(c[1], true);
            Assert.AreEqual(c[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenApp()
        {
            var card = new CardViewModel(a[1], true);

            Assert.AreEqual(a[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OrderCoursesDescending()
        {
            var card = new CardViewModel(p[1], true);
            Assert.AreEqual(c[2], card.Navigator.Current.Children[0].Holder); // 29 мая
            Assert.AreEqual(c[1], card.Navigator.Current.Children[1].Holder); // 7-14 марта
        }

        [TestMethod]
        public void OpenNewCourse()
        {
            // открываем пациента
            var card = new CardViewModel(p[1], true);
            Assert.AreEqual(p[1], card.Navigator.Current.Holder);

            // начинаем курс
            d1.StartCourse(p[1]);

            // курс открывается
            Assert.AreEqual(p[1].Courses.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewApp()
        {
            // открываем курс
            var card = new CardViewModel(c[2], true);
            Assert.IsFalse(c[2].IsEnded); // можно добавить осмотр

            card.Navigator.Current.AddAppointmentCommand.Execute(null);

            Assert.AreEqual(c[2].Appointments.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewApp2()
        {
            var card = new CardViewModel(p[1], true);
            Assert.AreEqual(c[2], card.Navigator.Current.Children[0].Holder);
            Assert.IsFalse(c[2].IsEnded);

            card.Navigator.Current.Children[0].AddAppointmentCommand.Execute(null);

            Assert.AreEqual(c[2].Appointments.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewHrInEditor()
        {
            var card = new CardViewModel(p[1], true);

            card.HrList.AddHealthRecordCommand.Execute(null);
            Assert.AreEqual(p[1].HealthRecords.Last(), card.HrEditor.HealthRecord.healthRecord);
        }

        [TestMethod]
        public void OpenLastAppInCourse()
        {
            // открываем курс
            var card = new CardViewModel(true);
            card.Open(c[1], lastAppOrCourse: true);

            // открывается последний осмотр
            Assert.AreEqual(a[2], card.Navigator.Current.Holder);
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void CreateCourseWithFirstHr()
        {
            var card = new CardViewModel(p[1], true);
            d1.StartCourse(p[1]);
            var newCourse = p[1].Courses.LastOrDefault();

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
            var card = new CardViewModel(p[1], true);

            Assert.IsNull(card.HrList.SelectedHealthRecord);

            card.HrList.AddHealthRecordCommand.Execute(null);
            Assert.AreEqual(p[1].HealthRecords.Last(), card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void DeleteAppCoursePatient()
        {
            var card = new CardViewModel(a[5], true);
            IHrsHolder holder = null;
            bool removed = false;
            card.Navigator.Navigating += (s, e) =>
            {
                holder = e.holder;
            };
            card.LastItemRemoved += (s, e) =>
            {
                removed = true;
            };
            card.Navigator.Current.DeleteCommand.Execute(null);
            Assert.AreEqual(c[4], holder);

            card.Navigator.Current.DeleteCommand.Execute(null);
            Assert.AreEqual(p[3], holder);

            card.Navigator.Current.DeleteCommand.Execute(null);
            Assert.IsNull(holder);
            Assert.IsTrue(removed);
        }
    }
}
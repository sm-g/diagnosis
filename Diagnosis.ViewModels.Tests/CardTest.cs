using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class CardTest : ViewModelTest
    {
        private CardViewModel card;

        [TestInitialize]
        public void Init()
        {
            Load<Doctor>();
            Load<Patient>();
            Load<Course>();
            Load<Appointment>();
            Load<HealthRecord>();
            Load<Word>();

            AuthorityController.TryLogIn(d1);
            // p[3] c[4] a[5] are empty, for deletions
            card = new CardViewModel(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (card != null)
                card.Dispose();
        }

        #region Opening

        [TestMethod]
        public void OpenPatient()
        {
            card.Open(p[1]);
            Assert.AreEqual(p[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenPatientWithLastApp()
        {
            card.Open(p[1], lastAppOrCourse: true);

            Assert.AreEqual(a[4], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenCourse()
        {
            card.Open(c[1]);
            Assert.AreEqual(c[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenApp()
        {
            card.Open(a[1]);
            Assert.AreEqual(a[1], card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OrderCoursesDescending()
        {
            card.Open(p[1]);
            Assert.AreEqual(c[2], card.Navigator.Current.Children[0].Holder); // 29 мая
            Assert.AreEqual(c[1], card.Navigator.Current.Children[1].Holder); // 7-14 марта
        }

        [TestMethod]
        public void OpenNewCourse()
        {
            card.Open(p[1]);
            // начинаем курс
            p[1].AddCourse(d1);

            // курс открывается
            Assert.AreEqual(p[1].Courses.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewApp()
        {
            card.Open(c[2]);
            Assert.IsTrue(card.Navigator.Current.HolderVm.AddAppointmentCommand.CanExecute(null));

            card.Navigator.Current.HolderVm.AddAppointmentCommand.Execute(null);

            Assert.AreEqual(c[2].Appointments.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewApp2()
        {
            card.Open(p[1]);
            Assert.AreEqual(c[2], card.Navigator.Current.Children[0].Holder);
            Assert.IsFalse(c[2].IsEnded);

            card.Navigator.Current.Children[0].HolderVm.AddAppointmentCommand.Execute(null);

            Assert.AreEqual(c[2].Appointments.LastOrDefault(), card.Navigator.Current.Holder);
        }

        [TestMethod]
        public void OpenNewHrInEditor()
        {
            card.Open(p[1]);
            card.HrList.AddHealthRecordCommand.Execute(null);
            Assert.AreEqual(p[1].HealthRecords.Last(), card.HrEditor.HealthRecord.healthRecord);
        }

        [TestMethod]
        public void OpenLastAppInCourse()
        {
            // открываем курс

            card.Open(c[1], lastAppOrCourse: true);

            // открывается последний осмотр
            Assert.AreEqual(a[2], card.Navigator.Current.Holder);

            card.Open(p[5], lastAppOrCourse: true);

            // открывается последний курс
            Assert.AreEqual(0, c[3].Appointments.Count());
            Assert.AreEqual(c[3], card.Navigator.Current.Holder);
        }

        #endregion Opening

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void CreateCourseWithFirstHrFails()
        {
            card.Open(p[1]);
            p[1].AddCourse(d1);
            var newCourse = p[1].Courses.LastOrDefault();

            // в курсе есть осмотр с записью
            Assert.AreEqual(1, newCourse.Appointments.Count());
            var newApp = newCourse.Appointments.LastOrDefault();
            Assert.AreEqual(1, newApp.HealthRecords.Count());

            // запись открыта в редакторе
            var newHr = newApp.HealthRecords.LastOrDefault();

            Assert.AreEqual(newHr, card.HrEditor.HealthRecord.healthRecord);
        }

        #region Selection

        [TestMethod]
        public void SelectNewHr()
        {
            card.Open(p[1]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.HrList.AddHealthRecordCommand.Execute(null);

            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
            Assert.AreEqual(p[1].HealthRecords.Last(), card.HrList.SelectedHealthRecord.healthRecord);
        }

        [TestMethod]
        public void SelectManyShowEditor()
        {
            card.Open(a[2]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.ToogleHrEditor();

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyOpenEditor()
        {
            card.Open(a[2]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.StartEditHr(hr[21], false);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyShowHideEditor()
        {
            card.Open(a[2]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.ToogleHrEditor();
            card.ToogleHrEditor();

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(2, card.HrList.SelectedHealthRecords.Count());
        }

        [TestMethod]
        public void SelectManyOpenCloseEditor()
        {
            card.Open(a[2]);
            card.HrList.SelectHealthRecords(new[] { hr[20], hr[21] });
            card.StartEditHr(hr[21], false);
            card.HrEditor.CloseCommand.Execute(null);

            Assert.AreEqual(hr[21], card.HrList.SelectedHealthRecord.healthRecord);
            Assert.AreEqual(1, card.HrList.SelectedHealthRecords.Count());
        }

        #endregion Selection

        #region Saving

        [TestMethod]
        public void DeleteEmptyAppCoursePatient()
        {
            card.Open(a[5]);
            IHrsHolder holder = null;
            bool removed = false;

            var NavigatingEh = (EventHandler<DomainEntityEventArgs>)((s, e) =>
            {
                holder = e.entity as IHrsHolder;
            });
            var LastItemRemovedEh = (EventHandler)((s, e) =>
            {
                removed = true;
            });
            card.Navigator.Navigating += NavigatingEh;
            card.LastItemRemoved += LastItemRemovedEh;

            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
            Assert.AreEqual(c[4], holder);

            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
            Assert.AreEqual(p[3], holder);

            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
            Assert.IsNull(holder);
            Assert.IsTrue(removed);

            card.Navigator.Navigating -= NavigatingEh;
            card.LastItemRemoved -= LastItemRemovedEh;
        }

        [TestMethod]
        public void DontSaveEditingHr()
        {
            card.Open(a[2]);
            var hrsBefore = a[2].HealthRecords.Count();

            // удалили несколько записей
            card.HrList.SelectHealthRecord(hr[20]);
            card.HrList.DeleteCommand.Execute(null);

            // открыли редактор
            card.HrList.SelectHealthRecord(hr[21]);
            card.ToogleHrEditor();
            hr[21].FromDate.Year = 2010;

            // завершили удаление
            a[2].RemoveHealthRecord(hr[20]);

            // не сохраненяем открытую запись
            Assert.IsTrue(!hr[20].IsDirty);
            Assert.AreEqual(hrsBefore - 1, a[2].HealthRecords.Count());

            Assert.IsTrue(hr[21].IsDirty);

            // сохраненяем открытую запись
            card.ToogleHrEditor();
            Assert.IsTrue(!hr[21].IsDirty);
        }

        [TestMethod]
        public void DeleteHolderWithDeltedHr()
        {
            card.Open(a[5]);
            Action<CardViewModel, IHrsHolder> AddTwoCommentsAndDelete = (_card, holder) =>
                {
                    _card.HrList.AddHealthRecordCommand.Execute(null);
                    holder.HealthRecords.Last().AddItems(new Comment("1").ToEnumerable());
                    _card.HrList.AddHealthRecordCommand.Execute(null);
                    holder.HealthRecords.Last().AddItems(new Comment("2").ToEnumerable());
                    _card.HrEditor.CloseCommand.Execute(null);

                    _card.HrList.HealthRecords.ForEach(x => x.DeleteCommand.Execute(null));
                };

            AddTwoCommentsAndDelete(card, a[5]);
            Assert.IsTrue(card.Navigator.Current.HolderVm.DeleteCommand.CanExecute(null));

            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
            Assert.AreEqual(c[4], card.Navigator.Current.Holder);

            AddTwoCommentsAndDelete(card, c[4]);
            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
            Assert.AreEqual(p[3], card.Navigator.Current.Holder);

            AddTwoCommentsAndDelete(card, p[3]);
            card.Navigator.Current.HolderVm.DeleteCommand.Execute(null);
        }

        [TestMethod]
        public void ChangeAppWithDeletedHrs()
        {
            card.Open(a[2]);
            card.HrList.SelectHealthRecords(a[2].HealthRecords);
            card.HrList.HealthRecords.ForEach(x => x.DeleteCommand.Execute(null));

            card.Navigator.NavigateTo(a[1]); // sibling

            Assert.AreEqual(0, a[2].HealthRecords.Count());
        }

        #endregion Saving

        [TestMethod]
        public void NavigatorFindVm()
        {
            card.Open(a[2]);
            var cardVm = card.Navigator.FindItemVmOf(a[2]);

            Assert.IsTrue(cardVm != null);
            Assert.AreEqual(a[2], cardVm.Holder);
        }

        [TestMethod]
        public void AddUsedHiddenWordToCustomVoc()
        {
            card.Dispose();
            AuthorityController.TryLogIn(d2);

            card = new CardViewModel(true);
            // слово в словарях, но недоступно врачу
            Assert.IsTrue(w[6].Vocabularies.Any());
            Assert.IsTrue(!d2.Words.Contains(w[6]));

            card.Open(a[1]);
            card.HrList.AddHealthRecordCommand.Execute(null);
            // используеум слово в записи
            card.HrEditor.HealthRecord.healthRecord.SetItems(new[] { w[6] });
            // сохраняем запись
            card.HrEditor.Unload();

            Assert.IsTrue(d2.Words.Contains(w[6]));
            Assert.IsTrue(d2.CustomVocabulary.Words.Contains(w[6]));
        }

        [TestMethod]
        public void AddNewPastedWordToCustomVoc()
        {
            // вырезаем новое слово
            var w = new Word("1");
            card.Open(a[1]);
            card.HrList.AddHealthRecordCommand.Execute(null);
            var auto = card.HrEditor.Autocomplete as AutocompleteViewModel;
            auto.AddTag(w);
            auto.SelectedTag = auto.Tags[0];
            auto.Cut();
            card.HrEditor.Unload();
            //  вставляем
            card.HrList.AddHealthRecordCommand.Execute(null);

            auto = card.HrEditor.Autocomplete as AutocompleteViewModel;
            auto.Paste();
            var replaced = auto.Tags[0].Blank as Word;

            // сохраняем запись - добавляется в словарь
            card.HrEditor.Unload();

            Assert.IsTrue(w.IsTransient);
            Assert.IsFalse(d1.CustomVocabulary.Words.Contains(w));
            Assert.IsTrue(d1.CustomVocabulary.Words.Contains(replaced));
        }
    }
}
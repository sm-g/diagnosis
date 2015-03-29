using Diagnosis.Common;
using EventAggregator;
using Diagnosis.Models;
using NHibernate.Linq;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Diagnosis.Data;
using System.Linq;
using System.Collections.Generic;
using Diagnosis.Data.Queries;

namespace Diagnosis.ViewModels.Screens
{
    public class WordEditorViewModel : DialogViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(WordEditorViewModel));
        private readonly Word word;
        internal Word saved;
        List<Word> dbWords;
        public WordEditorViewModel(Word word)
        {
            this.word = word;
            (word as IEditableObject).BeginEdit();

            dbWords = Session.Query<Word>()
                .ToList();

            Word = new WordViewModel(word);
            Word.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Title")
                {
                    TestExisting(Word);
                }
            };
            TestExisting(Word);

            Title = "Редактирование слова";
            HelpTopic = "addword";
            WithHelpButton = true;
        }

        /// <summary>
        /// Нельзя ввести слово, которое уже есть в словаре врача.
        /// </summary>
        private void TestExisting(WordViewModel vm)
        {
            vm.HasExistingTitle = dbWords.Any(w =>
                AuthorityController.CurrentDoctor.Words.Contains(w) &&
                w.Title == word.Title && w != word);
        }

        /// <summary>
        /// Начинает редактировать новое слово.
        /// </summary>
        public WordEditorViewModel(string text)
            : this(new Word(text))
        { }

        public WordViewModel Word
        {
            get;
            private set;
        }


        public override bool CanOk
        {
            get
            {
                return word.IsValid() && !Word.HasExistingTitle;
            }
        }

        protected override void OnOk()
        {
            (word as IEditableObject).EndEdit();

            // если такое слово уже было, делааем доступным врачу

            var toSave = dbWords.FirstOrDefault(w => w.Title == word.Title && w != word) ?? word;
            AuthorityController.CurrentDoctor.AddWords(toSave.ToEnumerable());
            new Saver(Session).Save(toSave);

            this.Send(Event.WordSaved, word.AsParams(MessageKeys.Word));
            saved = toSave;
        }

        protected override void OnCancel()
        {
            (word as IEditableObject).CancelEdit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Word.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
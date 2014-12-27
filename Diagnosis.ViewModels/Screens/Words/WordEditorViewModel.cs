using Diagnosis.Common;
using EventAggregator;
using Diagnosis.Models;
using NHibernate.Linq;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Diagnosis.Data;
using System.Linq;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Screens
{
    public class WordEditorViewModel : DialogViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(WordEditorViewModel));
        private readonly Word word;

        public WordEditorViewModel(Word word)
        {
            this.word = word;
            (word as IEditableObject).BeginEdit();

            var ws = Session.Query<Word>()
                .ToList();

            Word = new WordViewModel(word);
            Word.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Title")
                {
                    TestExisting(Word, ws);
                }
            };
            TestExisting(Word, ws);

            Title = "Редактирование слова";
        }

        /// <summary>
        /// Нельзя ввести слово, которое уже есть в словаре
        /// </summary>
        private void TestExisting(WordViewModel vm, IEnumerable<Word> ws)
        {
            vm.HasExistingTitle = ws.Any(w => w.Title == word.Title && w != word);
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

            new Saver(Session).Save(word);

            this.Send(Events.WordSaved, word.AsParams(MessageKeys.Word));
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
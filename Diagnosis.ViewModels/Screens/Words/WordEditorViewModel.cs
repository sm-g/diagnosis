using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using EventAggregator;
using NHibernate.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class WordEditorViewModel : DialogViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(WordEditorViewModel));
        private readonly Word word;
        internal Word saved;
        private ExistanceTester<Models.Word> tester;
        string oldTitle;
        public WordEditorViewModel(Word word)
        {
            Contract.Requires(word != null);

            this.word = word;
            oldTitle = word.Title;
            (word as IEditableObject).BeginEdit();

            Word = new WordViewModel(word);
            //Нельзя ввести слово, которое уже есть в словарях врача.
            tester = new ExistanceTester<Word>(word, Word, Session,
                extraTest: w => AuthorityController.CurrentDoctor.Words.Contains(w));
            tester.Test();

            Title = "Редактирование слова";
            HelpTopic = "addword";
            WithHelpButton = true;
        }

        public WordViewModel Word
        {
            get;
            private set;
        }

        public override bool CanOk
        {
            get
            {
                return word.IsValid() && !Word.HasExistingValue;
            }
        }

        protected override void OnOk()
        {
            foreach (var crit in word.Crits)
            {
                var l = OptionsLoader.FromFormat(crit.OptionsFormat, Session);
                crit.Options = l.ReplaceWord(crit.Options, oldTitle, word.Title);
            }

            (word as IEditableObject).EndEdit();

            // если такое слово уже было, делааем доступным врачу
            var toSave = WordQuery.ByTitle(Session)(word.Title) ?? word;
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
                tester.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
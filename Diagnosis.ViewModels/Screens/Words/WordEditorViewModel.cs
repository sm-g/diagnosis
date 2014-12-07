using Diagnosis.Common;
using EventAggregator;
using Diagnosis.Models;
using NHibernate;
using System.ComponentModel;
using System.Diagnostics.Contracts;

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

            Word = new WordViewModel(word);
            Title = "Редактирование слова";
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
                return word.IsValid();
            }
        }

        protected override void OnOk()
        {
            (word as IEditableObject).EndEdit();

            using (var t = Session.BeginTransaction())
            {
                try
                {
                    Session.SaveOrUpdate(word);
                    t.Commit();
                }
                catch (System.Exception e)
                {
                    t.Rollback();
                    logger.Error(e);
                }

                this.Send(Events.WordSaved, word.AsParams(MessageKeys.Word));
            }
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
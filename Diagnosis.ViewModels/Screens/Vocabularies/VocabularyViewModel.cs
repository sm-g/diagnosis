using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularyViewModel : CheckableBase
    {
        internal readonly Vocabulary voc;

        public VocabularyViewModel(Vocabulary voc)
        {
            Contract.Requires(voc != null);
            this.voc = voc;
            voc.PropertyChanged += voc_PropertyChanged;

            SyncCheckedAndSelected = true;
        }

        #region Model

        public string Title
        {
            get { return voc.Title; }
            set { voc.Title = value; }
        }

        /// <summary>
        /// Кол-во использованных слов
        /// </summary>
        public int Usage
        {
            get
            {
                return voc.Words.Where(x => !x.IsEmpty()).Count();
            }
        }

        #endregion Model

        //public ICommand EditCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            this.Send(Event.EditWord, voc.AsParams(MessageKeys.Word));
        //        });
        //    }
        //}

        public bool Unsaved
        {
            get
            {
                return voc.IsDirty;
            }
        }

        public bool HasExistingTitle { get; set; }
        public bool WasEdited { get; set; }

        public override string this[string columnName]
        {
            get
            {
                if (!WasEdited) return string.Empty;

                var results = voc.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                if (HasExistingTitle) message = "Такой словарь уже есть.";
                return message != null ? message : string.Empty;
            }
        }

        private void voc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }

        public override string ToString()
        {
            return voc.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                voc.PropertyChanged -= voc_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
}
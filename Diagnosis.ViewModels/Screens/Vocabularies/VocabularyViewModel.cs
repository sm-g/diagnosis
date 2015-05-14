using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class VocabularyViewModel : CheckableBase, IExistTestable
    {
        internal readonly Vocabulary voc;

        public VocabularyViewModel(Vocabulary voc)
        {
            Contract.Requires(voc != null);
            this.voc = voc;
            this.validatableEntity = voc;
            voc.PropertyChanged += voc_PropertyChanged;
            voc.WordTemplatesChanged += voc_WordTemplatesChanged;

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

        public int TemplatesCount { get { return voc.WordTemplates.Count(); } }

        #endregion Model

        //public ICommand EditCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            this.Send(Event.EditVoc, voc.AsParams(MessageKeys.Voc));
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

        public bool HasExistingValue { get; set; }

        public bool WasEdited { get; set; }

        string[] IExistTestable.TestExistingFor
        {
            get { return new[] { "Title" }; }
        }

        string IExistTestable.ThisValueExistsMessage
        {
            get { return "Такой словарь уже есть."; }
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
                voc.WordTemplatesChanged -= voc_WordTemplatesChanged;
            }
            base.Dispose(disposing);
        }

        private void voc_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }

        private void voc_WordTemplatesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(() => TemplatesCount);
        }
    }
}
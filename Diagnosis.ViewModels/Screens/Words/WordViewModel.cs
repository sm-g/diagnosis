using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class WordViewModel : HierarchicalBase<WordViewModel>, IExistTestable
    {
        internal readonly Word word;

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;
            this.validatableEntity = word;
            word.PropertyChanged += word_PropertyChanged;
        }

        #region Model

        public string Title
        {
            get { return word.Title; }
            set { word.Title = value; }
        }

        public int Usage
        {
            get
            {
                return word.HealthRecords.Count();
            }
        }

        #endregion Model

        #region CheckableBase

        private bool checkedBySelection;

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();

            // check when select and uncheck when selection goes away
            // except was checked by checkbox before
            if (!IsChecked || checkedBySelection)
            {
                checkedBySelection = IsSelected;
                IsChecked = IsSelected;
            }
        }

        protected override void OnCheckedChanged()
        {
            base.OnCheckedChanged();

            // убираем выделение при снятии флажка
            IsSelected = IsChecked;
        }

        #endregion CheckableBase

        public ICommand SendToSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                   {
                       this.Send(Event.SendToSearch, word.AsConfidencable().ToEnumerable().AsParams(MessageKeys.ToSearchPackage));
                   });
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Event.EditWord, word.AsParams(MessageKeys.Word));
                });
            }
        }

        public bool Unsaved
        {
            get
            {
                return word.IsDirty;
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
            get { return "Такое слово уже есть."; }
        }

        public override string ToString()
        {
            return word.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                word.PropertyChanged -= word_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        private void word_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
        }
    }
}
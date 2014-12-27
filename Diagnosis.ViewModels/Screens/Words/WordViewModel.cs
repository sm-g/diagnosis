using Diagnosis.Common;
using Diagnosis.Models;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class WordViewModel : HierarchicalBase<WordViewModel>
    {
        internal readonly Word word;

        public WordViewModel(Word w)
        {
            Contract.Requires(w != null);
            word = w;
            word.PropertyChanged += word_PropertyChanged;
        }

        #region Model

        public string Title
        {
            get { return word.Title; }
            set { word.Title = value; }
        }

        public HrCategory DefaultCategory
        {
            get { return word.DefaultCategory; }
            set { word.DefaultCategory = value; }
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
                       this.Send(Events.SendToSearch, word.ToEnumerable().AsParams(MessageKeys.HrItemObjects));
                   });
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    this.Send(Events.EditWord, word.AsParams(MessageKeys.Word));
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

        public bool HasExistingTitle { get; set; }
        public bool WasEdited { get; set; }

        public override string this[string columnName]
        {
            get
            {
                if (!WasEdited) return string.Empty;

                var results = word.SelfValidate();
                if (results == null)
                    return string.Empty;
                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                if (HasExistingTitle) message = "Такое слово уже есть.";
                return message != null ? message : string.Empty;
            }
        }

        private void word_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            WasEdited = true;
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
    }
}
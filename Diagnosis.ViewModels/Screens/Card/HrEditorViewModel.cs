using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels.Autocomplete;
using EventAggregator;
using log4net;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class HrEditorViewModel : ViewModelBase, IClipboardTarget
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HrEditorViewModel));

        private AutocompleteViewModel _autocomplete;

        private HealthRecordViewModel _hr;
        private ISession session;
        private bool _focused;
        private IEnumerable<HrCategory> _categories;
        private Recognizer recognizer;

        public HrEditorViewModel(ISession session)
        {
            this.session = session;
        }

        [Obsolete("For xaml only.")]
        public HrEditorViewModel()
        {
        }

        /// <summary>
        /// Запись выгружена.
        /// </summary>
        public event EventHandler<DomainEntityEventArgs> Unloaded;

        /// <summary>
        /// Редактор закрыт. Перед этим выгружается запись.
        /// </summary>
        public event EventHandler<DomainEntityEventArgs> Closed;

        /// <summary>
        /// Редактор закрывается по команде. Запись может быть null.
        /// </summary>
        public event EventHandler<DomainEntityEventArgs> Closing;

        #region HealthRecord

        public HealthRecordViewModel HealthRecord
        {
            get
            {
                return _hr;
            }
            private set
            {
                if (_hr != value)
                {
                    _hr = value;
                    OnPropertyChanged("HealthRecord");
                    OnPropertyChanged("Category");
                    OnPropertyChanged(() => HasHealthRecord);
                }
            }
        }

        public IEnumerable<HrCategory> Categories
        {
            get
            {
                if (_categories == null && session != null)
                {
                    var cats = new List<HrCategory>(session.Query<HrCategory>().ToList());
                    cats.Add(HrCategory.Null);
                    _categories = new List<HrCategory>(cats
                        .OrderBy(cat => cat.Ord).ToList());
                }
                return _categories;
            }
        }

        public HrCategory Category
        {
            get
            {
                return HealthRecord != null && HealthRecord.Category != null
                    ? HealthRecord.Category
                    : HrCategory.Null;
            }
            set { HealthRecord.Category = value; }
        }

        #endregion HealthRecord

        public bool HasHealthRecord
        {
            get { return HealthRecord != null; }
        }

        public bool IsFocused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    //logger.DebugFormat("hrEditor IsFocused {0}", value);
                    OnPropertyChanged(() => IsFocused);
                }
            }
        }
        #region Commands

        public RelayCommand RevertCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    (HealthRecord.healthRecord as IEditableObject).CancelEdit();
                    (HealthRecord.healthRecord as IEditableObject).BeginEdit();
                    CreateAutoComplete();
                }, () => HasHealthRecord && HealthRecord.healthRecord.IsDirty);
            }
        }

        public RelayCommand DeleteHrCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    HealthRecord.healthRecord.IsDeleted = true;
                });
            }
        }

        public RelayCommand DeleteItemsCommand
        {
            get { return Autocomplete != null ? Autocomplete.DeleteCommand : null; }
        }

        public VisibleRelayCommand<TagViewModel> SendToSearchCommand
        {
            get { return Autocomplete != null ? Autocomplete.SendToSearchCommand : null; }
        }


        public void Cut()
        {
            Autocomplete.Cut();
        }

        public void Copy()
        {
            Autocomplete.Copy();
        }

        public void Paste()
        {
            Autocomplete.Paste();
        }

        public bool CanPaste()
        {
            return Autocomplete.CanPaste();
        }

        public RelayCommand CutCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Autocomplete.Cut();
                });
            }
        }

        public RelayCommand CopyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Autocomplete.Copy();
                });
            }
        }

        public RelayCommand PasteCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Autocomplete.Paste();
                }, () => Autocomplete != null && Autocomplete.CanPaste());
            }
        }

        public RelayCommand AddIcdCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Autocomplete.AddFromEditor(BlankType.Icd);
                });
            }
        }

        public RelayCommand AddMeasureCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Autocomplete.AddFromEditor(BlankType.Measure);
                });
            }
        }

        /// <summary>
        /// Закрывает редактор (даже без записи).
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OnClosing(HealthRecord != null ? HealthRecord.healthRecord : null);
                    Unload();
                });
            }
        }
        #endregion

        #region AutoComplete

        public AutocompleteViewModel Autocomplete
        {
            get { return _autocomplete; }
            private set
            {
                _autocomplete = value;
                OnPropertyChanged("Autocomplete");
                OnPropertyChanged(() => SendToSearchCommand);
                OnPropertyChanged(() => DeleteItemsCommand);

            }
        }

        public Word SyncTransientWord(Word w)
        {
            if (recognizer != null)
            {
                return recognizer.SyncTransientWord(w);
            }
            return w;
        }

        /// <summary>
        /// Создает автокомплит с начальными словами и комментами из редактируемой записи.
        /// </summary>
        private void CreateAutoComplete()
        {
            if (Autocomplete != null)
            {
                Autocomplete.Dispose();
            }

            var initials = HealthRecord.healthRecord.GetOrderedCHIOs();
            recognizer = new Recognizer(session) { ShowChildrenFirst = true };

            Autocomplete = new AutocompleteViewModel(
                recognizer,
                AutocompleteViewModel.OptionsMode.HrEditor,
                initials);

            Autocomplete.EntitiesChanged += (s, e) =>
            {
                // меняем элементы записи
                var items = _autocomplete.GetCHIOs().ToList();
                HealthRecord.healthRecord.SetItems(items);
            };
            Autocomplete.ConfidencesChanged += (s, e) =>
            {
                // меняем уверенность заверешенных элементов
                var items = _autocomplete.GetCHIOsOfCompleted().ToList();
                HealthRecord.healthRecord.SetItems(items);
            };
        }

        #endregion AutoComplete

        /// <summary>
        /// Загружает запись в редактор.
        /// </summary>
        /// <param name="hr"></param>
        public void Load(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            if (HealthRecord != null && HealthRecord.healthRecord == hr)
                return;

            // ensure hr is not transient
            new Saver(session).Save(hr);

            FinishCurrentHr();

            hr.PropertyChanged += hr_PropertyChanged;
            (hr as IEditableObject).BeginEdit();

            try
            {
                // prevent saving hr during edit
                session.SetReadOnly(hr, true);
            }
            catch (TransientObjectException)
            {
                logger.WarnFormat("{0} still transient after save", hr);
            }

            HealthRecord = new HealthRecordViewModel(hr);
            CreateAutoComplete();
        }

        /// <summary>
        /// Выгружает редактируемую запись. Редактор будет закрыт без Closing.
        /// </summary>
        public void Unload()
        {
            if (HealthRecord != null)
            {
                var hr = HealthRecord.healthRecord;
                FinishCurrentHr();
                HealthRecord = null;

                OnClosed(hr);
            }
        }

        public override string ToString()
        {
            return string.Format("editor {0}", HealthRecord);
        }

        protected virtual void OnUnloaded(HealthRecord hr)
        {
            var h = Unloaded;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
            }
        }

        protected virtual void OnClosing(HealthRecord hr)
        {
            var h = Closing;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
            }
        }

        protected virtual void OnClosed(HealthRecord hr)
        {
            var h = Closed;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Unload();
                    if (_autocomplete != null)
                        _autocomplete.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void FinishCurrentHr()
        {
            Contract.Requires(HealthRecord == null || !HealthRecord.healthRecord.IsTransient);

            if (HealthRecord != null)
            {
                // завершаем теги
                var tag = Autocomplete.Tags.Where(t => t.State == State.Typing).FirstOrDefault();
                if (tag != null)
                    Autocomplete.CompleteOnLostFocus(tag);

                var hr = HealthRecord.healthRecord;
                HealthRecord.Dispose();

                hr.PropertyChanged -= hr_PropertyChanged;
                (hr as IEditableObject).EndEdit();

                session.SetReadOnly(hr, false);
                session.Evict(hr);

                Autocomplete.Dispose();
                _autocomplete = null;
                recognizer = null; // editor closed, created words persisted

                OnUnloaded(hr);
            }
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Category")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

    }
}
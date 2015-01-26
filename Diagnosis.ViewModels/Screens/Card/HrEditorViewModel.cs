using Diagnosis.Common;
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
    public class HrEditorViewModel : ViewModelBase
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
        /// <summary>
        /// For Xaml
        /// </summary>
        [Obsolete]
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

        public RelayCommand DeleteCommand
        {
            get
            {
                return new RelayCommand(() =>
                       {
                           HealthRecord.healthRecord.IsDeleted = true;
                       });
            }
        }

        public RelayCommand AddIcdCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var vm = new IcdSelectorViewModel();
                    this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                    if (vm.DialogResult == true)
                    {
                        Autocomplete.AddTag(vm.SelectedIcd);
                    }
                });
            }
        }

        public RelayCommand AddMeasureCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var vm = new MeasureEditorViewModel();
                    this.Send(Event.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                    if (vm.DialogResult == true)
                    {
                        Autocomplete.AddTag(vm.Measure);
                    }
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

        #region AutoComplete

        public AutocompleteViewModel Autocomplete { get { return _autocomplete; } }

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

            var initials = HealthRecord.healthRecord.GetOrderedEntities();
            recognizer = new Recognizer(session) { ShowChildrenFirst = true };
            _autocomplete = new AutocompleteViewModel(
                recognizer,
                true,
                true,
                false,
                initials);

            _autocomplete.EntitiesChanged += (s, e) =>
            {
                // меняем элементы записи
                var items = _autocomplete.GetEntities().ToList();
                HealthRecord.healthRecord.SetItems(items);
            };

            OnPropertyChanged("Autocomplete");
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

            FinishCurrentHr();

            HealthRecord = new HealthRecordViewModel(hr);

            hr.PropertyChanged += hr_PropertyChanged;

            (hr as IEditableObject).BeginEdit();
            if (!hr.IsTransient)
                session.SetReadOnly(hr, true);

            CreateAutoComplete();
        }

        /// <summary>
        /// Выгружает редактируемую запись. Редактор будет закрыт без Closing.
        /// </summary>
        public void Unload()
        {
            FinishCurrentHr();
            if (HealthRecord != null)
            {
                var hr = HealthRecord.healthRecord;
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
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void FinishCurrentHr()
        {
            if (HealthRecord != null)
            {
                // завершаем теги
                var tag = Autocomplete.Tags.Where(t => t.State == State.Typing).FirstOrDefault();
                if (tag != null)
                    Autocomplete.CompleteOnLostFocus(tag);

                var hr = HealthRecord.healthRecord;
                hr.PropertyChanged -= hr_PropertyChanged;
                (hr as IEditableObject).EndEdit();
                if (!hr.IsTransient)
                {
                    session.SetReadOnly(hr, false);
                    session.Evict(hr);
                }

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
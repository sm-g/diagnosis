using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
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
        private IEnumerable<HrCategory> _categories;
        private EventMessageHandler handler;

        public event EventHandler<DomainEntityEventArgs> Unloaded;
        private Recognizer recognizer;

        public HrEditorViewModel(ISession session)
        {
            this.session = session;

            handler = this.Subscribe(Event.SettingsSaved, (e) =>
             {
                 OnPropertyChanged(() => ShowIcdDiseaseSearch);
                 // после смены настроек доктора может понадобиться поиск по диагнозам
             });
        }

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
                    OnPropertyChanged("IsActive");
                }
            }
        }

        public IEnumerable<HrCategory> Categories
        {
            get
            {
                if (_categories == null)
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

        public bool IsActive
        {
            get
            {
                return HealthRecord != null;
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
                       }, () => IsActive && HealthRecord.healthRecord.IsDirty);
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

        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(Unload);
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

        public bool ShowIcdDiseaseSearch
        {
            get
            {
                return AuthorityController.CurrentDoctor.DoctorSettings.HasFlag(DoctorSettings.ShowIcdDisease);
            }
        }

        /// <summary>
        /// Загружает запись в редактор.
        /// </summary>
        /// <param name="hr"></param>
        public void Load(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            if (HealthRecord != null && HealthRecord.healthRecord == hr)
                return;

            CloseCurrentHr();

            HealthRecord = new HealthRecordViewModel(hr);
            hr.DateOffset.Settings = DateOffset.DateOffsetSettings.ExactSetting();

            hr.PropertyChanged += hr_PropertyChanged;

            (hr as IEditableObject).BeginEdit();
            if (!hr.IsTransient)
                session.SetReadOnly(hr, true);

            CreateAutoComplete();
        }

        /// <summary>
        /// Выгружает редактируемую запись.
        /// </summary>
        public void Unload()
        {
            CloseCurrentHr();
            HealthRecord = null;
        }

        private void CloseCurrentHr()
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
                OnUnloaded(hr);

                Autocomplete.Dispose();
                _autocomplete = null;
                recognizer = null; // editor closed, created words persisted
            }
        }

        private void hr_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Category")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        protected virtual void OnUnloaded(HealthRecord hr)
        {
            var h = Unloaded;
            if (h != null)
            {
                h(this, new DomainEntityEventArgs(hr));
            }
        }

        public override string ToString()
        {
            return string.Format("editor {0}", HealthRecord);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();
                    Unload();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
using Diagnosis.Common;
using Diagnosis.Data;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Controls.Autocomplete;
using EventAggregator;
using log4net;
using NHibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.ViewModels.Screens
{
    public class HrEditorViewModel : ViewModelBase, IClipboardTarget, IFocusable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HrEditorViewModel));

        private IHrEditorAutocomplete _autocomplete;
        private HealthRecordViewModel _hr;
        private ISession session;
        private bool _focused;
        private IEnumerable<HrCategory> _categories;
        private EventMessageHandler handler;

        public HrEditorViewModel(ISession session)
        {
            Contract.Assume(AuthorityController.CurrentDoctor != null);

            this.session = session;

            handler = this.Subscribe(Event.NewSession, (e) =>
            {
                var s = e.GetValue<ISession>(MessageKeys.Session);
                ReplaceSession(s);
            });
        }

        /// <summary>
        /// Запись выгружена, но редактор еще открыт.
        /// </summary>
        public event EventHandler<HealthRecordEventArgs> Unloaded;

        /// <summary>
        /// Редактор закрыт. Запись выгружается перед этим.
        /// </summary>
        public event EventHandler<HealthRecordEventArgs> Closed;

        /// <summary>
        /// Редактор закрывается по команде. Запись может быть null.
        /// </summary>
        public event EventHandler<HealthRecordEventArgs> Closing;


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
                    _categories = EntityQuery<HrCategory>.All(session)()
                        .Union(HrCategory.Null)
                        .OrderBy(cat => cat.Ord)
                        .ToList();
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
                    // not worked with collections
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

        public ICommand DeleteItemsCommand
        {
            get { return Autocomplete != null ? Autocomplete.DeleteCommand : null; }
        }

        public ICommand SendToSearchCommand
        {
            get { return Autocomplete != null ? Autocomplete.SendToSearchCommand : null; }
        }

        public ICommand ToggleSuggestionModeCommand
        {
            get { return Autocomplete != null ? Autocomplete.ToggleSuggestionModeCommand : null; }
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

        /// <summary>
        /// Закрывает редактор (даже без записи).
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var hr = HealthRecord != null ? HealthRecord.healthRecord : null;
                    OnClosing(hr);
                    Unload();
                    OnClosed(hr);
                });
            }
        }

        #endregion Commands

        #region AutoComplete

        public IHrEditorAutocomplete Autocomplete
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

        private void CreateAutoComplete()
        {
            if (Autocomplete != null)
            {
                Autocomplete.Dispose();
            }

            var initials = HealthRecord.healthRecord.GetOrderedCHIOs();
            var sugMaker = new SuggestionsMaker(session, AuthorityController.CurrentDoctor)
            {
                ShowChildrenFirst = true,
                AddQueryToSuggestions = AuthorityController.CurrentDoctor.Settings.AddQueryToSuggestions,
            };

            Autocomplete = new HrEditorAutocomplete(sugMaker, initials);

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
            Autocomplete.InputEnded += (s, e) =>
            {
                // завершаем выбранное перед добавлением записи
                if (e.value)
                {
                    var stratEdit = true;
                    this.Send(Event.AddHr, new object[] { HealthRecord.healthRecord.Holder, stratEdit }.AsParams(MessageKeys.Holder, MessageKeys.Boolean));
                }
            };
        }

        #endregion AutoComplete

        /// <summary>
        /// Загружает запись в редактор. Текущая запсиь выгружается, редактор осатется открытым.
        /// </summary>
        /// <param name="hr"></param>
        public void Load(HealthRecord hr)
        {
            Contract.Requires(hr != null);

            if (HealthRecord != null && HealthRecord.healthRecord == hr)
                return;

            // ensure hr is not transient
            session.DoSave(hr);

            FinishCurrentHr();

            hr.PropertyChanged += hr_PropertyChanged;
            (hr as IEditableObject).BeginEdit();

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
                var hr = HealthRecord != null ? HealthRecord.healthRecord : null;
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
                h(this, new HealthRecordEventArgs(hr));
            }
        }

        protected virtual void OnClosing(HealthRecord hr)
        {
            var h = Closing;
            if (h != null)
            {
                h(this, new HealthRecordEventArgs(hr));
            }
        }

        protected virtual void OnClosed(HealthRecord hr)
        {
            var h = Closed;
            if (h != null)
            {
                h(this, new HealthRecordEventArgs(hr));
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    handler.Dispose();

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
                Autocomplete.CompleteTypings();

                var hr = HealthRecord.healthRecord;
                HealthRecord.Dispose();

                hr.PropertyChanged -= hr_PropertyChanged;
                (hr as IEditableObject).EndEdit();

                var addQuery = Autocomplete.AddQueryToSuggestions;

                Autocomplete.Dispose();
                _autocomplete = null;

                // сохраняем запись
                OnUnloaded(hr);

                // сохраняем настройки редактора
                AuthorityController.CurrentDoctor.Settings.AddQueryToSuggestions = addQuery;
                session.DoSave(AuthorityController.CurrentDoctor);
            }
        }
        private void ReplaceSession(ISession s)
        {
            if (this.session.SessionFactory == s.SessionFactory)
                this.session = s;
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
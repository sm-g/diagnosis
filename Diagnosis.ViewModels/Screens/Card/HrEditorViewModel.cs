﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using EventAggregator;
using log4net;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Wintellect.PowerCollections;

namespace Diagnosis.ViewModels.Screens
{
    public class HrEditorViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HrEditorViewModel));

        private Autocomplete _autocomplete;

        private HealthRecordViewModel _hr;
        private ISession session;
        private IEnumerable<HrCategory> _categories;
        private EventMessageHandler handler;

        public event EventHandler<DomainEntityEventArgs> Unloaded;

        public HrEditorViewModel(ISession session)
        {
            this.session = session;

            handler = this.Subscribe(Events.SettingsSaved, (e) =>
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
                    _categories = new List<HrCategory>(session.Query<HrCategory>()
                        .OrderBy(cat => cat.Ord).ToList());
                }
                return _categories;
            }
        }

        public HrCategory Category
        {
            get
            {
                return HealthRecord != null ? HealthRecord.Category : null;
            }
            set
            {
                HealthRecord.Category = value;
            }
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
                    this.Send(Events.OpenDialog, vm.AsParams(MessageKeys.Dialog));
                    if (vm.DialogResult == true)
                    {
                        Autocomplete.AddTag(vm.SelectedIcd);
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

        public Autocomplete Autocomplete { get { return _autocomplete; } }

        /// <summary>
        /// Создает автокомплит с начальными словами и комментами из редактируемой записи.
        /// </summary>
        private void CreateAutoComplete()
        {
            if (Autocomplete != null)
                Autocomplete.Dispose();

            var initials = HealthRecord.healthRecord.GetOrderedEntities();

            _autocomplete = new Autocomplete(
                new Recognizer(session) { ShowChildrenFirst = true },
                true,
                initials);

            _autocomplete.EntitiesChanged += (s, e) =>
            {
                var entities = _autocomplete.GetEntities().ToList();
                // меняем элементы записи
                SetOrderedHrItems(HealthRecord.healthRecord, entities);
            };

            OnPropertyChanged("Autocomplete");
        }

        private static void SetOrderedHrItems(HealthRecord hr, List<IHrItemObject> entitiesToBe)
        {
            var hrEntities = hr.HrItems.Select(x => x.Entity).ToList();

            var willSet = new OrderedBag<IHrItemObject>(entitiesToBe);
            var wasSet = new OrderedBag<IHrItemObject>(hrEntities);
            var toA = willSet.Difference(wasSet);
            var toR = wasSet.Difference(willSet);

            logger.DebugFormat("set HrItems. IHrItemObject was: {0}, will: {1}", wasSet.FlattenString(), willSet.FlattenString());

            var itemsToRem = new List<HrItem>();
            var itemsToAdd = new List<HrItem>();

            // items to be in Hr = hr.HrItems - itemsToRem + itemsToAdd
            var itemsToBe = new List<HrItem>();

            // добалвяем все существующие, чьи сущности не надо убирать
            for (int i = 0; i < hrEntities.Count; i++)
            {
                var needRem = toR.Contains(hrEntities[i]);
                if (needRem)
                {
                    toR.Remove(hrEntities[i]);
                    itemsToRem.Add(hr.HrItems.ElementAt(i));
                }
                else
                {
                    itemsToBe.Add(hr.HrItems.ElementAt(i));
                }
            }
            // добавляем новые
            foreach (var item in toA)
            {
                var n = new HrItem(hr, item);
                itemsToAdd.Add(n);
                itemsToBe.Add(n);
            }

            logger.DebugFormat("set HrItems. itemsToAdd: {0}, itemsToRem: {1}", itemsToAdd.FlattenString(), itemsToRem.FlattenString());

            // индексы начала поиска в автокомплите для каждой сущности
            var dict = new Dictionary<IHrItemObject, int>();

            // ставим порядок
            for (int i = 0; i < itemsToBe.Count; i++)
            {
                var e = itemsToBe[i].Entity;
                int start = 0;
                dict.TryGetValue(e, out start);
                var index = entitiesToBe.IndexOf(e, start);

                Debug.Assert(index != -1, "entitiesToBe does not contain entity from itemsToBe");

                dict[e] = index + 1;
                itemsToBe[i].Ord = index;
            }

            logger.DebugFormat("set HrItems. itemsToBe: {0}", itemsToBe.FlattenString());

            foreach (var item in itemsToRem)
            {
                hr.RemoveItem(item);
            }
            // добавляем элементы уже с порядком
            foreach (var item in itemsToAdd)
            {
                hr.AddItem(item);
            }
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
            hr.PropertyChanged += hr_PropertyChanged;

            (hr as IEditableObject).BeginEdit();

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
                var tag = Autocomplete.Tags.Where(t => t.State == Tag.States.Typing).FirstOrDefault();
                if (tag != null)
                    Autocomplete.CompleteOnLostFocus(tag);

                HealthRecord.healthRecord.PropertyChanged -= hr_PropertyChanged;
                (HealthRecord.healthRecord as IEditableObject).EndEdit();
                OnUnloaded(HealthRecord.healthRecord);

                Autocomplete.Dispose();
                _autocomplete = null;
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
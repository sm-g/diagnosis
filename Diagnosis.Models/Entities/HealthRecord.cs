using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Wintellect.PowerCollections;

namespace Diagnosis.Models
{
    public class HealthRecord : EntityBase<Guid>, IDomainObject, IHaveAuditInformation, IComparable<HealthRecord>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HealthRecord));
        private ISet<HrItem> hrItems = new HashSet<HrItem>();
        private bool _isDeleted;
        private HrCategory _category;
        private HealthRecordUnit _unit;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private DateTime _describedAt;
        private DateOffset _fromDate;
        private DateOffset _toDate;
        private int _ord;
        private bool loaded;

        public HealthRecord(Appointment appointment, Doctor author)
            : this()
        {
            Contract.Requires(appointment != null);
            Contract.Requires(author != null);

            Appointment = appointment;
            Doctor = author;
        }

        public HealthRecord(Course course, Doctor author)
            : this()
        {
            Contract.Requires(course != null);
            Contract.Requires(author != null);

            Course = course;
            Doctor = author;
        }

        public HealthRecord(Patient patient, Doctor author)
            : this()
        {
            Contract.Requires(patient != null);
            Contract.Requires(author != null);

            Patient = patient;
            Doctor = author;
        }

        protected HealthRecord()
        {
            _createdAt = DateTime.Now;
            _updatedAt = _createdAt;
            _describedAt = _createdAt;
        }

        /// <summary>
        /// Добавление/удаление элементов, изменение порядка элементов (reset).
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ItemsChanged;

        /// <summary>
        /// Пациент, если запись на уровне пациента
        /// </summary>
        public virtual Patient Patient { get; protected set; }

        /// <summary>
        /// Курс, если запись на уровне курса
        /// </summary>
        public virtual Course Course { get; protected set; }

        /// <summary>
        /// Осмотр, если запись на уровне осмотра
        /// </summary>
        public virtual Appointment Appointment { get; protected set; }

        /// <summary>
        /// Автор записи
        /// </summary>
        public virtual Doctor Doctor { get; protected set; }

        /// <summary>
        /// В чьем списке запись
        /// </summary>
        public virtual IHrsHolder Holder
        {
            get
            {
                return (IHrsHolder)Patient ?? (IHrsHolder)Course ?? Appointment;
            }
        }

        /// <summary>
        /// Указывает, что запись помечена на удаление.
        /// </summary>
        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                SetProperty(ref _isDeleted, value, () => IsDeleted);
            }
        }

        public virtual HrCategory Category
        {
            get { return _category; }
            set
            {
                if (HrCategory.ConsideredNull(value)) value = null;
                SetProperty(ref _category, value, () => Category);
            }
        }

        public virtual DateOffset FromDate
        {
            get
            {
                if (_fromDate == null)
                {
                    _fromDate = new DateOffset(null, null, null, () => DescribedAt);
                    _fromDate.PropertyChanged += date_PropertyChanged;
                }
                return _fromDate;
            }
            protected set
            {
                if (SetProperty(ref _fromDate, value, () => FromDate) && _fromDate != null)
                {
                    _fromDate.PropertyChanged += date_PropertyChanged;
                };
            }
        }

        /// <summary>
        /// В БД может быть только ToDate, но это без смысла.
        /// </summary>
        public virtual DateOffset ToDate
        {
            get
            {
                if (_toDate == null)
                {
                    _toDate = new DateOffset(null, null, null, () => DescribedAt);
                    _toDate.PropertyChanged += date_PropertyChanged;
                }
                return _toDate;
            }
            protected set
            {
                if (SetProperty(ref _toDate, value, () => ToDate) && _toDate != null)
                {
                    _toDate.PropertyChanged += date_PropertyChanged;
                };
            }
        }

        /// <summary>
        /// Дата описания события.
        /// </summary>
        public virtual DateTime DescribedAt
        {
            get { return _describedAt; }
            set
            {
                if (SetProperty(ref _describedAt, value, () => DescribedAt))
                {
                    FromDate.Now = value;
                    ToDate.Now = value;
                }
            }
        }

        public virtual HealthRecordUnit Unit
        {
            get { return _unit; }
            set
            {
                SetProperty(ref _unit, value, () => Unit);
            }
        }

        public virtual bool IsClosedInterval { get { return ToDate != FromDate && !ToDate.IsEmpty; } }

        public virtual bool IsOpenedInterval { get { return ToDate != FromDate && ToDate.IsEmpty; } }

        public virtual bool IsPoint { get { return ToDate == FromDate; } }

        public virtual int Ord
        {
            get { return _ord; }
            set
            {
                SetProperty(ref _ord, value, () => Ord);
            }
        }

        public virtual DateTime CreatedAt
        {
            get { return _createdAt; }
        }

        public virtual DateTime UpdatedAt
        {
            get { return _updatedAt; }
        }

        DateTime IHaveAuditInformation.CreatedAt
        {
            get { return _updatedAt; }
            set
            {
                _createdAt = value;
            }
        }

        DateTime IHaveAuditInformation.UpdatedAt
        {
            get { return _updatedAt; }
            set { SetProperty(ref _updatedAt, value, () => UpdatedAt); }
        }

        /// <summary>
        /// Не использовать для сравнения записей по содержимому.
        /// Используем GetOrderedEntities()
        /// </summary>
        public virtual ISet<HrItem> HrItems
        {
            get { return hrItems; }
            protected internal set
            {
                hrItems = value;
                OnPropertyChanged("HrItems");
            }
        }

        public virtual IEnumerable<Measure> Measures
        {
            get { return hrItems.Where(x => x.Measure != null).Select(x => x.Measure); }
        }

        public virtual IEnumerable<Word> Words
        {
            get { return hrItems.Where(x => x.Word != null).Select(x => x.Word); }
        }

        /// <summary>
        /// Добавляет сущности к элементам записи.
        /// </summary>
        /// <param name="items"></param>
        public virtual void AddItems(IEnumerable<ConfindenceHrItemObject> items)
        {
            Contract.Requires(items != null);

            SetItems(this.GetOrderedCHIOs().Concat(items).ToList());
        }

        /// <summary>
        /// Добавляет сущности к элементам записи c уверенностью Present.
        /// </summary>
        /// <param name="items"></param>
        public virtual void AddItems(IEnumerable<IHrItemObject> items)
        {
            Contract.Requires(items != null);

            SetItems(this.GetOrderedCHIOs()
                .Concat(items.Select(x => new ConfindenceHrItemObject(x, Confidence.Present))).ToList());
        }

        /// <summary>
        /// Устанавливает сущности элементов записи c уверенностью Present.
        /// Полученные элементы нумеруются по порядку.
        /// </summary>
        /// <param name="entitiesToBe"></param>
        public virtual void SetItems(IList<IHrItemObject> entitiesToBe)
        {
            Contract.Requires(entitiesToBe != null);

            SetItems(entitiesToBe.Select(x => new ConfindenceHrItemObject(x, Confidence.Present)).ToList());
        }

        /// <summary>
        /// Устанавливает сущности элементов записи.
        /// Полученные элементы нумеруются по порядку.
        /// </summary>
        public virtual void SetItems(IList<ConfindenceHrItemObject> willChios)
        {
            Contract.Requires(willChios != null);
            Contract.Ensures(HrItems.Count == willChios.Count);
            Contract.Ensures(HrItems.Select(x => x.Entity)
                .ScrambledEquals(willChios.Select(x => x.HIO))); // same HIOs
            Contract.Ensures(HrItems.Select(x => x.Ord).Distinct().Count() == HrItems.Count); // Order is unique
            Contract.Ensures(HrItems.Select(x => x.Word).Where(x => x != null).All(x => x.HealthRecords.Contains(this))); // word2hr relation

            var hrItems = HrItems.ToList();
            var wasChios = hrItems.Select(x => x.GetConfindenceHrItemObject()).ToList();

            logger.DebugFormat("set HrItems. Chios was: {0}, will: {1}", wasChios.FlattenString(), willChios.FlattenString());

            // items to be in Hr = this.HrItems - itemsToRem + itemsToAdd
            var itemsToBe = new List<HrItem>();
            var itemsToRem = new List<HrItem>();
            var itemsToAdd = new List<HrItem>();

            var willBag = new Bag<ConfindenceHrItemObject>(willChios);
            var wasBag = new Bag<ConfindenceHrItemObject>(wasChios);

            // добалвяем все существующие, чьи сущности не надо убирать
            var toR = wasBag.Difference(willBag);
            for (int i = 0; i < wasChios.Count; i++)
            {
                if (toR.Contains(wasChios[i])) // убрать
                {
                    toR.Remove(wasChios[i]);
                    itemsToRem.Add(hrItems[i]);
                }
                else // оставить
                {
                    itemsToBe.Add(hrItems[i]);
                }
            }
            // добавляем новые

            var toA = willBag.Difference(wasBag);
            foreach (var item in toA)
            {
                var n = new HrItem(this, item.HIO) { Confidence = item.Confidence };
                itemsToAdd.Add(n);
                itemsToBe.Add(n);
            }

            logger.DebugFormat("set HrItems. itemsToAdd: {0}, itemsToRem: {1}", itemsToAdd.FlattenString(), itemsToRem.FlattenString());

            // индексы начала поиска в автокомплите для каждой сущности
            var dict = new Dictionary<IHrItemObject, int>();

            // ставим порядок
            bool reordered = false;
            var hiosToBe = willChios.Select(x => x.HIO).ToList();
            for (int i = 0; i < itemsToBe.Count; i++)
            {
                var e = itemsToBe[i].Entity;
                int start = 0;
                dict.TryGetValue(e, out start);
                var index = hiosToBe.IndexOf(e, start);

                Debug.Assert(index != -1, "hiosToBe does not contain entity from itemsToBe");

                dict[e] = index + 1;

                reordered = itemsToBe[i].Ord != index;
                itemsToBe[i].Ord = index;
            }

            logger.DebugFormat("set HrItems. itemsToBe: {0}", itemsToBe.FlattenString());

            foreach (var item in itemsToRem)
            {
                this.RemoveItem(item);
            }
            // добавляем элементы уже с порядком
            foreach (var item in itemsToAdd)
            {
                this.AddItem(item);
            }
            if (reordered || itemsToRem.Count > 0 || itemsToAdd.Count > 0)
            {
                OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public override string ToString()
        {
            return string.Format("hr({0}) {1} {2} {3}.{4}.{5} {6}", HrItems.FlattenString(), Ord, Category, FromDate.Year, FromDate.Month, FromDate.Day, this.ShortId());
        }

        public virtual int CompareTo(HealthRecord other)
        {
            // не сравниваем содержимое записи
            if (other == null)
                return -1;

            int res = Holder.CompareTo(other.Holder);
            if (res != 0) return res;

            res = Ord.CompareTo(other.Ord);
            if (res != 0) return res;

            res = UpdatedAt.CompareTo(other.UpdatedAt);
            if (res != 0) return res;

            res = Doctor.CompareTo(other.Doctor);
            return res;
        }

        protected internal virtual void OnDelete()
        {
            this.Words.ForEach(x => x.RemoveHr(this));
            Doctor.RemoveHr(this);
        }

        protected internal virtual void FixDescribedAtAfterLoad()
        {
            // Nhibernate components cannot use shared field
            // So after load set DateOffset.Now manually
            ToDate.Now = DescribedAt;
            FromDate.Now = DescribedAt;
            loaded = true;
        }

        protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = ItemsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        private void AddItem(HrItem item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(hrItems.Contains(item));

            var hrItemsCopy = new HashSet<HrItem>(hrItems);
            if (hrItems.Add(item))
            {
                // обновляем другую сторону many-2-many
                if (item.Word != null)
                    item.Word.AddHr(this);

                EditHelper.Edit("HrItems", hrItemsCopy);
                if (InEdit)
                {
                    IsDirty = true;
                }
            }
        }

        private void RemoveItem(HrItem item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(!hrItems.Contains(item));

            var hrItemsCopy = new HashSet<HrItem>(hrItems);
            if (hrItems.Remove(item))
            {
                // обновляем другую сторону many-2-many
                if (item.Word != null)
                    item.Word.RemoveHr(this);

                EditHelper.Edit("HrItems", hrItemsCopy);
                if (InEdit)
                {
                    IsDirty = true;
                }
            }
        }

        private void date_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Now")
                DescribedAt = (sender as DateOffset).Now;
            if (InEdit)
                IsDirty = true;
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(!loaded || ToDate.Now == FromDate.Now);
        }
    }
}
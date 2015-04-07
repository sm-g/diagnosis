using Diagnosis.Common;
using Iesi.Collections.Generic;
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
        private Iesi.Collections.Generic.ISet<HrItem> hrItems = new HashedSet<HrItem>();
        private int? _year;
        private int? _month;
        private int? _day;
        private bool _isDeleted;
        private HrCategory _category;
        private HealthRecordUnit _unit;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private int _ord;

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
            _updatedAt = DateTime.Now;
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

        public virtual int? FromDay
        {
            get { return _day; }
            set
            {
                SetProperty(ref _day, value, () => FromDay);
            }
        }

        public virtual int? FromMonth
        {
            get { return _month; }
            set
            {
                SetProperty(ref _month, value, () => FromMonth);
            }
        }

        public virtual int? FromYear
        {
            get { return _year; }
            set
            {
                SetProperty(ref _year, value, () => FromYear);
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
        public virtual Iesi.Collections.Generic.ISet<HrItem> HrItems
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

            SetItems(GetOrderedCHIOs().Concat(items).ToList());
        }

        /// <summary>
        /// Добавляет сущности к элементам записи c уверенностью Present.
        /// </summary>
        /// <param name="items"></param>
        public virtual void AddItems(IEnumerable<IHrItemObject> items)
        {
            Contract.Requires(items != null);

            SetItems(GetOrderedCHIOs()
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
        /// <param name="chiosToBe"></param>
        public virtual void SetItems(IList<ConfindenceHrItemObject> chiosToBe)
        {
            Contract.Requires(chiosToBe != null);
            Contract.Ensures(HrItems.Count == chiosToBe.Count);
            Contract.Ensures(HrItems.Select(x => x.Entity)
                .ScrambledEquals(chiosToBe.Select(x => x.HIO))); // same HIOs
            Contract.Ensures(HrItems.Select(x => x.Ord).Distinct().Count() == HrItems.Count); // Order is unique
            Contract.Ensures(HrItems.Select(x => x.Word).Where(x => x != null).All(x => x.HealthRecords.Contains(this))); // word2hr relation

            var hrChios = this.HrItems.Select(x => x.CHIO).ToList();
            var hiosToBe = chiosToBe.Select(x => x.HIO).ToList();

            var willSet = new OrderedBag<ConfindenceHrItemObject>(chiosToBe);
            var wasSet = new OrderedBag<ConfindenceHrItemObject>(hrChios);
            var toA = willSet.Difference(wasSet);
            var toR = wasSet.Difference(willSet);

            logger.DebugFormat("set HrItems. IHrItemObject was: {0}, will: {1}", wasSet.FlattenString(), willSet.FlattenString());

            var itemsToRem = new List<HrItem>();
            var itemsToAdd = new List<HrItem>();

            // items to be in Hr = this.HrItems - itemsToRem + itemsToAdd
            var itemsToBe = new List<HrItem>();

            // добалвяем все существующие, чьи сущности не надо убирать
            for (int i = 0; i < hrChios.Count; i++)
            {
                var needRem = toR.Contains(hrChios[i]);
                if (needRem)
                {
                    itemsToRem.Add(this.HrItems.ElementAt(i));
                }
                else
                {
                    itemsToBe.Add(this.HrItems.ElementAt(i));
                }
            }
            // добавляем новые
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
            for (int i = 0; i < itemsToBe.Count; i++)
            {
                var e = itemsToBe[i].Entity;
                int start = 0;
                dict.TryGetValue(e, out start);
                var index = hiosToBe.IndexOf(e, start);

                Debug.Assert(index != -1, "entitiesToBe does not contain entity from itemsToBe");

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

        public virtual IEnumerable<IHrItemObject> GetOrderedEntities()
        {
            return from item in HrItems
                   orderby item.Ord
                   select item.Entity;
        }

        public virtual IEnumerable<ConfindenceHrItemObject> GetOrderedCHIOs()
        {
            return from item in HrItems
                   orderby item.Ord
                   select item.CHIO;
        }

        public override string ToString()
        {
            return string.Format("hr({0}) {1} {2} {3}.{4}.{5} {6}", HrItems.FlattenString(), Ord, Category, FromYear, FromMonth, FromDay, this.ShortId());
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

            var hrItemsCopy = new HashedSet<HrItem>(hrItems);
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

            var hrItemsCopy = new HashedSet<HrItem>(hrItems);
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

        protected internal virtual void OnDelete()
        {
            this.Words.ForEach(x => x.RemoveHr(this));
            Doctor.RemoveHr(this);
        }
    }
}
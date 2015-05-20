using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("uom {Abbr} {Type} f{Factor}")]
    [Serializable]
    public class Uom : ValidatableEntity<Guid>, IDomainObject
    {
        public readonly static Uom Null = new NullUom();  // для измерения без единицы

        private ISet<UomFormat> formats = new HashSet<UomFormat>();

        private string _description;
        private string _abbr;
        private double _factor;
        private UomType _type;

        public Uom(string abbr, double factor, UomType type)
        {
            Contract.Requires(abbr != null);

            this.Abbr = abbr;
            this.Factor = factor;
            this.Type = type;
            type.AddUom(this);
        }

        protected Uom()
        {
        }

        public virtual event NotifyCollectionChangedEventHandler FormatsChanged;

        public virtual string Abbr
        {
            get { return _abbr; }
            set
            {
                Contract.Requires(value != null);

                var filtered = value.Trim();
                SetProperty(ref _abbr, filtered, () => Abbr);
            }
        }

        public virtual string Description
        {
            get { return _description; }
            set
            {
                var filtered = (value ?? "").Trim();
                SetProperty(ref _description, filtered, () => Description);
            }
        }

        /// <summary>
        /// Показатель степени множителя по основанию 10 относительно
        /// базовой единицы этого типа (единицы объема: -3 для мкл, 0 для мл).
        /// При сохранении в БД Measure.Value = Value * 10^Factor.
        /// </summary>
        public virtual double Factor
        {
            get { return _factor; }
            set
            {
                if (SetProperty(ref _factor, value, () => Factor))
                {
                    OnPropertyChanged(() => IsBase);
                }
            }
        }

        public virtual UomType Type
        {
            get { return _type; }
            set
            {
                SetProperty(ref _type, value, () => Type);
            }
        }

        public virtual IEnumerable<UomFormat> Formats { get { return formats; } }

        public virtual bool IsBase { get { return Factor == 0; } }

        public virtual void SetFormats(IEnumerable<UomFormat> formatsToBe)
        {
            Contract.Requires(!Formats.Any()); // только при создании единицы
            Contract.Requires(formatsToBe.All(x => x.Uom == null));

            var formatsToAdd = new List<UomFormat>(formatsToBe);

            foreach (var item in formatsToAdd)
            {
                item.Uom = this;
                formats.Add(item);
            }

            if (formatsToAdd.Count > 0)
            {
                OnFormatsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual void OnFormatsChanged(NotifyCollectionChangedEventArgs e)
        {
            var h = FormatsChanged;
            if (h != null)
            {
                h(this, e);
            }
        }

        public virtual string FormatValue(double value)
        {
            var f = Formats.Where(x => x.MeasureValue.Equals(value)).FirstOrDefault();
            if (f != null)
                return f.String;
            return value.ToString();
        }

        /// <summary>
        /// Nan, если не получилось.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual double ParseString(string str)
        {
            var f = Formats.Where(x => x.String.Equals(str, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (f != null)
                return f.MeasureValue;
            double d;
            if (double.TryParse(str, out d))
                return d;
            return double.NaN;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Description, Abbr);
        }

        public override ValidationResult SelfValidate()
        {
            return new UomValidator().Validate(this);
        }

        private sealed class NullUom : Uom
        {
            public NullUom()
                : base("без единицы", 1, new UomType("", int.MinValue))
            {
            }
        }
    }
}
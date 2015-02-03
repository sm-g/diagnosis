using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Iesi.Collections.Generic;
using FluentValidation.Results;
using Diagnosis.Models.Validators;

namespace Diagnosis.Models
{
    [Serializable]

    public class Uom : ValidatableEntity<int>, IDomainObject
    {
        public static Uom Null = new Uom("—", 1, -1);  // для измерения без единицы
        private string _description;
        private string _abbr;
        private double _factor;

        public virtual string Abbr
        {
            get { return _abbr; }
            set
            {
                var filtered = value.Trim();
                SetProperty(ref _abbr, filtered, () => Abbr);
            }
        }
        public virtual string Description
        {
            get { return _description; }
            set
            {
                var filtered = value.Trim();
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
                SetProperty(ref _factor, value, () => Factor);
            }
        }
        public virtual int Type { get; set; }

        public Uom(string abbr, double factor, int type)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(abbr));

            this.Abbr = abbr;
            this.Factor = factor;
            this.Type = type;
        }

        protected Uom() { }

        public override string ToString()
        {
            return string.Format("{0}", Abbr);
        }


        public override ValidationResult SelfValidate()
        {
            return new UomValidator().Validate(this);
        }
    }
}

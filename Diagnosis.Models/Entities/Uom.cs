using Diagnosis.Models.Validators;
using FluentValidation.Results;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Models
{
    [DebuggerDisplay("uom {Abbr} {Type} f{Factor}")]
    [Serializable]
    public class Uom : ValidatableEntity<Guid>, IDomainObject
    {
        public static Uom Null = new Uom("—", 1, new UomType("", int.MinValue));  // для измерения без единицы
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

        public virtual bool IsBase { get { return Factor == 0; } }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Description, Abbr);
        }

        public override ValidationResult SelfValidate()
        {
            return new UomValidator().Validate(this);
        }


    }
}
using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.Models.Validators;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class UomViewModel : CheckableBase, IExistTestable
    {
        internal readonly Uom uom;

        public UomViewModel(Uom u)
        {
            Contract.Requires(u != null);
            uom = u;
            this.validatableEntity = uom;
            uom.PropertyChanged += uom_PropertyChanged;
            if (uom.Type != null)
                uom.Type.PropertyChanged += Type_PropertyChanged;

            IsBase = uom.IsBase;
            ValueInBase = Math.Round(Math.Pow(10, -uom.Factor), Measure.Scale);
        }

        #region Model

        private double _valInBase;

        private bool _isBase;

        public string Description
        {
            get { return uom.Description; }
            set { uom.Description = value; }
        }

        public string Abbr
        {
            get { return uom.Abbr; }
            set { uom.Abbr = value; }
        }

        public string BaseSymbol
        {
            get
            {
                if (uom.Type == null || uom.Type.Base == null)
                    return "";
                return uom.Type.Base.Abbr;
            }
        }

        public double ValueInBase
        {
            get
            {
                return _valInBase;
            }
            set
            {
                if (_valInBase != value)
                {
                    _valInBase = value;
                    OnPropertyChanged(() => ValueInBase);
                    OnPropertyChanged(() => Factor);
                }
            }
        }

        /// <summary>
        /// Единица - базовая в типе.
        /// Т.к. единица не редактируется, нужно только пересчитать единицы типа на новую базу
        /// и не надо выбирать новую базу, если снимается этот флаг.
        /// </summary>
        public bool IsBase
        {
            get
            {
                return _isBase;
            }
            set
            {
                if (_isBase != value)
                {
                    _isBase = value;
                    OnPropertyChanged(() => IsBase);
                }
            }
        }

        public double Factor
        {
            get { return uom.Factor; }
        }

        public UomType Type
        {
            get { return uom.Type; }
            set
            {
                if (value == null)  // when clear combobox
                    return;
                uom.Type = value;
                OnPropertyChanged(() => BaseSymbol);
            }
        }

        #endregion Model

        public string TypeString
        {
            get { return uom.Type.ToString(); }
        }

        public bool Unsaved
        {
            get
            {
                return uom.IsDirty;
            }
        }

        public bool HasExistingValue { get; set; }

        public bool WasEdited { get; private set; }
        public string[] TestExistingFor
        {
            get { return new[] { "Description", "Abbr", "Type" }; }
        }
        string IExistTestable.ThisValueExistsMessage
        {
            get { return ""; }
        }

        public override string this[string columnName]
        {
            get
            {
                if (!WasEdited) return string.Empty;

                var results = uom.SelfValidate();
                if (results == null)
                    return string.Empty;

                var message = results.Errors
                    .Where(x => x.PropertyName == columnName)
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();

                // оригинальная ошибка валидации остается
                if (message == null && HasExistingValue && TestExistingFor.Contains(columnName))
                    message = "Такая единица уже есть.";

                if (columnName == "ValueInBase" && ValueInBase.CompareTo(0) <= 0)
                    return "Число единиц должно быть больше 0.";

                return message != null ? message : string.Empty;
            }
        }

        public override string ToString()
        {
            return uom.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                uom.PropertyChanged -= uom_PropertyChanged;
                if (uom.Type != null)
                    uom.Type.PropertyChanged -= Type_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        private void uom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            // validate linked fields
            if (TestExistingFor.Contains(e.PropertyName))
                OnPropertyChanged(TestExistingFor);

            WasEdited = true;
        }

        private void Type_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Base")
            {
                OnPropertyChanged(() => TypeString);
                OnPropertyChanged(() => IsBase);
            }
        }



    }
}
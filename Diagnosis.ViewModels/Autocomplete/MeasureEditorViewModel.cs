using Diagnosis.Data.Queries;
using Diagnosis.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace Diagnosis.ViewModels.Autocomplete
{
    public class MeasureEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MeasureEditorViewModel));

        private readonly bool withCompare;
        private List<Uom> _uoms;
        private string _val;
        private MeasureOperator _op;
        private bool _withAndValue;
        private string _andval;

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="measure"></param>
        public MeasureEditorViewModel(Measure measure, bool withCompare)
            : this(measure, null, withCompare)
        {
        }

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="word"></param>
        public MeasureEditorViewModel(Word word, bool withCompare)
            : this(null, word, withCompare)
        {
        }

        /// <summary>
        /// Create
        /// </summary>
        public MeasureEditorViewModel(bool withCompare)
            : this(null, null, withCompare)
        {
        }

        public MeasureEditorViewModel(Measure measure, Word w, bool withCompare)
        {
            this.withCompare = withCompare;

            _uoms = new List<Uom> { Uom.Null };
            var allUoms = UomQuery.Contains(Session)("");
            _uoms.AddRange(allUoms);

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(_uoms);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

            Operator = MeasureOperator.Equal;

            SetupMeasure(measure, w);
            CreateAutocomplete();

            Title = "Редактирование измерения";
            HelpTopic = "editmeasure";
            WithHelpButton = false;
        }

        public ITagsTrackableAutocomplete Autocomplete { get; private set; }

        public bool WithCompare { get { return withCompare; } }

        public bool WithAndValue
        {
            get
            {
                return _withAndValue && withCompare;
            }
            set
            {
                if (_withAndValue != value)
                {
                    _withAndValue = value;
                    OnPropertyChanged(() => WithAndValue);
                }
            }
        }

        public Word Word
        {
            get { return Measure.Word; }
            set
            {
                Measure.Word = value;

                // единица по слову
                if (value != null && value.Uom != null && Uom == null)
                    Uom = value.Uom;

                OnPropertyChanged(() => Word);
            }
        }

        public string Value
        {
            get { return _val; }
            set { _val = value; }
        }

        public string AndValue
        {
            get { return _andval; }
            set { _andval = value; }
        }

        public MeasureOperator Operator
        {
            get
            {
                return _op;
            }
            set
            {
                if (_op != value)
                {
                    _op = value;
                    WithAndValue = _op.IsBinary();
                    OnPropertyChanged(() => Operator);
                }
            }
        }

        public Uom Uom
        {
            get { return Measure.Uom; }
            set
            {
                Measure.Uom = value;
                OnPropertyChanged(() => Uom);

                // валидация
                OnPropertyChanged(() => Value);
                OnPropertyChanged(() => AndValue);
            }
        }

        public Measure Measure { get; private set; }

        public IEnumerable<MeasureOperator> Operators { get { return Enum.GetValues(typeof(MeasureOperator)).Cast<MeasureOperator>(); } }

        public IEnumerable<Uom> Uoms { get { return _uoms; } }

        public override bool CanOk
        {
            get
            {
                return Word != null && Measure != null && ValidateValue(Value) && (!WithAndValue || ValidateValue(AndValue));
            }
        }

        public override string this[string columnName]
        {
            get
            {
                bool isValueValid = true;
                if (columnName == "Value")
                    isValueValid = ValidateValue(Value);
                if (columnName == "AndValue" && WithAndValue)
                    isValueValid = ValidateValue(AndValue);

                if (!isValueValid)
                    return "Из этого не получается число.";
                return string.Empty;
            }
        }

        private bool ValidateValue(string str)
        {
            double d;
            return TryParseByFormat(str, out d);
        }

        protected override void OnOk()
        {
            double d;
            // пробуем заменить строку на число по формату, например 'I'->1
            if (!TryParseByFormat(Value, out d))
                throw new InvalidOperationException("Cannot save, Value is invalid.");
            Measure.Value = d;

            if (WithCompare)
            {
                var op = Measure as MeasureOp;
                op.Operator = Operator;

                if (!TryParseByFormat(AndValue, out d))
                    throw new InvalidOperationException("Cannot save, AndValue is invalid.");
                op.RightValue = d;
            }
        }

        protected override void OnCancel()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        private bool TryParseByFormat(string s, out double d)
        {
            if (Uom != null)
            {
                d = Uom.ParseString(s);
                return !double.IsNaN(d);
            }
            return double.TryParse(s, out d);
        }

        private void SetupMeasure(Measure measure, Word w)
        {
            var asOp = measure as MeasureOp;
            if (measure == null)
            {
                Measure = WithCompare ? new MeasureOp(MeasureOperator.Equal, 0) : new Measure(0);
                Measure.Word = w;
            }
            else
            {
                if (asOp != null)
                    Operator = asOp.Operator;

                Measure = WithCompare
                    ? new MeasureOp(Operator, measure.Value, measure.Uom) { RightValue = asOp.RightValue }
                    : new Measure(measure.Value, measure.Uom);
                Measure.Word = w ?? measure.Word; // новое слово или бывшее с измерением
            }
            Value = Measure.FormattedValue;
            AndValue = asOp == null ? Value : asOp.FormattedRightValue;
        }

        private void CreateAutocomplete()
        {
            Autocomplete = new MeasureAutocomplete(
                new SuggestionsMaker(Session)
                {
                    AddQueryToSuggestions = true,
                    CanChangeAddQueryToSuggstions = false
                },
                Word == null ? null : new[] { new ConfWithHio(Word, Confidence.Present) })
            {
                IsDragSourceEnabled = false,
                IsDropTargetEnabled = false
            };
            Autocomplete.EntitiesChanged += (s, e) =>
            {
                var wordHio = Autocomplete.GetCHIOs().FirstOrDefault();
                Word = wordHio != null ? wordHio.HIO as Word : null;
            };
        }
    }
}
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
        private bool isValueValid;
        private bool _withAndValue;
        private string _andval;

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="measure"></param>
        public MeasureEditorViewModel(Measure measure, bool c)
            : this(measure, null, c)
        {
        }

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="word"></param>
        public MeasureEditorViewModel(Word word, bool c)
            : this(null, word, c)
        {
        }

        /// <summary>
        /// Create
        /// </summary>
        public MeasureEditorViewModel(bool c)
            : this(null, null, c)
        {
        }

        private MeasureEditorViewModel(Measure measure, Word w, bool withCompare)
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

        public AutocompleteViewModel Autocomplete { get; private set; }

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
            set { Measure.Word = value; }
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
            set { Measure.Uom = value; }
        }

        public Measure Measure { get; private set; }

        public IEnumerable<MeasureOperator> Operators { get { return Enum.GetValues(typeof(MeasureOperator)).Cast<MeasureOperator>(); } }

        public IEnumerable<Uom> Uoms { get { return _uoms; } }

        public override bool CanOk
        {
            get
            {
                return Word != null && Measure != null && isValueValid;
            }
        }

        public override string this[string columnName]
        {
            get
            {
                double d;
                if (columnName == "Value")
                    isValueValid = double.TryParse(Value, out d);
                if (columnName == "AndValue" && WithAndValue)
                    isValueValid = double.TryParse(AndValue, out d);

                if (!isValueValid)
                    return "Из этого не получается число.";
                return string.Empty;
            }
        }

        protected override void OnOk()
        {
            if (WithCompare)
            {
                var op = Measure as MeasureOp;
                op.Operator = Operator;
                op.RightValue = double.Parse(AndValue);
                op.Value = double.Parse(Value); // corrects RightBetweenValue
            }
            else
            {
                Measure.Value = double.Parse(Value);
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
            Value = Measure.Value.ToString();
            AndValue = asOp == null ? Value : asOp.RightValue.ToString();
        }

        private void CreateAutocomplete()
        {
            Autocomplete = new AutocompleteViewModel(
                new SuggestionsMaker(Session)
                {
                    AddQueryToSuggestions = true,
                    CanChangeAddQueryToSuggstions = false
                },
                AutocompleteViewModel.OptionsMode.MeasureEditor,
                Word == null ? null : new[] { new ConfindenceHrItemObject(Word, Confidence.Present) })
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
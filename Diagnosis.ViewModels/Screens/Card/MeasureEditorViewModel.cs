using Diagnosis.Models;
using Diagnosis.ViewModels.Search.Autocomplete;
using log4net;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class MeasureEditorViewModel : DialogViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MeasureEditorViewModel));

        private List<Uom> _uoms;
        MeasureEditorViewModel(Measure measure, Word w)
        {
            _uoms = new List<Uom> { Uom.Null };
            _uoms.AddRange(Session.Query<Uom>()
                .OrderBy(s => s.Abbr));

            if (measure == null)
            {
                Measure = new Measure(0) { Word = w };
            }
            else
            {
                Measure = new Measure(measure.Value, measure.Uom)
                {
                    Word = w ?? measure.Word  // новое слово или бывшее с измерением
                };
            }

            Autocomplete = new Search.Autocomplete.Autocomplete(
                new Recognizer(Session) { OnlyWords = true },
                false,
                false,
                Word == null ? null : new[] { Word });
            Autocomplete.EntitiesChanged += (s, e) =>
            {
                var entities = Autocomplete.GetEntities().ToList();
                Word = entities.FirstOrDefault() as Word;
            };

            Title = "Редактирование измерения";
        }
        public MeasureEditorViewModel(Measure measure)
            : this(measure, null)
        {

        }

        public MeasureEditorViewModel(Word word)
            : this(null, word)
        {
        }
        public MeasureEditorViewModel()
            : this(null, null)
        {
        }

        public Autocomplete Autocomplete { get; private set; }

        public double Value
        {
            get { return Measure.Value; }
            set { Measure.Value = value; }
        }

        public Uom Uom
        {
            get { return Measure.Uom; }
            set { Measure.Uom = value; }
        }

        public Word Word
        {
            get { return Measure.Word; }
            set { Measure.Word = value; }
        }
        public Measure Measure { get; private set; }

        public IEnumerable<Uom> Uoms { get { return _uoms; } }

        public override bool CanOk
        {
            get
            {
                return Word != null && Measure != null;
            }
        }

        //public override string this[string columnName]
        //{
        //    get
        //    {
        //        var results = appointment.SelfValidate();
        //        if (results == null)
        //            return string.Empty;
        //        var message = results.Errors
        //            .Where(x => x.PropertyName == columnName)
        //            .Select(x => x.ErrorMessage)
        //            .FirstOrDefault();
        //        return message != null ? message : string.Empty;
        //    }
        //}

        protected override void OnOk()
        {
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
    }
}
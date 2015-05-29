using AutoMapper;
using Diagnosis.Common;
using Diagnosis.Data.DTOs;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Diagnosis.Data
{
    public abstract class OptionsLoader
    {
        public const string JsonFormat = "json";
        private ISession session;

        static OptionsLoader()
        {
            ModelDtosMapper.Map();
        }

        public OptionsLoader(ISession session)
        {
            this.session = session;
        }

        public abstract string Format { get; }

        public static OptionsLoader FromFormat(string format, ISession session)
        {
            if (format == JsonFormat)
            {
                return new JsonOptionsLoader(session);
            }
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Десериализует опции. Возвращает null в случае ошибки.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public SearchOptions ReadOptions(string str)
        {
            try
            {
                var dto = ReadOptionsInner(str);
                var opt = LoadFromDTO(dto);
                opt.SetIsRoot();
                return opt;
            }
            catch
            {
                return null;
            }
        }

        public string WriteOptions(SearchOptions options)
        {
            Contract.Requires(options != null);

            options.Minimize();
            var dto = MapToDto(options);
            return WriteOptionsInner(dto);
        }

        /// <summary>
        /// Меняем текст слова прямо в сериализованной строке запроса.
        /// </summary>
        public string ReplaceWord(string options, string oldTitle, string newTitle)
        {
            Contract.Requires(options != null);
            Contract.Requires(oldTitle != null);
            Contract.Requires(newTitle != null);

            return ReplaceWordInner(options, oldTitle, newTitle);
        }

        protected abstract string ReplaceWordInner(string options, string oldTitle, string newTitle);

        protected abstract SearchOptionsDTO ReadOptionsInner(string str);

        protected abstract string WriteOptionsInner(SearchOptionsDTO dto);

        protected SearchOptionsDTO MapToDto(SearchOptions options)
        {
            return Mapper.Map<SearchOptionsDTO>(options);
        }

        /// <summary>
        /// Делает опции с реальными сущностями.
        /// </summary>
        protected SearchOptions LoadFromDTO(SearchOptionsDTO dto)
        {
            Contract.Requires(dto != null);
            Contract.Ensures(dto.GetAllChildrenCount() == Contract.Result<SearchOptions>().GetAllChildrenCount());

            var result = new SearchOptions();

            // some options
            bool sscopeNotParsed = false;
            bool grOpNotParsed = false;
            QueryGroupOperator groupOperator;
            if (!Enum.TryParse<QueryGroupOperator>(dto.GroupOperator, out groupOperator))
            {
                groupOperator = QueryGroupOperator.All;
                grOpNotParsed = true;
            }
            SearchScope sscope;
            if (!Enum.TryParse<SearchScope>(dto.SearchScope, out sscope))
            {
                sscope = SearchScope.HealthRecord;
                sscopeNotParsed = true;
            }

            result.GroupOperator = groupOperator;
            result.SearchScope = sscope;
            result.MinAny = dto.MinAny;
            result.WithConf = dto.WithConf;

            // words
            var allWordsTitles = dto.CWordsAll.Union(dto.CWordsAny).Union(dto.CWordsNot).Select(x => x.Title);
            var words = WordQuery.ByTitles(session)(allWordsTitles);

            bool confNotParsedAll;
            bool confNotParsedAny;
            bool confNotParsedNot;
            result.CWordsAll = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsAll, words, out confNotParsedAll));
            result.CWordsAny = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsAny, words, out confNotParsedAny));
            result.CWordsNot = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsNot, words, out confNotParsedNot));

            // measures
            bool mWordsMissed;
            bool uomsMissed;
            var mWords = LoadMeasureWords(dto, out mWordsMissed);
            var uoms = LoadUoms(dto, out uomsMissed);

            bool mopNotParsed;
            var mAll = SelectMeasures(dto.MeasuresAll, mWords, uoms, out mopNotParsed);
            var mAny = SelectMeasures(dto.MeasuresAny, mWords, uoms, out mopNotParsed);

            result.MeasuresAll = new List<MeasureOp>(mAll);
            result.MeasuresAny = new List<MeasureOp>(mAny);

            // cats
            var cats = CategoryQuery.ByTitles(session)(dto.Categories.Select(x => x.Title));
            result.Categories = new List<HrCategory>(cats);

            // childs
            dto.Children.ForAll(x =>
            {
                var child = this.LoadFromDTO(x);
                result.Children.Add(child);
            });

            var smthMissed =
                result.CWordsAll.Count != dto.CWordsAll.Count ||
                result.CWordsAny.Count != dto.CWordsAny.Count ||
                result.CWordsNot.Count != dto.CWordsNot.Count ||
                result.MeasuresAll.Count != dto.MeasuresAll.Count ||
                result.MeasuresAny.Count != dto.MeasuresAny.Count ||
                result.Categories.Count != dto.Categories.Count ||
                mWordsMissed ||
                uomsMissed;

            var notParsed = confNotParsedAll || confNotParsedAny || confNotParsedNot || sscopeNotParsed || grOpNotParsed || mopNotParsed;

            if (smthMissed || notParsed || result.Children.Any(x => x.PartialLoaded))
                result.SetPartialLoaded();

            return result;
        }

        private List<Uom> LoadUoms(SearchOptionsDTO dto, out bool uomsMissed)
        {
            var uomSpecs = dto.MeasuresAll
                        .Union(dto.MeasuresAny)
                        .Where(x => x.UomDescr != null)
                        .Select(x => new { Abbr = x.Abbr, Descr = x.UomDescr, TypeName = x.UomTypeTitle == null ? null : x.UomTypeTitle })
                        .Distinct()
                        .ToList();
            var uoms = uomSpecs
                .Select(x => UomQuery.ByAbbrDescrAndTypeName(session)(x.Abbr, x.Descr, x.TypeName))
                .Where(x => x != null)
                .ToList();

            uomsMissed = uomSpecs.Count != uoms.Count;
            return uoms;
        }

        private IEnumerable<Word> LoadMeasureWords(SearchOptionsDTO dto, out bool mWordsMissed)
        {
            var mWordTitles = (from w in dto.MeasuresAll
                                            .Union(dto.MeasuresAny)
                                            .Select(x => x.Word)
                               where w != null
                               select w.Title).Distinct().ToList();
            var mWords = WordQuery.ByTitles(session)(mWordTitles).ToList();

            mWordsMissed = mWordTitles.Count != mWords.Count;
            return mWords;
        }

        private IEnumerable<MeasureOp> SelectMeasures(IEnumerable<MeasureOpDTO> mopDtos, IEnumerable<Word> mWords, IEnumerable<Uom> uoms, out bool mopNotParsed)
        {
            MeasureOperator op;
            var opNP = false;
            var res = mopDtos.Select(x =>
            {
                if (!Enum.TryParse<MeasureOperator>(x.Operator, out op))
                {
                    op = MeasureOperator.GreaterOrEqual;
                    opNP = true;
                }

                return new MeasureOp(op, x.Value)
                {
                    Uom = x.UomDescr == null ? null : uoms.FirstOrDefault(u => x.UomEquals(u)),
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title),
                    RightValue = x.RightValue
                };
            });
            mopNotParsed = opNP;
            return res;
        }

        private IEnumerable<Confindencable<Word>> SelectConfWords(IEnumerable<ConfWordDTO> cwords, IEnumerable<Word> words, out bool confNotParsed)
        {
            Confidence conf;
            confNotParsed = false;
            var res = new List<Confindencable<Word>>();
            foreach (var cw in cwords)
            {
                var word = words.FirstOrDefault(w => w.Title == cw.Title);
                if (word != null)
                {
                    if (!Enum.TryParse<Confidence>(cw.Confidence, out conf))
                    {
                        conf = Confidence.Present;
                        confNotParsed = true;
                    }
                    res.Add(word.AsConfidencable(conf));
                }
            }

            return res;
        }
    }

    public class JsonOptionsLoader : OptionsLoader
    {
        public JsonOptionsLoader(ISession session)
            : base(session)
        {
        }

        public override string Format
        {
            get { return JsonFormat; }
        }

        protected override SearchOptionsDTO ReadOptionsInner(string str)
        {
            return str.DeserializeDCJson<SearchOptionsDTO>();
        }

        protected override string WriteOptionsInner(SearchOptionsDTO dto)
        {
            return dto.SerializeDCJson();
        }

        protected override string ReplaceWordInner(string options, string oldTitle, string newTitle)
        {
            // "Title":"АД"
            return Regex.Replace(options, "(?<=Title\":\")" + oldTitle + "(?=\")", newTitle);
        }
    }

    public static class SearchOptionsExtensions
    {
        [Pure]
        public static int GetAllChildrenCount(this SearchOptions o)
        {
            Contract.Requires(o != null);
            return o.Children.Aggregate(o.Children.Count, (x, d) => x + GetAllChildrenCount(d));
        }

        [Pure]
        public static int GetAllChildrenCount(this SearchOptionsDTO dto)
        {
            Contract.Requires(dto != null);

            return dto.Children.Aggregate(dto.Children.Count, (x, d) => x + GetAllChildrenCount(d));
        }
    }
}
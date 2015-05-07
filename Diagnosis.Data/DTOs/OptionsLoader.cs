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

namespace Diagnosis.Data
{
    public abstract class OptionsLoader
    {
        private ISession session;

        public OptionsLoader(ISession session)
        {
            this.session = session;
        }

        public abstract SearchOptions ReadOptions(string str);

        public abstract string WriteOptions(SearchOptions options);

        protected SearchOptionsDTO MapToDto(SearchOptions options)
        {
            return Mapper.Map<SearchOptionsDTO>(options);
        }

        /// <summary>
        /// Делает опции с реальными сущностями.
        /// </summary>
        protected SearchOptions LoadFromDTO(SearchOptionsDTO dto)
        {
            Contract.Ensures(dto.GetAllChildrenCount() == Contract.Result<SearchOptions>().GetAllChildrenCount());

            var result = new SearchOptions();
            result.GroupOperator = dto.GroupOperator;
            result.SearchScope = dto.SearchScope;
            result.MinAny = dto.MinAny;
            result.WithConf = dto.WithConf;

            var allWordsTitles = dto.CWordsAll.Union(dto.CWordsAny).Union(dto.CWordsNot).Select(x => x.Title);
            var words = WordQuery.ByTitles(session)(allWordsTitles);

            bool confNotParsedAll;
            bool confNotParsedAny;
            bool confNotParsedNot;
            result.CWordsAll = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsAll, words, out confNotParsedAll));
            result.CWordsAny = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsAny, words, out confNotParsedAny));
            result.CWordsNot = new List<Confindencable<Word>>(SelectConfWords(dto.CWordsNot, words, out confNotParsedNot));
            var notParsed = confNotParsedAll || confNotParsedAny || confNotParsedNot;

            // measures
            var mWordTitles = (from w in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Word)
                               where w != null
                               select w.Title).Distinct().ToList();

            var mWords = WordQuery.ByTitles(session)(mWordTitles).ToList();

            var uomSpecs = (from u in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Uom)
                            where u != null
                            select new { Abbr = u.Abbr, Descr = u.Description, TypeName = u.Type == null ? null : u.Type.Title })
                           .Distinct().ToList();
            var uoms = uomSpecs
                .Select(x => UomQuery.ByAbbrDescrAndTypeName(session)(x.Abbr, x.Descr, x.TypeName))
                .Where(x => x != null)
                .ToList();

            var mAll = dto.MeasuresAll.Select(x =>
                new MeasureOp(x.Operator, x.Value)
                {
                    Uom = x.Uom == null ? null : uoms.FirstOrDefault(u => x.Uom.Equals(u)),
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title),
                    RightValue = x.RightValue
                });
            var mAny = dto.MeasuresAny.Select(x =>
                new MeasureOp(x.Operator, x.Value)
                {
                    Uom = x.Uom == null ? null : uoms.FirstOrDefault(u => x.Uom.Equals(u)),
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title),
                    RightValue = x.RightValue
                });

            result.MeasuresAll = new List<MeasureOp>(mAll);
            result.MeasuresAny = new List<MeasureOp>(mAny);

            // cats
            var cats = CategoryQuery.ByTitles(session)(dto.Categories.Select(x => x.Title));
            result.Categories = new List<HrCategory>(cats);

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
                mWords.Count != mWordTitles.Count() ||
                uoms.Count != uomSpecs.Count();

            if (smthMissed || notParsed || result.Children.Any(x => x.PartialLoaded))
                result.SetPartialLoaded();

            return result;
        }

        private static IEnumerable<Confindencable<Word>> SelectConfWords(IEnumerable<ConfWordDTO> cwords, IEnumerable<Word> words, out bool confNotParsed)
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

        public override SearchOptions ReadOptions(string str)
        {
            var dto = str.DeserializeDCJson<SearchOptionsDTO>();
            var opt = LoadFromDTO(dto);
            return opt;
        }

        public override string WriteOptions(SearchOptions options)
        {
            var dto = MapToDto(options);
            return dto.SerializeDCJson();
        }
    }

    public static class SearchOptionsExtensions
    {
        public static int GetAllChildrenCount(this SearchOptions o)
        {
            return o.Children.Aggregate(o.Children.Count, (x, d) => x + GetAllChildrenCount(d));
        }

        public static int GetAllChildrenCount(this SearchOptionsDTO dto)
        {
            return dto.Children.Aggregate(dto.Children.Count, (x, d) => x + GetAllChildrenCount(d));
        }
    }
}
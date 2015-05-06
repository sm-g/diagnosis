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

            // words
            var words = WordQuery.ByTitles(session)(dto.WordsAll.Select(x => x.Title));
            result.WordsAll = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsAny.Select(x => x.Title));
            result.WordsAny = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsNot.Select(x => x.Title));
            result.WordsNot = new List<Word>(words);

            // measures
            var mWordTitles = from w in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Word)
                              where w != null
                              select w.Title;

            var mWords = WordQuery.ByTitles(session)(mWordTitles).ToList();

            var uomSpecs = from u in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Uom)
                           where u != null
                           select new { Abbr = u.Abbr, Descr = u.Description, TypeName = u.Type == null ? null : u.Type.Title };
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

            var smthMissed = result.WordsAll.Count != dto.WordsAll.Count ||
                result.WordsAny.Count != dto.WordsAny.Count ||
                result.WordsNot.Count != dto.WordsNot.Count ||
                result.MeasuresAll.Count != dto.MeasuresAll.Count ||
                result.MeasuresAny.Count != dto.MeasuresAny.Count ||
                result.Categories.Count != dto.Categories.Count ||
                mWords.Count != mWordTitles.Count() ||
                uoms.Count != uomSpecs.Count();

            result.PartialLoaded = smthMissed || result.Children.Any(x => x.PartialLoaded);
            return result;
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
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

            var words = WordQuery.ByTitles(session)(dto.WordsAll.Select(x => x.Title));
            result.WordsAll = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsAny.Select(x => x.Title));
            result.WordsAny = new List<Word>(words);

            words = WordQuery.ByTitles(session)(dto.WordsNot.Select(x => x.Title));
            result.WordsNot = new List<Word>(words);

            var mWordTitles = from w in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Word)
                              where w != null
                              select w.Title;
            var mWords = WordQuery.ByTitles(session)(mWordTitles);

            var uomSpecs = from u in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Uom)
                           where u != null
                           select new { Abbr = u.Abbr, Descr = u.Description, TypeName = u.Type == null ? null : u.Type.Title };
            var uoms = uomSpecs.Select(x => UomQuery.ByAbbrDescrAndTypeName(session)(x.Abbr, x.Descr, x.TypeName));

            var mAll = dto.MeasuresAll.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, x.Uom == null ? null : uoms.FirstOrDefault(u => Equal(u, x.Uom)))
                {
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title)
                });
            var mAny = dto.MeasuresAny.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, x.Uom == null ? null : uoms.FirstOrDefault(u => Equal(u, x.Uom)))
                {
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title)
                });

            result.MeasuresAll = new List<MeasureOp>(mAll);
            result.MeasuresAny = new List<MeasureOp>(mAny);

            var cats = CategoryQuery.ByTitles(session)(dto.Categories.Select(x => x.Title));
            result.Categories = new List<HrCategory>(cats);

            dto.Children.ForAll(x =>
            {
                var child = this.LoadFromDTO(x);
                result.Children.Add(child);
            });

            if (result.WordsAll.Count != dto.WordsAll.Count ||
                result.WordsAny.Count != dto.WordsAny.Count ||
                result.WordsNot.Count != dto.WordsNot.Count ||
                result.MeasuresAll.Count != dto.MeasuresAll.Count ||
                result.MeasuresAny.Count != dto.MeasuresAny.Count ||
                result.Categories.Count != dto.Categories.Count
                )
            {
                // чего-то нет на клиенте, запрос не такой, каким был сохранен
                result.PartialLoaded = true;
            }

            return result;
        }

        private bool Equal(Uom u, UomDTO dto)
        {
            return u.Abbr == dto.Abbr &&
                u.Description == dto.Description &&
                (dto.Type == null && u.Type == null ||
                u.Type.Title == dto.Type.Title);
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
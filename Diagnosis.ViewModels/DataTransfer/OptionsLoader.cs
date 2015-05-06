using Diagnosis.Common;
using Diagnosis.Data.DTOs;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.DataTransfer
{
    public class OptionsLoader
    {
        private ISession session;

        public OptionsLoader(ISession session)
        {
            this.session = session;
        }

        /// <summary>
        /// Делает опции с реальными сущностями.
        /// </summary>
        public SearchOptions LoadFromDTO(SearchOptionsDTO dto)
        {
            Contract.Ensures(dto.GetAllChildrenCount() == Contract.Result<SearchOptions>().GetAllChildrenCount());

            var result = new SearchOptions();
            result.GroupOperator = dto.GroupOperator;
            result.SearchScope = dto.SearchScope;
            result.MinAny = dto.MinAny;
            result.WithConf = dto.WithConf;

            var allWordsTitles = dto.CWordsAll.Union(dto.CWordsAny).Union(dto.CWordsNot).Select(x => x.Title);
            var words = WordQuery.ByTitles(session)(allWordsTitles);

            result.CWordsAll = new List<Confindencable<Word>>(from cw in dto.CWordsAll
                                                              let w = words.FirstOrDefault(w => w.Title == cw.Title)
                                                              where w != null
                                                              select new Confindencable<Word>(w, cw.Confidence));

            result.CWordsAny = new List<Confindencable<Word>>(from cw in dto.CWordsAny
                                                              let w = words.FirstOrDefault(w => w.Title == cw.Title)
                                                              where w != null
                                                              select new Confindencable<Word>(w, cw.Confidence));

            result.CWordsNot = new List<Confindencable<Word>>(from cw in dto.CWordsNot
                                                              let w = words.FirstOrDefault(w => w.Title == cw.Title)
                                                              where w != null
                                                              select new Confindencable<Word>(w, cw.Confidence));

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

            if (
                 result.CWordsAll.Count != dto.CWordsAll.Count ||
                 result.CWordsAny.Count != dto.CWordsAny.Count ||
                 result.CWordsNot.Count != dto.CWordsNot.Count ||
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
        bool Equal(Uom u, UomDTO dto)
        {
            return u.Abbr == dto.Abbr &&
                u.Description == dto.Description &&
                (dto.Type == null && u.Type == null ||
                u.Type.Title == dto.Type.Title);
        }
    }

    public static class Ex
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
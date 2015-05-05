﻿using Diagnosis.Common;
using Diagnosis.Data.Queries;
using Diagnosis.Models;
using Diagnosis.ViewModels.Search;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

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

            var uomTitles = from u in dto.MeasuresAll
                                         .Union(dto.MeasuresAny)
                                         .Select(x => x.Uom)
                            where u != null
                            select new { Abbr = u.Abbr, TypeName = u.UomType.Title };
            var uoms = uomTitles.Select(x => UomQuery.ByAbbrAndTypeName(session)(x.Abbr, x.TypeName));

            var mAll = dto.MeasuresAll.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, uoms.FirstOrDefault(u => u.Abbr == x.Uom.Abbr))
                {
                    Word = x.Word == null ? null : mWords.FirstOrDefault(w => w.Title == x.Word.Title)
                });
            var mAny = dto.MeasuresAny.Select(x =>
                new MeasureOp(x.Operator, x.DbValue, uoms.FirstOrDefault(u => u.Abbr == x.Uom.Abbr))
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
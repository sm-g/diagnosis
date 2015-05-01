using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.Serialization;
using NHibernate;
using Diagnosis.Data.Queries;
using Diagnosis.Data.DTOs;
using AutoMapper;

namespace Diagnosis.ViewModels.Search
{

    [Serializable]

    public class SearchOptions
    {
        [NonSerialized]
        private bool _isRoot;
        [NonSerialized]
        private bool _part;

        public SearchOptions(bool isRoot)
        {
            Children = new ObservableCollection<SearchOptions>();

            WordsAll = new List<Word>();
            WordsAny = new List<Word>();
            WordsNot = new List<Word>();
            MeasuresAny = new List<MeasureOp>();
            MeasuresAll = new List<MeasureOp>();
            Categories = new List<HrCategory>();

            _isRoot = isRoot;
        }
        public SearchOptions()
            : this(false)
        {

        }

        /// <summary>
        /// Записи со всеми словами
        /// </summary>
        public List<Word> WordsAll { get; set; }

        /// <summary>
        /// И любым словом из
        /// </summary>
        public List<Word> WordsAny { get; set; }

        /// <summary>
        /// Хотя бы столько элементов из Any
        /// </summary>
        public int MinAny { get; set; }

        /// <summary>
        /// B ни одного слова из.
        /// </summary>
        public List<Word> WordsNot { get; set; }

        /// <summary>
        /// Записи со всеми измерениями
        /// </summary>
        public List<MeasureOp> MeasuresAll { get; set; }

        /// <summary>
        /// И любым из
        /// </summary>
        public List<MeasureOp> MeasuresAny { get; set; }

        /// <summary>
        /// Категория. Если несколько, то любая их них.
        /// </summary>
        public List<HrCategory> Categories { get; set; }

        public QueryGroupOperator GroupOperator { get; set; }

        public SearchScope SearchScope { get; set; }

        public ObservableCollection<SearchOptions> Children { get; private set; }

        public bool IsGroup { get { return Children.Count > 0; } }
        /// <summary>
        /// Разрешает искать исключающий блок-корень.
        /// </summary>
        public bool IsRoot { get { return _isRoot; } }
        public bool IsExcluding
        {
            get
            {
                return !IsGroup && !WordsAll.Any() &&
                                   !WordsAny.Any() &&
                                   WordsNot.Any();
            }
        }


        public bool PartialLoaded
        {
            get { return _part; }
            set { _part = value; }
        }

        //        public List<ConfindenceHrItemObject> ChiosAll { get; set; }

        public override string ToString()
        {
            //var all = "всё {0}".FormatStr(string.Join(", ", WordsAll));
            //var any = "хотя бы {0} {1}".FormatStr(MinAny, string.Join(", ", WordsAny));
            //var not = "ни одного {0}".FormatStr(string.Join(", ", WordsNot));
            //var cat = "разделы {0}".FormatStr(string.Join(", ", Categories));
            if (IsGroup)
            {
                string s = string.Empty;
                switch (GroupOperator)
                {
                    case QueryGroupOperator.All: s = " и "; break;
                    case QueryGroupOperator.Any: s = " или "; break;
                    case QueryGroupOperator.NotAny: s = " не или "; break;
                }
                var childs = string.Join(s, Children);
                return "({0} в {1})".FormatStr(childs, SearchScope);
            }
            var sb = new StringBuilder();


            var alls = WordsAll.Union<IHrItemObject>(MeasuresAll).ToList();
            var anys = WordsAny.Union<IHrItemObject>(MeasuresAny);
            if (anys.Count() <= MinAny)
                alls.AddRange(anys); // повторы?

            if (alls.Any())
            {
                if (alls.Count() > 1)
                {
                    sb.Append("всё: ");
                }
                sb.Append(string.Join(", ", alls));
                sb.Append("/");
            }

            if (anys.Count() > MinAny)
            {
                sb.AppendFormat("хотя бы {0}: ", MinAny);

                sb.Append(string.Join(", ", anys));
                sb.Append("/");
            }

            if (WordsNot.Any())
            {
                if (WordsNot.Count() > 1)
                {
                    sb.AppendFormat("ни одного: ");
                }
                else
                    sb.AppendFormat("без ");

                sb.Append(string.Join(", ", WordsNot));
                sb.Append("/");
            }
            if (Categories.Any())
            {
                sb.AppendFormat("разделы: ");
                sb.Append(string.Join(", ", Categories));
                sb.Append("/");
            }
            return sb.ToString().Trim('/');
        }

    }


    public class OldHrSearchOptions
    {
        /// <summary>
        /// Записи со всеми словами
        /// </summary>
        public IEnumerable<Word> WordsAll { get; set; }

        /// <summary>
        /// Записи со всеми измерениями
        /// </summary>
        public IEnumerable<MeasureOp> MeasuresAll { get; set; }

        /// <summary>

        /// <summary>
        /// В области должны быть все слова из запроса.
        /// </summary>
        public bool AllWords { get; set; }

        /// <summary>
        /// Область поиска всех слов.
        /// </summary>
        public HealthRecordQueryAndScope QueryScope { get; set; }

        /// <summary>
        /// Категория. Если несколько, то любая их них.
        /// </summary>
        public IEnumerable<HrCategory> Categories { get; set; }
    }
}
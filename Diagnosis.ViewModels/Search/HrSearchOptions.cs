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

    public class HrSearchOptions
    {
        [NonSerialized]
        private bool _isRoot;
        public HrSearchOptions(bool isRoot)
        {
            Children = new ObservableCollection<HrSearchOptions>();

            WordsAll = new List<Word>();
            WordsAny = new List<Word>();
            WordsNot = new List<Word>();
            MeasuresAny = new List<MeasureOp>();
            MeasuresAll = new List<MeasureOp>();
            Categories = new List<HrCategory>();

            _isRoot = isRoot;
        }
        public HrSearchOptions()
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

        public bool All { get; set; }

        public SearchScope SearchScope { get; set; }

        public ObservableCollection<HrSearchOptions> Children { get; private set; }

        public bool IsGroup { get { return Children.Count > 0; } }

        public bool IsRoot { get { return _isRoot; } }
        public bool IsExcluding
        {
            get
            {
                return !IsGroup && WordsAll.Count() == 0 &&
                                   WordsAny.Count() == 0 &&
                                   WordsNot.Any();
            }
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
                var child = string.Join(All ? " и " : " или ", Children);
                return "({0} в {1})".FormatStr(child, SearchScope);
            }
            var sb = new StringBuilder();

            //var anystr = string.Join(", ", WordsAny);
            //var allstr = string.Join(", ", WordsAll);

            var alls = WordsAll;
            if (WordsAny.Count() <= MinAny)
                alls.AddRange(WordsAny); // повторы?

            if (alls.Count() > 0)
            {
                if (alls.Count() > 1)
                {
                    sb.Append("всё: ");
                }
                sb.Append(string.Join(", ", alls));
                sb.Append("/");
            }

            if (WordsAny.Count() > 0)
            {
                if (WordsAny.Count() > MinAny)
                {
                    sb.AppendFormat("хотя бы {0}: ", MinAny);

                    sb.Append(string.Join(", ", WordsAny));
                    sb.Append("/");
                }
            }

            if (WordsNot.Count() > 0)
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
            if (Categories.Count() > 0)
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
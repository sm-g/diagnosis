using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    [Serializable]
    public class SearchOptions : IDomainObject
    {
        [NonSerialized]
        private bool _isRoot;

        [NonSerialized]
        private bool _part;

        public SearchOptions(bool isRoot)
        {
            Children = new ObservableCollection<SearchOptions>();

            CWordsAll = new List<Confindencable<Word>>();
            CWordsAny = new List<Confindencable<Word>>();
            CWordsNot = new List<Confindencable<Word>>();
            MeasuresAny = new List<MeasureOp>();
            MeasuresAll = new List<MeasureOp>();
            Categories = new List<HrCategory>();

            _isRoot = isRoot;
        }

        public SearchOptions()
            : this(false)
        {
        }

        public int MinAny { get; set; }

        public List<Confindencable<Word>> CWordsAll { get; set; }
        public List<Confindencable<Word>> CWordsAny { get; set; }
        public List<Confindencable<Word>> CWordsNot { get; set; }
        public IEnumerable<Word> WordsAll { get { return CWordsAll.Select(x => x.HIO); } }
        public IEnumerable<Word> WordsAny { get { return CWordsAny.Select(x => x.HIO); } }
        public IEnumerable<Word> WordsNot { get { return CWordsNot.Select(x => x.HIO); } }

        public List<MeasureOp> MeasuresAll { get; set; }

        public List<MeasureOp> MeasuresAny { get; set; }

        /// <summary>
        /// Категория. Если несколько, то любая их них.
        /// </summary>
        public List<HrCategory> Categories { get; set; }

        public QueryGroupOperator GroupOperator { get; set; }

        public SearchScope SearchScope { get; set; }

        public ObservableCollection<SearchOptions> Children { get; private set; }

        public bool IsGroup { get { return Children.Count > 0; } }
        public bool WithConf { get; set; }

        /// <summary>
        /// Разрешает искать исключающий блок-корень.
        /// </summary>
        public bool IsRoot { get { return _isRoot; } }

        public bool IsExcluding
        {
            get
            {
                return !IsGroup && !CWordsAll.Any() &&
                                   !CWordsAny.Any() &&
                                   CWordsNot.Any();
            }
        }
        /// <summary>
        /// Чего-то нет на клиенте, запрос не такой, каким был сохранен
        /// </summary>
        public bool PartialLoaded
        {
            get { return _part; }
            set { _part = value; }
        }


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

            var alls = CWordsAll.Union<object>(MeasuresAll).ToList();
            var anys = CWordsAny.Union<object>(MeasuresAny);
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

            if (CWordsNot.Any())
            {
                if (CWordsNot.Count() > 1)
                {
                    sb.AppendFormat("ни одного: ");
                }
                else
                    sb.AppendFormat("без ");

                sb.Append(string.Join(", ", CWordsNot));
                sb.Append("/");
            }
            if (Categories.Any())
            {
                sb.AppendFormat("разделы: ");
                sb.Append(string.Join(", ", Categories));
                sb.Append("/");
            }
            if (WithConf)
            {
                sb.Append("с отрицанием");
            }
            return sb.ToString().Trim('/');
        }

        public override bool Equals(object obj)
        {
            var other = obj as SearchOptions;
            if (other == null) return false;

            return GroupOperator == other.GroupOperator &&
                SearchScope == other.SearchScope &&
                MinAny == other.MinAny &&
                WordsAll.ScrambledEquals(other.WordsAll) &&
                WordsAny.ScrambledEquals(other.WordsAny) &&
                WordsNot.ScrambledEquals(other.WordsNot) &&
                MeasuresAll.ScrambledEquals(other.MeasuresAll) &&
                MeasuresAny.ScrambledEquals(other.MeasuresAny) &&
                Categories.ScrambledEquals(other.Categories) &&
                Children.ScrambledEquals(other.Children);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = GroupOperator.GetHashCode();
                hash = hash * 23 + SearchScope.GetHashCode();
                hash = hash * 23 + WordsAll.Count;
                hash = hash * 23 + WordsAny.Count;
                hash = hash * 23 + WordsNot.Count;
                hash = hash * 23 + Children.Count;
                return hash;
            }
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
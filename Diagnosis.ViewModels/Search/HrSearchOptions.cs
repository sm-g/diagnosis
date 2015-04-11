using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Diagnosis.Data.Queries;

namespace Diagnosis.ViewModels.Search
{
    public class HrSearchOptions
    {
        /// <summary>
        /// Записи со всеми словами
        /// </summary>
        public IEnumerable<Word> WordsAll { get; set; }
        /// <summary>
        /// И любым словом из
        /// </summary>
        public IEnumerable<Word> WordsAny { get; set; }
        /// <summary>
        /// B ни одного слова из.
        /// </summary>
        public IEnumerable<Word> WordsNot { get; set; }
        /// <summary>
        /// Записи со всеми измерениями
        /// </summary>
        public IEnumerable<MeasureOp> MeasuresAll { get; set; }
        /// <summary>
        /// И любым из
        /// </summary>
        public IEnumerable<MeasureOp> MeasuresAny { get; set; }

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
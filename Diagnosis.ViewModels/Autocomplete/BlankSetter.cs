using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;

using System.Linq;

namespace Diagnosis.ViewModels.Autocomplete
{
    internal class BlankSetter
    {
        private Func<string, Word> FirstMatchingOrNewWord;
        private Action<Measure, Word, Action<Measure>> OpenMeasureEditor;
        private Action<IcdDisease, string, Action<IcdDisease>> OpenIcdSelector;

        public BlankSetter(Func<string, Word> FirstMatchingOrNewWord,
            Action<Measure, Word, Action<Measure>> OpenMeasureEditor,
            Action<IcdDisease, string, Action<IcdDisease>> OpenIcdSelector)
        {
            this.FirstMatchingOrNewWord = FirstMatchingOrNewWord;
            this.OpenMeasureEditor = OpenMeasureEditor;
            this.OpenIcdSelector = OpenIcdSelector;
        }
        /// <summary>
        /// 
        /// 
        /// Comment если нет hio или inverse, или hio не совпадает с запросом.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="hio"></param>
        /// <param name="exactMatchRequired"></param>
        /// <param name="inverse"></param>
        public void SetBlank(TagViewModel tag, IHrItemObject hio, bool exactMatchRequired, bool inverse)
        {
            Contract.Requires(tag != null);
            Contract.Requires<ArgumentException>(!(inverse && hio == null && tag.Query.IsNullOrEmpty())); // делаем слово по запросу

            if (hio == null ^ inverse) // direct no suggestion or inverse with suggestion
            {
                if (!tag.Query.IsNullOrEmpty())
                    tag.Blank = new Comment(tag.Query);
                else
                    tag.Blank = null; // для поиска или ентер в пустом непоследнем
            }
            else if (!inverse) // direct with hio
            {
                if (!exactMatchRequired || tag.Query.MatchesAsStrings(hio))
                    tag.Blank = hio; // main
                else
                    tag.Blank = new Comment(tag.Query); // запрос не совпал с предположением (CompleteOnLostFocus)
            }
            else // inverse, no hio
            {
                tag.Blank = FirstMatchingOrNewWord(tag.Query);
            }
        }

        /// <summary>
        /// Изменяет сущность тега с одной на другую.
        /// </summary>
        public void ConvertBlank(TagViewModel tag, BlankType toType, Action onConverted)
        {
            Contract.Requires(tag.BlankType != toType);
            Contract.Requires(toType != BlankType.None);
            Contract.Ensures(tag.BlankType == toType || tag.BlankType == Contract.OldValue(tag.BlankType));

            // if queryOrMeasureWord == null - initial or after clear query
            string queryOrMeasureWord = null;
            if (tag.BlankType == BlankType.Measure)
            {
                var w = (tag.Blank as Measure).Word;
                if (w != null)
                    queryOrMeasureWord = w.Title;
            }
            else
                queryOrMeasureWord = tag.Query;

            switch (toType)
            {
                case BlankType.Comment:
                    tag.Blank = new Comment(tag.Query);
                    onConverted();
                    break;

                case BlankType.Word: // новое или существующее
                    Contract.Assume(!queryOrMeasureWord.IsNullOrEmpty());

                    tag.Blank = FirstMatchingOrNewWord(queryOrMeasureWord);
                    onConverted();
                    break;

                case BlankType.Measure: // слово
                    Word w = null;
                    if (!queryOrMeasureWord.IsNullOrEmpty())
                        w = FirstMatchingOrNewWord(queryOrMeasureWord);
                    OpenMeasureEditor(null, w, (m) =>
                    {
                        tag.Blank = m;
                        onConverted();
                    });
                    break;

                case BlankType.Icd: // слово/коммент в поисковый запрос
                    OpenIcdSelector(null, queryOrMeasureWord, (i) =>
                    {
                        tag.Blank = i;
                        onConverted();
                    });
                    break;
            }
        }
    }
}
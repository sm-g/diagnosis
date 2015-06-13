using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Controls.Autocomplete
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
            if (inverse && hio == null && tag.Query.IsNullOrEmpty()) // делаем слово по запросу
                throw new ArgumentException();
            Contract.EndContractBlock();

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
                Contract.Assume(!tag.Query.IsNullOrEmpty());
                tag.Blank = FirstMatchingOrNewWord(tag.Query);
            }
        }

        /// <summary>
        /// Изменяет сущность тега с одной на другую.
        /// Может быть создан пустой коммент/слово, которые не должны сохраняться.
        /// </summary>
        public void ConvertBlank(TagViewModel tag, BlankType toType, Action onConverted)
        {
            if (tag.BlankType == toType)
                throw new ArgumentException();
            if (toType == BlankType.None)
                throw new ArgumentException();
            if (onConverted == null)
                throw new ArgumentException();
            Contract.Ensures(tag.BlankType == toType || tag.BlankType == Contract.OldValue(tag.BlankType)); // можно отменить to icd/measure
            Contract.EndContractBlock();

            string queryOrMeasureWordTitle;
            if (tag.BlankType == BlankType.Measure)
            {
                var w = (tag.Blank as Measure).Word;
                Contract.Assume(w != null); // измерение без слова в теге не бывает
                queryOrMeasureWordTitle = w.Title;
            }
            else
                queryOrMeasureWordTitle = tag.Query; // == "" if initial or after clear query

            Contract.Assume(queryOrMeasureWordTitle != null);

            switch (toType)
            {
                case BlankType.Comment:
                    Contract.Assume(tag.Query != null);
                    tag.Blank = new Comment(tag.Query);
                    onConverted();
                    break;

                case BlankType.Word:
                    tag.Blank = FirstMatchingOrNewWord(queryOrMeasureWordTitle);
                    onConverted();
                    break;

                case BlankType.Measure: // слово
                    Word w = null;
                    if (!queryOrMeasureWordTitle.IsNullOrEmpty())
                        w = FirstMatchingOrNewWord(queryOrMeasureWordTitle);
                    OpenMeasureEditor(null, w, (m) =>
                    {
                        tag.Blank = m;
                        onConverted();
                    });
                    break;

                case BlankType.Icd: // слово/коммент в поисковый запрос
                    OpenIcdSelector(null, queryOrMeasureWordTitle, (i) =>
                    {
                        tag.Blank = i;
                        onConverted();
                    });
                    break;
            }
        }
    }
}
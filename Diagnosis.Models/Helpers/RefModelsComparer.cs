using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Models
{
    /// <summary>
    /// Parents "greater" than children, so after OrderBy children proceeded first
    /// </summary>
    public class RefModelsComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            // UomType > Uom
            if (x == typeof(UomType) && y == typeof(Uom))
                return 1;
            if (y == typeof(UomType) && x == typeof(Uom))
                return -1;

            // Uom > UomFormat
            if (x == typeof(Uom) && y == typeof(UomFormat))
                return 1;
            if (y == typeof(Uom) && x == typeof(UomFormat))
                return -1;

            // Speciality > SpecialityIcdBlocks
            if (x == typeof(Speciality) && y == typeof(SpecialityIcdBlocks))
                return 1;
            if (y == typeof(Speciality) && x == typeof(SpecialityIcdBlocks))
                return -1;

            // IcdBlock > SpecialityIcdBlocks
            if (x == typeof(IcdBlock) && y == typeof(SpecialityIcdBlocks))
                return 1;
            if (y == typeof(IcdBlock) && x == typeof(SpecialityIcdBlocks))
                return -1;

            // Speciality > SpecialityVocabularies
            if (x == typeof(Speciality) && y == typeof(SpecialityVocabularies))
                return 1;
            if (y == typeof(Speciality) && x == typeof(SpecialityVocabularies))
                return -1;

            // Vocabulary > VocabularyWords
            if (x == typeof(Vocabulary) && y == typeof(VocabularyWords))
                return 1;
            if (y == typeof(Vocabulary) && x == typeof(VocabularyWords))
                return -1;

            // IcdBlock > IcdDisease
            if (x == typeof(IcdBlock) && y == typeof(IcdDisease))
                return 1;
            if (y == typeof(IcdBlock) && x == typeof(IcdDisease))
                return -1;

            // IcdChapter > IcdBlock
            if (x == typeof(IcdChapter) && y == typeof(IcdBlock))
                return 1;
            if (y == typeof(IcdChapter) && x == typeof(IcdBlock))
                return -1;

            // IcdChapter > IcdDisease
            if (x == typeof(IcdChapter) && y == typeof(IcdDisease))
                return 1;
            if (y == typeof(IcdChapter) && x == typeof(IcdDisease))
                return -1;

            return 0;
        }
    }
}
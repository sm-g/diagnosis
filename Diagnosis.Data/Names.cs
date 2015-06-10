using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data
{
    public static class Names
    {
        public readonly static string Doctor = "Doctor";
        public readonly static string Patient = "Patient";
        public readonly static string Course = "Course";
        public readonly static string Appointment = "Appointment";
        public readonly static string HealthRecord = "HealthRecord";
        public readonly static string HrCategory = "HrCategory";
        public readonly static string HrItem = "HrItem";
        public readonly static string Word = "Word";
        public readonly static string WordTemplate = "WordTemplate";
        public readonly static string Vocabulary = "Vocabulary";
        public readonly static string VocabularyWords = "VocabularyWords";
        public readonly static string Speciality = "Speciality";
        public readonly static string Uom = "Uom";
        public readonly static string UomType = "UomType";
        public readonly static string UomFormat = "UomFormat";
        public readonly static string IcdChapter = "IcdChapter";
        public readonly static string IcdBlock = "IcdBlock";
        public readonly static string IcdDisease = "IcdDisease";
        public readonly static string SpecialityIcdBlocks = "SpecialityIcdBlocks";
        public readonly static string SpecialityVocabularies = "SpecialityVocabularies";
        public readonly static string Passport = "Passport";
        public readonly static string Setting = "Setting";
        public readonly static string Crit = "Crit";
        public readonly static string Criterion = "Criterion";
        public readonly static string CriteriaGroup = "CriteriaGroup";
        public readonly static string Estimator = "Estimator";
        public readonly static string CritWords = "CritWords";

        public static Dictionary<string, Type> tblToTypeMap = new Dictionary<string, Type> {
            { Doctor,            typeof(Doctor) },
            { Patient,           typeof(Patient) },
            { Course,            typeof(Course) },
            { Appointment,       typeof(Appointment) },
            { HealthRecord,      typeof(HealthRecord) },
            { HrCategory,        typeof(HrCategory) },

            { HrItem,            typeof(HrItem) },
            { Word,              typeof(Word) },
            { WordTemplate,      typeof(WordTemplate) },
            { Vocabulary,        typeof(Vocabulary) },
            { VocabularyWords,   typeof(VocabularyWords) },
            { Speciality,        typeof(Speciality) },
            { SpecialityVocabularies,        typeof(SpecialityVocabularies) },
            { Uom,               typeof(Uom) },
            { UomType,           typeof(UomType) },
            { UomFormat,         typeof(UomFormat) },

            { IcdChapter,        typeof(IcdChapter) },
            { IcdBlock,          typeof(IcdBlock) },
            { IcdDisease,        typeof(IcdDisease) },
            { SpecialityIcdBlocks,typeof(SpecialityIcdBlocks) },

            { Passport,          typeof(Passport) },
            { Setting,           typeof(Setting) },

            { Crit,         typeof(Crit) },
            { Criterion,         typeof(Criterion) },
            { CriteriaGroup,     typeof(CriteriaGroup) },
            { Estimator,         typeof(Estimator) },
        };

        public static string GetTblByType(Type type)
        {
            var tbl = tblToTypeMap.FirstOrDefault(x => x.Value == type).Key;
            if (tbl == default(string))
                throw new NotSupportedException("No table for type");
            return tbl;
        }

        public static class Id
        {
            public readonly static string Doctor = Names.Doctor + "ID";
            public readonly static string Patient = Names.Patient + "ID";
            public readonly static string Course = Names.Course + "ID";
            public readonly static string Appointment = Names.Appointment + "ID";
            public readonly static string HealthRecord = Names.HealthRecord + "ID";
            public readonly static string HrCategory = Names.HrCategory + "ID";

            public readonly static string HrItem = Names.HrItem + "ID";
            public readonly static string Word = Names.Word + "ID";
            public readonly static string WordTemplate = Names.WordTemplate + "ID";
            public readonly static string Vocabulary = Names.Vocabulary + "ID";
            public readonly static string VocabularyWords = Names.VocabularyWords + "ID";
            public readonly static string Speciality = Names.Speciality + "ID";
            public readonly static string Uom = Names.Uom + "ID";
            public readonly static string UomType = Names.UomType + "ID";
            public readonly static string UomFormat = Names.UomFormat + "ID";

            public readonly static string IcdChapter = "ChapterID"; //
            public readonly static string IcdBlock = Names.IcdBlock + "ID";
            public readonly static string IcdDisease = Names.IcdDisease + "ID";
            public readonly static string SpecialityIcdBlocks = Names.SpecialityIcdBlocks + "ID";
            public readonly static string SpecialityVocabularies = Names.SpecialityVocabularies + "ID";

            public readonly static string Passport = Names.Passport + "ID";
            public readonly static string Setting = Names.Setting + "ID";

            public readonly static string Criterion = Names.Criterion + "ID";
            public readonly static string CriteriaGroup = Names.CriteriaGroup + "ID";
            public readonly static string Estimator = Names.Estimator + "ID";
            public readonly static string Crit = Names.Crit + "ID";
            public readonly static string CritParent = "ParentID";
        }

        public static class FK
        {
            public readonly static string Doctor_Passport = string.Format("FK_{0}_{1}", Doctor, Passport);
            public readonly static string Doc_Voc = string.Format("FK_{0}_{1}", Doctor, Vocabulary);
            public readonly static string WordTemplate_Voc = string.Format("FK_{0}_{1}", WordTemplate, Vocabulary);
            public readonly static string SpecVoc_Spec = string.Format("FK_{0}_{1}", SpecialityVocabularies, Speciality);
            public readonly static string SpecVoc_Voc = string.Format("FK_{0}_{1}", SpecialityVocabularies, Vocabulary);
            public readonly static string VocWord_Word = string.Format("FK_{0}_{1}", VocabularyWords, Word);
            public readonly static string VocWord_Voc = string.Format("FK_{0}_{1}", VocabularyWords, Vocabulary);
            public readonly static string Uom_UomType = string.Format("FK_{0}_{1}", Uom, UomType);
            public readonly static string UomFormat_Uom = string.Format("FK_{0}_{1}", UomFormat, Uom);
            public readonly static string HrItem_Uom = string.Format("FK_{0}_{1}", HrItem, Uom);
            public readonly static string Hr_HrCategory = string.Format("FK_Hr_HrCategory"); //
            public readonly static string Doctor_Speciality = string.Format("FK_{0}_{1}", Doctor, Speciality);
            public readonly static string SpecialityIcdBlocks_Specia = string.Format("FK_{0}_Specia", SpecialityIcdBlocks); //
            public readonly static string Word_HrCategory = string.Format("FK_{0}_{1}", Word, HrCategory);
            public readonly static string Word_Word = string.Format("FK_{0}_{1}", Word, Word);
            public readonly static string Word_Uom = string.Format("FK_{0}_{1}", Word, Uom);
            public readonly static string Setting_Doctor = string.Format("FK_{0}_{1}", Setting, Doctor);
            public readonly static string Criterion_CritGr = string.Format("FK_{0}_{1}", Criterion, CriteriaGroup);
            public readonly static string CrGr_Est = string.Format("FK_{0}_{1}", CriteriaGroup, Estimator);
            public readonly static string CritWord_Word = string.Format("FK_{0}_{1}", CritWords, Word);
            public readonly static string CritWord_Crit = string.Format("FK_{0}_{1}", CritWords, Crit);
            public readonly static string Crit_Crit = string.Format("FK_{0}_{1}", Crit, Crit);
        }

        public static class Col
        {
            public readonly static string CourseStart = "StartDate";
            public readonly static string CourseEnd = "EndDate";
            public readonly static string HrItemMeasure = "MeasureValue";
            public readonly static string DoctorCustomVocabulary = "CustomVocabularyID";
            public readonly static string WordParent = "ParentID";
            public readonly static string HrFromDay = "FromDay";
            public readonly static string HrFromMonth = "FromMonth";
            public readonly static string HrFromYear = "FromYear";
            public readonly static string HrToDay = "ToDay";
            public readonly static string HrToMonth = "ToMonth";
            public readonly static string HrToYear = "ToYear";
            public readonly static string HrDescribedAt = "DescribedAt";
            public readonly static string CreatedAt = "CreatedAt";
            public readonly static string UomFValue = "MeasureValue";
            public readonly static string UomFStr = "String";
            public readonly static string HrItemTextRepr = "TextRepr";
            public readonly static string CritType = "CritType";
            public readonly static string EstDiscriminator = "Estimator";
            public readonly static string CrGrDiscriminator = "CriteriaGroup";
            public readonly static string CritDiscriminator = "Criterion";
        }

        public static class Unique
        {
            public readonly static string ChpaterCode = "ChpaterCode";
            public readonly static string BlockCode = "BlockCode";
            public readonly static string DiseaseCode = "DiseaseCode";
            public readonly static string WordTitle = "WordTitle";
        }
    }

    internal class Types
    {
        public class Numeric
        {
            public const short Scale = 6;
            public const short ShortScale = 3;
            public const short Precision = 18;

        }
    }


}
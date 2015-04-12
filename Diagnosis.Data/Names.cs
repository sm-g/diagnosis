using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data
{
    public static class Names
    {
        public static string Doctor = "Doctor";
        public static string Patient = "Patient";
        public static string Course = "Course";
        public static string Appointment = "Appointment";
        public static string HealthRecord = "HealthRecord";
        public static string HrCategory = "HrCategory";
        public static string HrItem = "HrItem";
        public static string Word = "Word";
        public static string WordTemplate = "WordTemplate";
        public static string Vocabulary = "Vocabulary";
        public static string VocabularyWords = "VocabularyWords";
        public static string Speciality = "Speciality";
        public static string Uom = "Uom";
        public static string UomType = "UomType";
        public static string IcdChapter = "IcdChapter";
        public static string IcdBlock = "IcdBlock";
        public static string IcdDisease = "IcdDisease";
        public static string SpecialityIcdBlocks = "SpecialityIcdBlocks";
        public static string SpecialityVocabularies = "SpecialityVocabularies";
        public static string Passport = "Passport";
        public static string Setting = "Setting";

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
         // { VocabularyWordTbl,    typeof() },
            { Speciality,        typeof(Speciality) },
            { SpecialityVocabularies,        typeof(SpecialityVocabularies) },
            { Uom,               typeof(Uom) },
            { UomType,           typeof(UomType) },

            { IcdChapter,        typeof(IcdChapter) },
            { IcdBlock,          typeof(IcdBlock) },
            { IcdDisease,        typeof(IcdDisease) },
            { SpecialityIcdBlocks,typeof(SpecialityIcdBlocks) },

            { Passport,          typeof(Passport) },
            { Setting,           typeof(Setting) },
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
            public static string Doctor = Names.Doctor + "ID";
            public static string Patient = Names.Patient + "ID";
            public static string Course = Names.Course + "ID";
            public static string Appointment = Names.Appointment + "ID";
            public static string HealthRecord = Names.HealthRecord + "ID";
            public static string HrCategory = Names.HrCategory + "ID";

            public static string HrItem = Names.HrItem + "ID";
            public static string Word = Names.Word + "ID";
            public static string WordTemplate = Names.WordTemplate + "ID";
            public static string Vocabulary = Names.Vocabulary + "ID";
            public static string VocabularyWords = Names.VocabularyWords + "ID";
            public static string Speciality = Names.Speciality + "ID";
            public static string Uom = Names.Uom + "ID";
            public static string UomType = Names.UomType + "ID";

            public static string IcdChapter = "ChapterID"; //
            public static string IcdBlock = Names.IcdBlock + "ID";
            public static string IcdDisease = Names.IcdDisease + "ID";
            public static string SpecialityIcdBlocks = Names.SpecialityIcdBlocks + "ID";
            public static string SpecialityVocabularies = Names.SpecialityVocabularies + "ID";

            public static string Passport = Names.Passport + "ID";
            public static string Setting = Names.Setting + "ID";
        }

        public static class FK
        {
            public static string Doctor_Passport = string.Format("FK_{0}_{1}", Doctor, Passport);
            public static string Doc_Voc = string.Format("FK_{0}_{1}", Doctor, Vocabulary);
            public static string WordTemplate_Voc = string.Format("FK_{0}_{1}", WordTemplate, Vocabulary);
            public static string SpecVoc_Spec = string.Format("FK_{0}_{1}", SpecialityVocabularies, Speciality);
            public static string SpecVoc_Voc = string.Format("FK_{0}_{1}", SpecialityVocabularies, Vocabulary);
            public static string VocWord_Word = string.Format("FK_{0}_{1}", VocabularyWords, Word);
            public static string VocWord_Voc = string.Format("FK_{0}_{1}", VocabularyWords, Vocabulary);
            public static string Uom_UomType = string.Format("FK_{0}_{1}", Uom, UomType);
            public static string HrItem_Uom = string.Format("FK_{0}_{1}", HrItem, Uom);
            public static string Hr_HrCategory = string.Format("FK_Hr_HrCategory"); //
            public static string Doctor_Speciality = string.Format("FK_{0}_{1}", Doctor, Speciality);
            public static string SpecialityIcdBlocks_Specia = string.Format("FK_{0}_Specia", SpecialityIcdBlocks); //
            public static string Word_HrCategory = string.Format("FK_{0}_{1}", Word, HrCategory);
            public static string Word_Word = string.Format("FK_{0}_{1}", Word, Word);
            public static string Setting_Doctor = string.Format("FK_{0}_{1}", Setting, Doctor);
        }
        public static class Col
        {
            public static string CourseStart = "StartDate";
            public static string CourseEnd = "EndDate";
            public static string HrItemMeasure = "MeasureValue";
            public static string DoctorCustomVocabulary = "CustomVocabularyID";
            public static string WordParent = "ParentID";
            public static string HrFromDay = "FromDay";
            public static string HrFromMonth = "FromMonth";
            public static string HrFromYear = "FromYear";
            public static string HrToDay = "ToDay";
            public static string HrToMonth = "ToMonth";
            public static string HrToYear = "ToYear";
            public static string HrDescribedAt = "DescribedAt";
            public static string CreatedAt = "CreatedAt";
        }

        public static class Unique
        {
            public static string ChpaterCode = "ChpaterCode";
            public static string BlockCode = "BlockCode";
            public static string DiseaseCode = "DiseaseCode";
            public static string WordTitle = "WordTitle";
        }
    }
}
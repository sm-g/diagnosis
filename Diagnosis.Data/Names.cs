using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data
{
    internal static class Names
    {
        public class Id
        {
            public static string Doctor = Doctor + "ID";
            public static string Patient = Patient + "ID";
            public static string Course = Course + "ID";
            public static string Appointment = Appointment + "ID";
            public static string HealthRecord = HealthRecord + "ID";
            public static string HrCategory = HrCategory + "ID";

            public static string HrItem = HrItem + "ID";
            public static string Word = Word + "ID";
            public static string WordTemplate = WordTemplate + "ID";
            public static string Vocabulary = Vocabulary + "ID";
            public static string VocabularyWord = VocabularyWord + "ID";
            public static string Speciality = Speciality + "ID";
            public static string Uom = Uom + "ID";
            public static string UomType = UomType + "ID";

            public static string IcdChapter = "Chapter" + "ID";
            public static string IcdBlock = IcdBlock + "ID";
            public static string IcdDisease = IcdDisease + "ID";
            public static string SpecialityIcdBlocks = SpecialityIcdBlocks + "ID";
            public static string SpecialityVocabularies = SpecialityVocabularies + "ID";

            public static string Passport = Passport + "ID";
            public static string Setting = Setting + "ID";
        }

        public class Col
        {
            public static string CourseStart = "StartDate";
            public static string CourseEnd = "EndDate";
            public static string HrItemMeasure = "MeasureValue";
            public static string DoctorCustomVocabulary = "CustomVocabularyID";
            public static string WordParent = "ParentID";
        }

        public class Unique
        {
            public static string ChpaterCode = "ChpaterCode";
            public static string BlockCode = "BlockCode";
            public static string DiseaseCode = "DiseaseCode";
            public static string WordTitle = "WordTitle";
        }

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
         // { SpecialityVocabulariesTbl,        typeof() },
            { Uom,               typeof(Uom) },
            { UomType,           typeof(UomType) },

            { IcdChapter,        typeof(IcdChapter) },
            { IcdBlock,          typeof(IcdBlock) },
            { IcdDisease,        typeof(IcdDisease) },
            { SpecialityIcdBlocks,typeof(SpecialityIcdBlocks) },

            { Passport,          typeof(Passport) },
            { Setting,           typeof(Setting) },
        };
    }
}
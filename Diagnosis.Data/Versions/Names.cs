using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    internal static class Names
    {
        public static string DoctorTbl = "Doctor";
        public static string PatientTbl = "Patient";
        public static string CourseTbl = "Course";
        public static string AppointmentTbl = "Appointment";
        public static string HealthRecordTbl = "HealthRecord";
        public static string HrCategoryTbl = "HrCategory";

        public static string HrItemTbl = "HrItem";
        public static string WordTbl = "Word";
        public static string WordTemplateTbl = "WordTemplate";
        public static string VocabularyTbl = "Vocabulary";
        public static string VocabularyWordTbl = "VocabularyWords";
        public static string SpecialityTbl = "Speciality";
        public static string UomTbl = "Uom";
        public static string UomTypeTbl = "UomType";

        public static string IcdChapterTbl = "IcdChapter";
        public static string IcdBlockTbl = "IcdBlock";
        public static string IcdDiseaseTbl = "IcdDisease";
        public static string SpecialityIcdBlocksTbl = "SpecialityIcdBlocks";
        public static string SpecialityVocabulariesTbl = "SpecialityVocabularies";

        public static string PassportTbl = "Passport";
        public static string SettingTbl = "Setting";

        public static Dictionary<string, Type> tblToTypeMap = new Dictionary<string, Type> {
            { DoctorTbl,            typeof(Doctor) },
            { PatientTbl,           typeof(Patient) },
            { CourseTbl,            typeof(Course) },
            { AppointmentTbl,       typeof(Appointment) },
            { HealthRecordTbl,      typeof(HealthRecord) },
            { HrCategoryTbl,        typeof(HrCategory) },

            { HrItemTbl,            typeof(HrItem) },
            { WordTbl,              typeof(Word) },
            { WordTemplateTbl,      typeof(WordTemplate) },
            { VocabularyTbl,        typeof(Vocabulary) },
         // { VocabularyWordTbl,    typeof() },
            { SpecialityTbl,        typeof(Speciality) },
         // { SpecialityVocabulariesTbl,        typeof() },
            { UomTbl,               typeof(Uom) },
            { UomTypeTbl,           typeof(UomType) },

            { IcdChapterTbl,        typeof(IcdChapter) },
            { IcdBlockTbl,          typeof(IcdBlock) },
            { IcdDiseaseTbl,        typeof(IcdDisease) },
            { SpecialityIcdBlocksTbl,typeof(SpecialityIcdBlocks) },

            { PassportTbl,          typeof(Passport) },
            { SettingTbl,           typeof(Setting) },
        };
    }
}
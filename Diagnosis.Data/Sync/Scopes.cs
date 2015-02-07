using Diagnosis.Data.Versions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    internal enum Scope
    {
        holder,
        hr,
        user,
        reference
    }

    internal static class Scopes
    {
        public static string syncPrefix = "sync";

        private static string holderScope = "holderScope";
        private static string hrScope = "hrScope";
        private static string userScope = "userScope";
        private static string referenceScope = "referenceScope";
        private static string stagingSchema = "staging";
        private static string referenceSchema = "dbo";

        // parent before child
        private static string[] holderTableNames = new[] {
                Names.PatientTbl,
                Names.CourseTbl,
                Names.AppointmentTbl,
            };

        private static string[] hrTableNames = new[] {
                Names.HealthRecordTbl,
                Names.HrItemTbl,
                Names.WordTbl,
            };

        private static string[] userTableNames = new[] {
                //Names.PassportTbl,
                Names.DoctorTbl,
               // Names.SettingTbl,
            };

        private static string[] referenceTableNames = new[] {
                //Names.HrCategoryTbl, ?

                Names.IcdChapterTbl,
                Names.IcdBlockTbl,
                Names.IcdDiseaseTbl,
                Names.UomTypeTbl,
                Names.UomTbl,
                Names.SpecialityTbl,
                Names.SpecialityIcdBlockTbl,
            };

        private static Dictionary<Scope, string> scopeNames = new Dictionary<Scope, string>
        {
            {Scope.holder,      holderScope},
            {Scope.hr,          hrScope},
            {Scope.user,        userScope},
            {Scope.reference,   referenceScope},
        };

        private static Dictionary<Scope, string[]> scopeToTables = new Dictionary<Scope, string[]>
        {
            {Scope.holder,      holderTableNames},
            {Scope.hr,          hrTableNames},
            {Scope.user,        userTableNames},
            {Scope.reference,   referenceTableNames},
        };

        public static string ToScopeString(this Scope scope)
        {
            string result = "";
            if (scopeNames.TryGetValue(scope, out result))
            {
                return result;
            }

            throw new NotImplementedException();
        }

        public static string[] ToTableNames(this Scope scope)
        {
            string[] result;
            if (scopeToTables.TryGetValue(scope, out result))
            {
                return result;
            }

            throw new NotImplementedException();
        }

        public static string ToSchema(this string table)
        {
            return referenceTableNames.Contains(table) ? referenceSchema : stagingSchema;
        }
    }
}
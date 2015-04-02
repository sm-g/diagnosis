﻿using Diagnosis.Data.Versions;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    /// <summary>
    /// In order
    /// </summary>
    public enum Scope
    {
        Icd,
        Voc,
        Reference,
        User,
        Holder,
        Hr,
    }

    public static class Scopes
    {
        public static string syncPrefix = "sync";

        private static string holderScope = "holderScope";
        private static string hrScope = "hrScope";
        private static string userScope = "userScope";
        private static string referenceScope = "referenceScope";
        private static string icdScope = "icdScope";
        private static string vocScope = "vocScope";

        private static string stagingSchema = "staging";
        private static string referenceSchema = "dbo";

        // parent before child (for SetCreateTableDefault)
        private static string[] icdTableNames = new[] {
                Names.IcdChapter,
                Names.IcdBlock,
                Names.IcdDisease,
            };

        private static string[] vocTableNames = new[] {
                Names.Vocabulary,
                Names.WordTemplate,
                Names.Speciality,
                Names.SpecialityVocabularies,
                // do not sync VocabularyWords - its client-only
            };

        private static string[] referenceTableNames = new[] {
                Names.HrCategory,
                Names.UomType,
                Names.Uom,
                Names.Speciality,
                Names.SpecialityIcdBlocks,
            };

        private static string[] userTableNames = new[] {
                //Names.PassportTbl,
                Names.Doctor,
               // Names.SettingTbl,
            };

        private static string[] holderTableNames = new[] {
                Names.Patient,
                Names.Course,
                Names.Appointment,
            };

        private static string[] hrTableNames = new[] {
                Names.Word,
                Names.HealthRecord,
                Names.HrItem,
            };


        private static Dictionary<Scope, string> scopeNames = new Dictionary<Scope, string>
        {
            {Scope.Holder,      holderScope},
            {Scope.Hr,          hrScope},
            {Scope.User,        userScope},
            {Scope.Reference,   referenceScope},
            {Scope.Icd,         icdScope},
            {Scope.Voc,         vocScope},
        };

        private static Dictionary<Scope, string[]> scopeToTables = new Dictionary<Scope, string[]>
        {
            {Scope.Holder,      holderTableNames},
            {Scope.Hr,          hrTableNames},
            {Scope.User,        userTableNames},
            {Scope.Reference,   referenceTableNames},
            {Scope.Icd,         icdTableNames},
            {Scope.Voc,         vocTableNames},
        };

        public static IList<Scope> GetOrderedUploadScopes()
        {
            return new List<Scope>()
            {
                Scope.User,
                Scope.Holder,
                Scope.Hr,
            };
        }
        public static IList<Scope> GetOrderedDownloadScopes()
        {
            return new List<Scope>()
            {
#if !DEBUG		  
                Scope.Icd,
    #endif
                Scope.Voc,
                Scope.Reference,
            };
        }
        public static IList<Scope> GetOrderedScopes(this Side from)
        {
            switch (from)
            {
                case Side.Client:
                    return GetOrderedUploadScopes();
                case Side.Server:
                    return GetOrderedDownloadScopes();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Scope GetScope(this Type type)
        {
            var tbl = Names.tblToTypeMap.FirstOrDefault(x => x.Value == type).Key;
            if (tbl == default(String))
                throw new ArgumentOutOfRangeException("Type is not syncronized");

            return scopeToTables.FirstOrDefault(x => x.Value.Contains(tbl)).Key;
        }

        public static IList<Scope> GetOrderedScopes()
        {
            return new List<Scope>(Enum.GetValues(typeof(Scope)).Cast<Scope>().OrderScopes());
        }

        public static IEnumerable<string> GetVocOnlyTables()
        {
            return new[] { Names.SpecialityVocabularies, Names.Vocabulary, Names.WordTemplate };
        }

        public static IEnumerable<Scope> OrderScopes(this IEnumerable<Scope> scopes)
        {
            return scopes.OrderBy(x => x);
        }

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

        public static string GetSchemaForTable(this string table)
        {
            return referenceTableNames.Contains(table) ? referenceSchema : stagingSchema;
        }

    }
}
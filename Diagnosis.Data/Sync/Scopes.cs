﻿using Diagnosis.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public readonly static string syncPrefix = "sync";

        private readonly static string holderScope = "holderScope";
        private readonly static string hrScope = "hrScope";
        private readonly static string userScope = "userScope";
        private readonly static string referenceScope = "referenceScope";
        private readonly static string icdScope = "icdScope";
        private readonly static string vocScope = "vocScope";

        private readonly static string stagingSchema = "staging";
        private readonly static string referenceSchema = "dbo";

        // parent before child (for SetCreateTableDefault)
        private static string[] icdTableNames = new[] {
                Names.IcdChapter,
                Names.IcdBlock,
                Names.IcdDisease,
            };

        private static string[] vocTableNames = new[] {
                // for just create Doctor table in exchange db, must exclude data from sync
                Names.Doctor,
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
                Names.UomFormat,
                Names.Speciality,
                Names.SpecialityIcdBlocks,
            };

        private static string[] userTableNames = new[] {
                //Names.Passport,
                Names.Doctor,
                //Names.Setting,
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

        private static Dictionary<Scope, IEnumerable<string>> scopeToTables = new Dictionary<Scope, IEnumerable<string>>
        {
            {Scope.Holder,      holderTableNames},
            {Scope.Hr,          hrTableNames},
            {Scope.User,        userTableNames},
            {Scope.Reference,   referenceTableNames},
            {Scope.Icd,         icdTableNames},
            {Scope.Voc,         vocTableNames},
        };

        private static Dictionary<string, IEnumerable<Scope>> tableToScopes = scopeToTables.ReverseManyToMany();

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

        public static IEnumerable<Scope> GetScopes(this Type type)
        {
            var tbl = Names.tblToTypeMap.FirstOrDefault(x => x.Value == type).Key;
            if (tbl == default(String))
                throw new ArgumentOutOfRangeException("Type is not syncronized");

            return tableToScopes[tbl];
        }

        public static IEnumerable<Scope> GetScopes(this string table)
        {
            return tableToScopes[table];
        }

        /// <summary>
        /// Все области, в которых есть хотя бы одна таблица из области scope.
        /// </summary>
        public static IEnumerable<Scope> GetRelatedScopes(this Scope scope)
        {
            switch (scope)
            {
                case Scope.Voc:
                case Scope.Reference:
                case Scope.User:
                    return new[] { Scope.Voc, Scope.Reference, Scope.User };
                case Scope.Holder:
                    return new[] { Scope.Holder };
                case Scope.Hr:
                    return new[] { Scope.Hr };
                case Scope.Icd:
                    return new[] { Scope.Icd };
                default:
                    throw new NotImplementedException();
            }
            //return ToTableNames(scope)
            //    .SelectMany(t => GetScopes(t))
            //    .Distinct();
        }

        public static IEnumerable<Type> GetTypes(this Scope scope)
        {
            return scopeToTables[scope].Select(x => Names.tblToTypeMap[x]);
        }

        public static IList<Scope> GetOrderedScopes()
        {
            return new List<Scope>(Enum.GetValues(typeof(Scope)).Cast<Scope>().OrderScopes());
        }

        public static IEnumerable<string> GetVocOnlyTablesToDownload()
        {
            return new[] { Names.Vocabulary, Names.WordTemplate, Names.SpecialityVocabularies };
        }

        public static IEnumerable<Scope> OrderScopes(this IEnumerable<Scope> scopes)
        {
            Contract.Requires(scopes != null);
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

        public static IEnumerable<string> ToTableNames(this Scope scope)
        {
            IEnumerable<string> result;
            if (scopeToTables.TryGetValue(scope, out result))
                return result;

            throw new NotImplementedException();
        }

        public static string GetSchemaForTable(this string table)
        {
            return referenceTableNames.Contains(table) ? referenceSchema : stagingSchema;
        }
    }
}
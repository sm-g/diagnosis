using Diagnosis.Common;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;

namespace Diagnosis.Data.Sync
{
    internal class SyncUtil
    {
        private readonly static string prefix = Scopes.syncPrefix;
        private static Dictionary<string, DbSyncTableDescription> map = new Dictionary<string, DbSyncTableDescription>();

        public static RelationalSyncProvider CreateProvider(DbConnection conn, string scopeName)
        {
            RelationalSyncProvider serverProvider;
            if (conn is SqlCeConnection)
                serverProvider = new SqlCeSyncProvider(scopeName, conn as SqlCeConnection, prefix);
            else if (conn is SqlConnection)
                serverProvider = new SqlSyncProvider(scopeName, conn as SqlConnection, prefix);
            else throw new ArgumentOutOfRangeException();

            return serverProvider;
        }

        public static void Provision(DbConnection con, Scope scope, DbConnection conToGetScopeDescr)
        {
            try
            {
                var applied = false;
                if (con is SqlCeConnection)
                {
                    applied = ProvisionSqlCe(con as SqlCeConnection, scope, conToGetScopeDescr);
                }
                else if (con is SqlConnection)
                {
                    applied = ProvisionSqlServer(con as SqlConnection, scope);
                }

                if (applied)
                    Poster.PostMessage("Provisioned scope '{0}' in '{1}'\n", scope.ToScopeString(), con.ConnectionString);
                else
                    Poster.PostMessage("Scope '{0}' exists in '{1}'\n", scope.ToScopeString(), con.ConnectionString);
            }
            catch (Exception ex)
            {
                Poster.PostMessage(ex);
            }
        }

        public static void Deprovision(DbConnection con, Scope scope)
        {
            try
            {
                if (con is SqlCeConnection)
                    DeprovisionSqlCe(con as SqlCeConnection, scope.ToScopeString());
                else if (con is SqlConnection)
                    DeprovisionSqlServer(con as SqlConnection, scope.ToScopeString());

                Poster.PostMessage("Deprovisioned '{0}'\n", scope.ToScopeString());
            }
            catch (Exception ex)
            {
                Poster.PostMessage(ex);
            }
        }

        internal static void Clear()
        {
            map.Clear();
        }
        private static bool ProvisionSqlCe(SqlCeConnection con, Scope scope, DbConnection conToGetScopeDescr)
        {
            var sqlceProv = new SqlCeSyncScopeProvisioning(con);
            sqlceProv.ObjectPrefix = prefix;

            if (!sqlceProv.ScopeExists(scope.ToScopeString()))
            {
                var scopeDescr = new DbSyncScopeDescription(scope.ToScopeString());
                var failedTables = AddTablesToScopeDescr(scope.ToTableNames(), scopeDescr, con);

                if (failedTables.Count > 0 && conToGetScopeDescr != null)
                {
                    Poster.PostMessage("GetScopeDescription for scope '{0}' from '{1}'", scope.ToScopeString(), conToGetScopeDescr.ConnectionString);
                    //use scope description from server to intitialize the client
                    scopeDescr = GetScopeDescription(scope, conToGetScopeDescr);
                }

                sqlceProv.PopulateFromScopeDescription(scopeDescr);

                sqlceProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlceProv.Apply();
                return true;
            }
            return false;
        }

        private static bool ProvisionSqlServer(SqlConnection con, Scope scope)
        {
            var sqlProv = new SqlSyncScopeProvisioning(con);
            sqlProv.ObjectSchema = prefix;

            if (!sqlProv.ScopeExists(scope.ToScopeString()))
            {
                var scopeDescr = new DbSyncScopeDescription(scope.ToScopeString());
                AddTablesToScopeDescr(scope.ToTableNames(), scopeDescr, con);

                sqlProv.PopulateFromScopeDescription(scopeDescr);

                sqlProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlProv.Apply();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Добавляет таблицы в область синхронизации.
        /// </summary>
        /// <param name="tableNames"></param>
        /// <param name="scope"></param>
        /// <param name="connection"></param>
        /// <returns>Таблицы, которые не были добавлены.</returns>
        private static IList<string> AddTablesToScopeDescr(IEnumerable<string> tableNames, DbSyncScopeDescription scope, DbConnection connection)
        {
            var failed = new List<string>();
            Poster.PostMessage("Adding tables to scope '{0}' in '{1}'...", scope.ScopeName, connection.ConnectionString);

            foreach (var name in tableNames)
            {
                try
                {
                    DbSyncTableDescription desc;

                    // не создаем описание для таблицы повторно, после провизионирования другой области с этой таблицей
                    var nameWithSchema = connection is SqlCeConnection ? name : name.GetSchemaForTable() + '.' + name;
                    var nameWithConn = connection.ConnectionString + nameWithSchema;

                    if (!map.TryGetValue(nameWithConn, out desc))
                    {
                        if (connection is SqlCeConnection)
                            desc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable(nameWithSchema, connection as SqlCeConnection);
                        else if (connection is SqlConnection)
                            desc = SqlSyncDescriptionBuilder.GetDescriptionForTable(nameWithSchema, connection as SqlConnection);

                        map[nameWithConn] = desc;
                    }
                    else
                    {
                        Poster.PostMessage("Reuse created Description For Table '{0}'", name);
                    }

                    desc.GlobalName = name;
                    scope.Tables.Add(desc);
                    Poster.PostMessage("Table '{0}' added, columns: {1}", name, string.Join(", ", desc.Columns.Select(x => x.UnquotedName)));
                }
                catch (Exception ex)
                {
                    Poster.PostMessage(ex);
                    failed.Add(name);
                }
            }
            return failed;
        }

        private static DbSyncScopeDescription GetScopeDescription(Scope scope, DbConnection conn)
        {
            DbSyncScopeDescription scopeDesc = null;
            if (conn is SqlCeConnection)
                scopeDesc = SqlCeSyncDescriptionBuilder.GetDescriptionForScope(scope.ToScopeString(), prefix, (SqlCeConnection)conn);
            else if (conn is SqlConnection)
                scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(scope.ToScopeString(), prefix, (SqlConnection)conn);

            return scopeDesc;
        }
        private static void DeprovisionSqlCe(SqlCeConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlCeSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectPrefix = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }

        private static void DeprovisionSqlServer(SqlConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectSchema = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }
    }
}
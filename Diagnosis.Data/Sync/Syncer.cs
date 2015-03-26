using Diagnosis.Common;
using Diagnosis.Data.Versions;
using Diagnosis.Models;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnosis.Data.Sync
{
    public class Syncer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Syncer));

        private static Stopwatch sw = new Stopwatch();
        private static string prefix = Scopes.syncPrefix;

        private static bool inSync;
        private static object lockInSync = new object();

        //private DbConnection clientCon;
        //private DbConnection serverCon;

        private string serverConStr;
        private string clientConStr;

        private string serverProviderName;
        private string clientProviderName;

        public Syncer(string serverConStr, string clientConStr, string serverProviderName)
        {
            this.serverConStr = serverConStr;
            this.clientConStr = clientConStr;
            this.serverProviderName = serverProviderName;
            this.clientProviderName = Constants.SqlCeProvider;
        }

        public static event EventHandler<StringEventArgs> MessagePosted;

        public static event EventHandler<TimeSpanEventArgs> SyncEnded;

        public static bool InSync
        {
            get { return inSync; }
            private set
            {
                lock (lockInSync)
                {
                    if (inSync != value)
                    {
                        inSync = value;
                        if (value)
                        {
                            sw.Restart();
                        }
                        else
                        {
                            sw.Stop();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ID добавленных во время последней синхронизации сущностей.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> AddedIdsPerType { get; set; }
        /// <summary>
        /// ID сущностей, которые должны быть удалены.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> DeletedIdsPerType { get; set; }

        /// <summary>
        /// Sync scopes determined by <paramref name="from"/>. Specify conncetions in correct order in ctor.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public Task SendFrom(Side from)
        {
            return SendFrom(from, from.GetOrderedScopes());
        }

        /// <summary>
        /// Sync specified scopes.
        /// We can reverse both <paramref name="from"/> and connections in ctor to get same result.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public Task SendFrom(Side from, IEnumerable<Scope> scopes)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                foreach (var scope in scopes.OrderScopes())
                {
                    var toContinue = Sync(from, scope);
                    if (!toContinue)
                        break;
                }
            });

            t.Start();
            t.ContinueWith((task) =>
            {
                InSync = false;
                OnSyncEnded(sw.Elapsed);
            });
            return t;
        }

        public Task Deprovision(Side side, IEnumerable<Scope> scopesToDeprovision = null)
        {
            if (side == Side.Client)
            {
                return Deprovision(clientConStr, clientProviderName, scopesToDeprovision);
            }
            return Deprovision(serverConStr, serverProviderName, scopesToDeprovision);
        }
        public static Task Deprovision(string connstr, string provider, IEnumerable<Scope> scopesToDeprovision = null)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                using (var conn = CreateConnection(connstr, provider))
                {
                    if (conn.IsAvailable())
                    {
                        var scopes = scopesToDeprovision ?? Scopes.GetOrderedScopes();
                        foreach (Scope scope in scopes)
                        {
                            Deprovision(conn, scope);
                        }
                    }
                    else
                    {
                        CanNotConnect(conn);
                    }
                }
            });

            t.Start();
            t.ContinueWith((task) => InSync = false);
            return t;
        }

        public static void BeforeMigrate(string connectionString, string provider, params string[] tables)
        {
            using (var con = CreateConnection(connectionString, provider))
            {
                if (!con.IsAvailable())
                {
                    CanNotConnect(con);
                    return;
                }

                HashSet<Scope> scopes = new HashSet<Scope>();
                foreach (Scope scope in Scopes.GetOrderedScopes())
                {
                    foreach (var table in tables)
                    {
                        if (scope.ToTableNames().Contains(table))
                        {
                            scopes.Add(scope);
                            break;
                        }
                    }
                }
                foreach (var scope in scopes)
                {
                    Deprovision(con, scope);
                }
            }
        }

        private DbConnection CreateConnection(Side db)
        {
            if (db == Side.Client)
            {
                return CreateConnection(clientConStr, clientProviderName);
            }
            else
            {
                return CreateConnection(serverConStr, serverProviderName);
            }
        }

        private static DbConnection CreateConnection(string connstr, string provider)
        {
            try
            {
                return SqlHelper.CreateConnection(connstr, provider);
            }
            catch (Exception ex)
            {
                Poster.PostMessage(ex);
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="scope"></param>
        /// <returns>Could connect</returns>
        private bool Sync(Side from, Scope scope)
        {
            Poster.PostMessage("Begin sync scope '{0}' from {1}", scope, from);
            using (var serverConn = CreateConnection(Side.Server))
            using (var clientConn = CreateConnection(Side.Client))
            {
                if (!serverConn.IsAvailable())
                {
                    CanNotConnect(serverConn);
                    return false;
                }
                if (!clientConn.IsAvailable())
                {
                    CanNotConnect(clientConn);
                    return false;
                }

                Provision(serverConn, scope, null);
                Provision(clientConn, scope, serverConn);

                var scopeName = scope.ToScopeString();
                var clientProvider = CreateProvider(clientConn, scopeName);
                var serverProvider = CreateProvider(serverConn, scopeName);

                DownloadSyncOrchestrator syncOrchestrator;
                switch (from)
                {
                    case Side.Client:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            clientProvider,
                            serverProvider);
                        break;

                    default:
                    case Side.Server:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            serverProvider,
                            clientProvider,
                            tablesToTrackAdding: Scope.Reference.ToTableNames(),  // на клиенте могут быть справочные сущности с другими ID, но такие же по значению
                            tablesToTrackDeleting: Names.VocabularyTbl.ToEnumerable(), //  перед удалением словаря надо убрать связь со словами на клиенте
                            tablesToIgnoreAdding: new[] { Names.SpecialityVocabulariesTbl, Names.VocabularyTbl, Names.WordTemplateTbl } // новые словари загружаются отдельно
                          );
                        break;
                }

                try
                {
                    Poster.PostMessage("Synchronize...");

                    var syncStats = syncOrchestrator.Synchronize();

                    // выводим статистику конфликтов
                    var conflicts = syncOrchestrator.ConflictsCounter.Keys.Where(k => syncOrchestrator.ConflictsCounter[k] > 0).ToList();
                    if (conflicts.Count > 0)
                    {
                        Poster.PostMessage("Conflicts:");
                        conflicts.ForAll((conflict) =>
                        {
                            Poster.PostMessage("{0} = {1}", conflict, syncOrchestrator.ConflictsCounter[conflict]);
                        });
                    }

                    // запоминаем добавленные строки
                    AddedIdsPerType = new Dictionary<Type, IEnumerable<object>>();
                    foreach (var table in syncOrchestrator.AddedIdsPerTable.Keys)
                    {
                        AddedIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.AddedIdsPerTable[table];
                    }

                    DeletedIdsPerType = new Dictionary<Type, IEnumerable<object>>();
                    foreach (var table in syncOrchestrator.DeletedIdsPerTable.Keys)
                    {
                        DeletedIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.DeletedIdsPerTable[table];
                    }

                    Poster.PostMessage("ChangesApplied: {0}, ChangesFailed: {1}\n", syncStats.DownloadChangesApplied, syncStats.DownloadChangesFailed);
                }
                catch (Exception ex)
                {
                    Poster.PostMessage(ex);
                }
                return true;
            }
        }

        private static RelationalSyncProvider CreateProvider(DbConnection conn, string scopeName)
        {
            RelationalSyncProvider serverProvider;
            if (conn is SqlCeConnection)
                serverProvider = new SqlCeSyncProvider(scopeName, conn as SqlCeConnection, prefix);
            else if (conn is SqlConnection)
                serverProvider = new SqlSyncProvider(scopeName, conn as SqlConnection, prefix);
            else throw new ArgumentOutOfRangeException();

            return serverProvider;
        }

        private static void Provision(DbConnection con, Scope scope, DbConnection conToGetScopeDescr)
        {
            var scopeDescr = new DbSyncScopeDescription(scope.ToScopeString());

            var failedTables = AddTablesToScopeDescr(scope.ToTableNames(), scopeDescr, con);

            try
            {
                var applied = false;
                if (con is SqlCeConnection)
                {
                    var fromScope = false;
                    if (failedTables.Count > 0 && conToGetScopeDescr != null)
                    {
                        Poster.PostMessage("Retrieve description for scope '{0}' from '{1}'", scope.ToScopeString(), con.ConnectionString);
                        scopeDescr = GetScopeDescription(scope, conToGetScopeDescr);
                        fromScope = true;
                    }

                    applied = ProvisionSqlCe(con as SqlCeConnection, scopeDescr, fromScope);
                }
                else if (con is SqlConnection)
                {
                    applied = ProvisionSqlServer(con as SqlConnection, scopeDescr);
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

        private static bool ProvisionSqlCe(SqlCeConnection con, DbSyncScopeDescription scope, bool populateFromScope)
        {
            var sqlceProv = new SqlCeSyncScopeProvisioning(con, scope);
            sqlceProv.ObjectPrefix = prefix;

            if (!sqlceProv.ScopeExists(scope.ScopeName))
            {
                if (populateFromScope)
                    //use scope description from server to intitialize the client
                    sqlceProv.PopulateFromScopeDescription(scope);

                sqlceProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlceProv.Apply();
                return true;
            }
            return false;
        }

        private static bool ProvisionSqlServer(SqlConnection con, DbSyncScopeDescription scope)
        {
            var sqlProv = new SqlSyncScopeProvisioning(con, scope);
            sqlProv.ObjectSchema = prefix;

            if (!sqlProv.ScopeExists(scope.ScopeName))
            {
                sqlProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlProv.Apply();
                return true;
            }
            return false;
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

        private static void Deprovision(DbConnection con, Scope scope)
        {
            try
            {
                if (con is SqlCeConnection)
                    DeprovisionSqlCe(con as SqlCeConnection, scope.ToScopeString());
                else if (con is SqlConnection)
                    DeprovisionSqlServer(con as SqlConnection, scope.ToScopeString());

                Poster.PostMessage("Deprovisioned '{0}' in '{1}'\n", scope.ToScopeString(), con.ConnectionString);
            }
            catch (Exception ex)
            {
                Poster.PostMessage(ex);
            }
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

        /// <summary>
        /// Добавляет таблицы в область синхронизации.
        /// </summary>
        /// <param name="tableNames"></param>
        /// <param name="scope"></param>
        /// <param name="connection"></param>
        /// <returns>Таблицы, которые не были добавлены.</returns>
        private static IList<string> AddTablesToScopeDescr(string[] tableNames, DbSyncScopeDescription scope, DbConnection connection)
        {
            var failed = new List<string>();
            Poster.PostMessage("Adding tables to scope '{0}' in '{1}'...", scope.ScopeName, connection.ConnectionString);

            foreach (var name in tableNames)
            {
                try
                {
                    DbSyncTableDescription desc;
                    if (connection is SqlCeConnection)
                        desc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable(name, connection as SqlCeConnection);
                    else if (connection is SqlConnection)
                        desc = SqlSyncDescriptionBuilder.GetDescriptionForTable(name.ToSchema() + '.' + name, connection as SqlConnection);
                    else throw new ArgumentOutOfRangeException();

                    desc.GlobalName = name;
                    scope.Tables.Add(desc);
                    Poster.PostMessage("Table '{0}' added", name);
                }
                catch (Exception ex)
                {
                    Poster.PostMessage(ex);
                    failed.Add(name);
                }
            }
            return failed;
        }

        private static void CanNotConnect(DbConnection con)
        {
            Poster.PostMessage("Невозможно поключиться к {0}", con.ConnectionString);
        }

        private static Task GetEndedTask()
        {
            var t = new Task(() => { });
            t.Start();
            return t;
        }

        private static void OnSyncEnded(TimeSpan time)
        {
            var h = SyncEnded;
            if (h != null)
            {
                h(typeof(Syncer), new TimeSpanEventArgs(time));
            }
        }

        private static class Poster
        {
            public static void PostMessage(string str)
            {
                logger.DebugFormat(str);
                var h = MessagePosted;
                if (h != null)
                {
                    h(typeof(Syncer), new StringEventArgs(str));
                }
            }

            public static void PostMessage(string str, params object[] p)
            {
                logger.DebugFormat(string.Format(str, p));
                var h = MessagePosted;
                if (h != null)
                {
                    h(typeof(Syncer), new StringEventArgs(string.Format(str, p)));
                }
            }

            public static void PostMessage(Exception ex)
            {
                logger.DebugFormat(ex.ToString());
                var h = MessagePosted;
                if (h != null)
                {
                    h(typeof(Syncer), new StringEventArgs(ex.Message));
                }
            }
        }

        public class DownloadSyncOrchestrator : SyncOrchestrator
        {
            public DownloadSyncOrchestrator(RelationalSyncProvider from, RelationalSyncProvider to,
                IEnumerable<string> tablesToTrackAdding = null,
                IEnumerable<string> tablesToTrackDeleting = null,
                IEnumerable<string> tablesToIgnoreAdding = null)
            {
                ConflictsCounter = new Dictionary<DbConflictType, int>();
                AddedIdsPerTable = new Dictionary<string, IEnumerable<object>>();
                DeletedIdsPerTable = new Dictionary<string, IEnumerable<object>>();

                this.RemoteProvider = from;
                this.LocalProvider = to;
                this.Direction = SyncDirectionOrder.Download;

                to.ChangesSelected += (s, e) =>
                {

                };

                to.ApplyChangeFailed += (s, e) =>
                {
#if DEBUG
                    if (to.ScopeName == Scope.Icd.ToScopeString())
                        return;
#endif
                    if (e.Conflict.Type == DbConflictType.ErrorsOccurred)
                    {
                        var rows = e.Conflict.LocalChange.Rows.Cast<DataRow>();
                        Poster.PostMessage("ApplyChangeFailed. Error: {0}", e.Error);
                    }
                    //else
                    //    Poster.PostMessage("ApplyChangeFailed. ConflictType: {0}", e.Conflict.Type);

                    if (!ConflictsCounter.Keys.Contains(e.Conflict.Type))
                        ConflictsCounter[e.Conflict.Type] = 0;

                    ConflictsCounter[e.Conflict.Type]++;

                    if (e.Conflict.Type != DbConflictType.ErrorsOccurred)
                        e.Action = ApplyAction.RetryWithForceWrite;
                };
                to.ChangesApplied += (s, e) =>
                {
                    // запоминаем добавленные строки для желаемых таблиц
                    if (tablesToTrackAdding != null)
                    {
                        foreach (var table in tablesToTrackAdding)
                        {
                            if (e.Context.DataSet.Tables.Contains(table))
                            {
                                var dataTable = e.Context.DataSet.Tables[table];
                                var addedRows = new List<DataRow>();
                                var deletedRows = new List<DataRow>();
                                for (int j = 0; j < dataTable.Rows.Count; j++)
                                {
                                    DataRow row = dataTable.Rows[j];

                                    if (row.RowState == DataRowState.Added)
                                    {
                                        addedRows.Add(row);
                                    }
                                    else if (row.RowState == DataRowState.Deleted)
                                    {
                                        deletedRows.Add(row);
                                    }
                                }
                                AddedIdsPerTable.Add(dataTable.TableName, addedRows.Select(x => x["Id"]));

                                //DeletedIdsPerTable.Add(
                                //    dataTable.TableName,
                                //    dataTable.Rows
                                //        .Cast<DataRow>()
                                //        .Where(x => x.RowState == DataRowState.Deleted)
                                //        .Select(x => x["Id"]));
                            }
                        }
                    }
                    // запоминаем удаляемые строки для желаемых таблиц
                    if (tablesToTrackDeleting != null)
                    {
                        foreach (var table in tablesToTrackDeleting)
                        {
                            if (e.Context.DataSet.Tables.Contains(table))
                            {
                                var dataTable = e.Context.DataSet.Tables[table];


                                DeletedIdsPerTable.Add(
                                    dataTable.TableName,
                                    dataTable.Rows
                                        .Cast<DataRow>()
                                        .Where(x => x.RowState == DataRowState.Deleted)
                                        .Select(x => x["Id"]));
                            }
                        }
                    }
                    if (tablesToIgnoreAdding != null)
                    {
                        foreach (var table in tablesToIgnoreAdding)
                        {
                            if (e.Context.DataSet.Tables.Contains(table))
                            {
                                var dataTable = e.Context.DataSet.Tables[table];

                                for (int j = 0; j < dataTable.Rows.Count; j++)
                                {
                                    DataRow row = dataTable.Rows[j];
                                    if (row.RowState == DataRowState.Added)
                                    {
                                        // не синхронизируем новые словари
                                        dataTable.Rows.Remove(row);
                                        j--;
                                    }
                                }
                            }
                        }
                    }
                };
                to.ApplyingChanges += (s, e) =>
                {
                };
                to.DbConnectionFailure += (s, e) =>
                {
                    Poster.PostMessage("DbConnectionFailure: {0}", e.FailureException.Message);
                };
            }

            public Dictionary<DbConflictType, int> ConflictsCounter { get; set; }

            public Dictionary<string, IEnumerable<object>> AddedIdsPerTable { get; set; }
            public Dictionary<string, IEnumerable<object>> DeletedIdsPerTable { get; set; }
        }
    }
}
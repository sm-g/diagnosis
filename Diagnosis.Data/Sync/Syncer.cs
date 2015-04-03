using Diagnosis.Common;
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
using System.Text;
using System.Threading.Tasks;

namespace Diagnosis.Data.Sync
{
    public class Syncer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Syncer));

        private static Dictionary<string, DbSyncTableDescription> map = new Dictionary<string, DbSyncTableDescription>();
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
        public Dictionary<Type, IEnumerable<object>> AddedIdsPerType { get; private set; }

        /// <summary>
        /// ID сущностей, которые должны быть удалены на клиенте.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> DeletedIdsPerType { get; private set; }

        /// <summary>
        /// Не синхронизировать новые сущности в таблицах.
        /// </summary>
        public IEnumerable<string> TablesIgnoreAdding { get; set; }

        /// <summary>
        /// Синхронизировать только эти сущности.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> IdsToSyncPerType { get; set; }

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

            AddedIdsPerType = new Dictionary<Type, IEnumerable<object>>();
            DeletedIdsPerType = new Dictionary<Type, IEnumerable<object>>();

            var t = new Task(() =>
            {
                Poster.PostMessage("Going to sync scopes: '{0}' from {1}\n", string.Join("', '", scopes), from);
                using (var serverConn = CreateConnection(Side.Server))
                using (var clientConn = CreateConnection(Side.Client))
                {
                    if (!serverConn.IsAvailable())
                    {
                        CanNotConnect(serverConn);
                        return;
                    }
                    if (!clientConn.IsAvailable())
                    {
                        CanNotConnect(clientConn);
                        return;
                    }

                    foreach (var scope in scopes.OrderScopes())
                    {
                        Sync(from, scope, serverConn, clientConn);
                    }
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
        private void Sync(Side from, Scope scope, DbConnection serverConn, DbConnection clientConn)
        {
            Poster.PostMessage("Begin sync scope '{0}'", scope);

            Provision(serverConn, scope, null);
            Provision(clientConn, scope, serverConn);

            var scopeName = scope.ToScopeString();
            var clientProvider = CreateProvider(clientConn, scopeName);
            var serverProvider = CreateProvider(serverConn, scopeName);

            var syncOrchestrator = CreateOrch(from, clientProvider, serverProvider);

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
                foreach (var table in syncOrchestrator.AddedIdsPerTable.Keys)
                {
                    AddedIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.AddedIdsPerTable[table];
                }

                foreach (var table in syncOrchestrator.DeletedIdsPerTable.Keys)
                {
                    DeletedIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.DeletedIdsPerTable[table];
                }

                Poster.PostMessage("ChangesApplied: {0}, ChangesFailed: {1}", syncStats.DownloadChangesApplied, syncStats.DownloadChangesFailed);
            }
            catch (Exception ex)
            {
                Poster.PostMessage(ex);
            }
            finally
            {
                Poster.PostMessage("End sync scope '{0}' \n", scope);
            }
        }

        private DownloadSyncOrchestrator CreateOrch(Side from, RelationalSyncProvider clientProvider, RelationalSyncProvider serverProvider)
        {
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
                        clientProvider)
                        {
                            TablesToTrackAdding = Scope.Reference.ToTableNames()  // на клиенте могут быть справочные сущности с другими ID, но такие же по значению
                            ,
                            TablesToTrackDeleting = Names.Vocabulary.ToEnumerable() //  перед удалением словаря надо убрать связь со словами на клиенте
                            ,
                            TablesToIgnoreAdding = TablesIgnoreAdding
                            ,
                            TableIdsToSync = IdsToSyncPerType != null ? IdsToSyncPerType.ToDictionary(x => Names.GetTblByType(x.Key), x => x.Value) : null
                        };
                    break;
            }
            return syncOrchestrator;
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
                        Poster.PostMessage("GetScopeDescription for scope '{0}' from '{1}'", scope.ToScopeString(), con.ConnectionString);
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
                    {
                        if (!map.TryGetValue(name, out desc))
                        {
                            desc = SqlCeSyncDescriptionBuilder.GetDescriptionForTable(name, connection as SqlCeConnection);
                            map[name] = desc;
                        }
                    }
                    else if (connection is SqlConnection)
                    {
                        var nameWithSchema = name.GetSchemaForTable() + '.' + name;
                        if (!map.TryGetValue(nameWithSchema, out desc))
                        {
                            desc = SqlSyncDescriptionBuilder.GetDescriptionForTable(nameWithSchema, connection as SqlConnection);
                            map[nameWithSchema] = desc;
                        }
                    }
                    else throw new ArgumentOutOfRangeException();

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
                Send(str);
            }

            public static void PostMessage(string str, params object[] p)
            {
                logger.DebugFormat(string.Format(str, p));
                Send(string.Format(str, p));
            }

            public static void PostMessage(Exception ex)
            {
                logger.WarnFormat(ex.ToString());
                Send(ex.Message);
            }

            private static void Send(string str)
            {
                var h = MessagePosted;
                if (h != null)
                {
                    h(typeof(Syncer), new StringEventArgs(str));
                }
            }
        }

        public class DownloadSyncOrchestrator : SyncOrchestrator
        {
            public DownloadSyncOrchestrator(RelationalSyncProvider from, RelationalSyncProvider to)
            {
                ConflictsCounter = new Dictionary<DbConflictType, int>();
                AddedIdsPerTable = new Dictionary<string, IEnumerable<object>>();
                DeletedIdsPerTable = new Dictionary<string, IEnumerable<object>>();

                this.RemoteProvider = from;
                this.LocalProvider = to;
                this.Direction = SyncDirectionOrder.Download;

                from.ChangesSelected += from_ChangesSelected;
                to.ApplyChangeFailed += to_ApplyChangeFailed;
                to.ChangesApplied += to_ChangesApplied;
                to.DbConnectionFailure += (s, e) =>
                {
                    Poster.PostMessage("DbConnectionFailure: {0}", e.FailureException.Message);
                };
            }

            public IEnumerable<string> TablesToTrackAdding { get; set; }

            public IEnumerable<string> TablesToTrackDeleting { get; set; }

            public IEnumerable<string> TablesToIgnoreAdding { get; set; }

            public Dictionary<String, IEnumerable<object>> TableIdsToSync { get; set; }

            public Dictionary<DbConflictType, int> ConflictsCounter { get; private set; }

            public Dictionary<string, IEnumerable<object>> AddedIdsPerTable { get; private set; }

            public Dictionary<string, IEnumerable<object>> DeletedIdsPerTable { get; private set; }

            private void from_ChangesSelected(object sender, DbChangesSelectedEventArgs e)
            {
                if (TablesToIgnoreAdding != null)
                {
                    foreach (var table in TablesToIgnoreAdding)
                    {
                        DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                        {
                            if (row.RowState == DataRowState.Added)
                            {
                                // не синхронизируем новые словари
                                dataTable.Rows.Remove(row);
                            }
                        });
                    }
                }
                if (TableIdsToSync != null)
                {
                    foreach (var table in TableIdsToSync.Keys)
                    {
                        DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                        {
                            if (!TableIdsToSync[table].Contains(row["Id"]))
                            {
                                dataTable.Rows.Remove(row);
                            }
                        });
                    }
                }
            }

            private void to_ChangesApplied(object sender, DbChangesAppliedEventArgs e)
            {
                // запоминаем добавленные строки для желаемых таблиц
                if (TablesToTrackAdding != null)
                {
                    foreach (var table in TablesToTrackAdding)
                    {
                        var addedRows = new List<DataRow>();

                        DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                        {
                            if (row.RowState == DataRowState.Added)
                            {
                                addedRows.Add(row);
                            }
                        });

                        AddedIdsPerTable.Add(table, addedRows.Select(x => x["Id"]));
                    }
                }
                // запоминаем удаляемые строки для желаемых таблиц
                if (TablesToTrackDeleting != null)
                {
                    foreach (var table in TablesToTrackDeleting)
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
            }

            private void to_ApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs e)
            {
                var toProvider = sender as RelationalSyncProvider;

#if DEBUG
                if (toProvider.ScopeName == Scope.Icd.ToScopeString())
                    return;
#endif
                if (e.Conflict.Type == DbConflictType.ErrorsOccurred)
                {
                    var rows = e.Conflict.LocalChange.Rows.Cast<DataRow>();
                    Poster.PostMessage("ApplyChangeFailed. Error: {0}", e.Error);
                }
                else if (SyncTracer.IsVerboseEnabled() == false)
                {
                    DataTable conflictingServerChange = e.Conflict.RemoteChange;
                    DataTable conflictingClientChange = e.Conflict.LocalChange;
                    int serverColumnCount = conflictingServerChange.Columns.Count;
                    int clientColumnCount = conflictingClientChange.Columns.Count;
                    StringBuilder clientRowAsString = new StringBuilder();
                    StringBuilder serverRowAsString = new StringBuilder();

                    for (int i = 0; i < clientColumnCount; i++)
                    {
                        clientRowAsString.Append(conflictingClientChange.Rows[0][i] + " | ");
                    }

                    for (int i = 0; i < serverColumnCount; i++)
                    {
                        serverRowAsString.Append(conflictingServerChange.Rows[0][i] + " | ");
                    }

                    SyncTracer.Warning(1, "CONFLICT DETECTED FOR CLIENT {0}", toProvider.Connection);
                    SyncTracer.Warning(2, "** Client change **");
                    SyncTracer.Warning(2, clientRowAsString.ToString());
                    SyncTracer.Warning(2, "** Server change **");
                    SyncTracer.Warning(2, serverRowAsString.ToString());
                }

                if (!ConflictsCounter.Keys.Contains(e.Conflict.Type))
                    ConflictsCounter[e.Conflict.Type] = 0;

                ConflictsCounter[e.Conflict.Type]++;

                if (e.Conflict.Type != DbConflictType.ErrorsOccurred)
                    e.Action = ApplyAction.RetryWithForceWrite;
            }

            private void DoPerTableRow(DataTableCollection tables, string table, Action<DataTable, DataRow> act)
            {
                if (tables.Contains(table))
                {
                    var dataTable = tables[table];
                    var rows = dataTable.Rows.Cast<DataRow>().ToList();

                    foreach (var row in rows)
                    {
                        act(dataTable, row);
                    }
                }
            }
        }
    }
}
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

        private string serverConStr;
        private string clientConStr;

        private string serverProviderName;
        private string clientProviderName;

        private Dictionary<Type, Action<DataRow>> _shaper;
        private Dictionary<Type, Func<DataRow, bool>> _ignoreAdding;
        private Dictionary<Type, IEnumerable<object>> _idsForSync;

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
        public Dictionary<Type, IEnumerable<object>> AddedOnServerIdsPerType { get; private set; }

        /// <summary>
        /// ID сущностей, которые должны быть удалены на клиенте после загрузки с сервера.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> DeletedOnServerIdsPerType { get; private set; }

        /// <summary>
        /// Синхронизировать только эти сущности.
        /// </summary>
        public Dictionary<Type, IEnumerable<object>> IdsForSyncPerType { get { return _idsForSync ?? (_idsForSync = new Dictionary<Type, IEnumerable<object>>()); } }

        /// <summary>
        /// Не синхронизировать новые сущности, проходящие фильтр.
        /// </summary>
        public Dictionary<Type, Func<DataRow, bool>> IgnoreAddingFilterPerType { get { return _ignoreAdding ?? (_ignoreAdding = new Dictionary<Type, Func<DataRow, bool>>()); } }

        /// <summary>
        /// Обработчик сущностей перед отправкой с клиента.
        /// </summary>
        public Dictionary<Type, Action<DataRow>> ShaperPerType { get { return _shaper ?? (_shaper = new Dictionary<Type, Action<DataRow>>()); } }

        /// <summary>
        /// Sync specified scopes.
        /// Specify conncetions in correct order in ctor.
        /// We can reverse both <paramref name="from"/> and connections in ctor to get same result.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public async Task SendFrom(Side from, IEnumerable<Scope> scopes = null)
        {
            if (InSync)
                return;

            InSync = true;

            if (scopes == null)
            {
                scopes = from.GetOrderedScopes();
            }

            var t = new Task(() =>
            {
                SendFromCore(from, scopes);
            });

            t.Start();
            await t;

            InSync = false;
            OnSyncEnded(sw.Elapsed);
        }

        internal void SendFromCore(Side from, IEnumerable<Scope> scopes)
        {
            AddedOnServerIdsPerType = new Dictionary<Type, IEnumerable<object>>();
            DeletedOnServerIdsPerType = new Dictionary<Type, IEnumerable<object>>();

            Poster.PostMessage("+++\nGoing to sync scopes: '{0}' from {1}\n", string.Join("', '", scopes), from);
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
        }

        public async Task Deprovision(Side side, IEnumerable<Scope> scopesToDeprovision = null)
        {
            if (side == Side.Client)
                await Deprovision(clientConStr, clientProviderName, scopesToDeprovision);
            else
                await Deprovision(serverConStr, serverProviderName, scopesToDeprovision);
        }

        public async static Task Deprovision(string connstr, string provider, IEnumerable<Scope> scopesToDeprovision = null)
        {
            if (InSync)
                return;

            InSync = true;

            var t = new Task(() =>
            {
                var scopes = scopesToDeprovision ?? Scopes.GetOrderedScopes();
                if (scopes.Count() == 0)
                    return;

                Poster.PostMessage("Going to deprovision scopes: '{0}' in '{1}'\n", string.Join("', '", scopes), connstr);
                using (var conn = CreateConnection(connstr, provider))
                {
                    if (conn.IsAvailable())
                    {
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
            await t;

            InSync = false;
        }

        public static void BeforeMigrate(string connstr, string provider, params string[] tables)
        {
            Poster.PostMessage("BeforeMigrate tables: '{0}' in '{1}'\n", string.Join("', '", tables));

            using (var con = CreateConnection(connstr, provider))
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
                Poster.PostMessage("Going to deprovision scopes: '{0}' in '{1}'\n", string.Join("', '", scopes, connstr));
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
                    AddedOnServerIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.AddedIdsPerTable[table];
                }

                foreach (var table in syncOrchestrator.DeletedIdsPerTable.Keys)
                {
                    DeletedOnServerIdsPerType[Names.tblToTypeMap[table]] = syncOrchestrator.DeletedIdsPerTable[table];
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
                            // на клиенте могут быть справочные сущности с другими ID, но такие же по значению
                            TablesTrackAdding = Scope.Reference.ToTableNames(),
                            //  перед удалением словаря надо убрать связь со словами на клиенте
                            TablesTrackDeleting = new[] { Names.Vocabulary },
                        };
                    break;
            }
            syncOrchestrator.TableRowsShaper = ShaperPerType.ToDictionary(
                                x => Names.GetTblByType(x.Key),
                                x => x.Value);
            syncOrchestrator.TableToIdsForSync = IdsForSyncPerType.ToDictionary(
                                x => Names.GetTblByType(x.Key),
                                x => x.Value);
            syncOrchestrator.TablesToIgnoreAddingFilter = IgnoreAddingFilterPerType.ToDictionary(
                x => Names.GetTblByType(x.Key),
                x => x.Value);
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

                Poster.PostMessage("Deprovisioned '{0}'\n", scope.ToScopeString());
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

        private static void CanNotConnect(DbConnection con)
        {
            Poster.PostMessage("Невозможно поключиться к {0}", con.ConnectionString);
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

                TablesTrackAdding = Enumerable.Empty<string>();
                TablesTrackDeleting = Enumerable.Empty<string>();
                TableToIdsForSync = new Dictionary<string, IEnumerable<object>>();
                TablesToIgnoreAddingFilter = new Dictionary<string, Func<DataRow, bool>>();
                TableRowsShaper = new Dictionary<string, Action<DataRow>>();
            }

            // settings
            public IEnumerable<string> TablesTrackAdding { get; set; }

            public IEnumerable<string> TablesTrackDeleting { get; set; }

            public Dictionary<string, IEnumerable<object>> TableToIdsForSync { get; set; }

            public Dictionary<string, Func<DataRow, bool>> TablesToIgnoreAddingFilter { get; set; }

            public Dictionary<string, Action<DataRow>> TableRowsShaper { get; set; }

            // results
            public Dictionary<DbConflictType, int> ConflictsCounter { get; private set; }

            public Dictionary<string, IEnumerable<object>> AddedIdsPerTable { get; private set; }

            public Dictionary<string, IEnumerable<object>> DeletedIdsPerTable { get; private set; }

            private void from_ChangesSelected(object sender, DbChangesSelectedEventArgs e)
            {
                foreach (var table in TablesToIgnoreAddingFilter.Keys)
                {
                    DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                    {
                        if (row.RowState == DataRowState.Added)
                        {
                            if (TablesToIgnoreAddingFilter[table](row))
                                dataTable.Rows.Remove(row);
                        }
                    });
                }
                foreach (var table in TableToIdsForSync.Keys)
                {
                    DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                    {
                        if (!TableToIdsForSync[table].Contains(row["Id"]))
                        {
                            dataTable.Rows.Remove(row);
                        }
                    });
                }

                foreach (var table in TableRowsShaper.Keys)
                {
                    DoPerTableRow(e.Context.DataSet.Tables, table, (dataTable, row) =>
                    {
                        TableRowsShaper[table](row);
                    });
                }
            }

            private void to_ChangesApplied(object sender, DbChangesAppliedEventArgs e)
            {
                // запоминаем добавленные строки для желаемых таблиц

                foreach (var table in TablesTrackAdding)
                {
                    if (e.Context.DataSet.Tables.Contains(table))
                    {
                        var dataTable = e.Context.DataSet.Tables[table];

                        AddedIdsPerTable.Add(
                               dataTable.TableName,
                               dataTable.Rows
                                   .Cast<DataRow>()
                                   .Where(x => x.RowState == DataRowState.Added)
                                   .Select(x => x["Id"])
                                   .ToList());
                    }
                }

                // запоминаем удаляемые строки для желаемых таблиц

                foreach (var table in TablesTrackDeleting)
                {
                    if (e.Context.DataSet.Tables.Contains(table))
                    {
                        var dataTable = e.Context.DataSet.Tables[table];

                        DeletedIdsPerTable.Add(
                            dataTable.TableName,
                            dataTable.Rows
                                .Cast<DataRow>()
                                .Where(x => x.RowState == DataRowState.Deleted)
                                .Select(x => x["Id"])
                                .ToList());
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
                    Poster.PostMessage("ApplyChangeFailed. Error: {0}", e.Error);
                }
                else if (SyncTracer.IsVerboseEnabled() == false)
                {
                    SyncTracer.Warning(1, "CONFLICT DETECTED FOR CLIENT {0}", toProvider.Connection);
                    SyncTracer.Warning(2, "** Local change **");
                    SyncTracer.Warning(2, TableToStr(e.Conflict.LocalChange));
                    SyncTracer.Warning(2, "** Remote change **");
                    SyncTracer.Warning(2, TableToStr(e.Conflict.RemoteChange));
                }

                if (!ConflictsCounter.Keys.Contains(e.Conflict.Type))
                    ConflictsCounter[e.Conflict.Type] = 0;

                ConflictsCounter[e.Conflict.Type]++;

                if (e.Conflict.Type != DbConflictType.ErrorsOccurred)
                    e.Action = ApplyAction.RetryWithForceWrite;
            }

            private string TableToStr(DataTable table)
            {
                if (table == null)
                    return string.Empty;
                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;
                var tableAsStr = new StringBuilder();

                for (int r = 0; r < rowCount; r++)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        tableAsStr.Append(table.Rows[r][i] + " | ");
                    }
                    tableAsStr.AppendLine();
                }
                return tableAsStr.ToString();
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
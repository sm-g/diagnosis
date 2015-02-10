using Diagnosis.Common;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnosis.Data.Sync
{
    public enum Db
    {
        Client, Server
    }

    public class Syncer
    {
        private const string sqlCeProvider = "System.Data.SqlServerCE.4.0";
        private const string sqlServerProvider = "System.Data.SqlClient";
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

        public Syncer(string serverConStr, string clientConStr, string serverProviderName)
        {
            this.serverConStr = serverConStr;
            this.clientConStr = clientConStr;
            this.serverProviderName = serverProviderName;
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
                    inSync = value;
                    if (value)
                        sw.Restart();
                    else
                    {
                        sw.Stop();
                        OnSyncEnded(sw.Elapsed);
                    }
                }
            }
        }

        public Task SendFrom(Db from)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                foreach (var scope in from.GetOrderedScopes())
                {
                    Sync(from, scope);
                }
            });

            t.Start();
            t.ContinueWith((task) => InSync = false);
            return t;
        }

        public Task Deprovision(Db db)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                using (var conn = CreateConnection(db))
                {
                    if (conn.IsAvailable())
                    {
                        foreach (Scope scope in Scopes.GetOrderedScopes())
                        {
                            Deprovision(conn, scope);
                        }
                    }
                    else
                    {
                        PostMessage("Невозможно поключиться к {0}", conn);
                    }
                }
            });

            t.Start();
            t.ContinueWith((task) => InSync = false);
            return t;
        }

        public void BeforeMigrate(params string[] tables)
        {
            // TODO server?
            using (var clientConn = CreateConnection(Db.Client))
            {
                if (!clientConn.IsAvailable())
                {
                    PostMessage("Невозможно поключиться к {0}", clientConn);
                    return;
                }

                foreach (var table in tables)
                {
                    foreach (Scope scope in Scopes.GetOrderedScopes())
                    {
                        if (scope.ToTableNames().Contains(table))
                        {
                            Deprovision(clientConn, scope);
                            break;
                        }
                    }
                }
            }
        }

        private static void PostMessage(string str)
        {
            logger.DebugFormat(str);
            var h = MessagePosted;
            if (h != null)
            {
                h(typeof(Syncer), new StringEventArgs(str));
            }
        }

        private static void PostMessage(string str, params object[] p)
        {
            logger.DebugFormat(string.Format(str, p));
            var h = MessagePosted;
            if (h != null)
            {
                h(typeof(Syncer), new StringEventArgs(string.Format(str, p)));
            }
        }

        private static void PostMessage(Exception ex)
        {
            logger.DebugFormat(ex.ToString());
            var h = MessagePosted;
            if (h != null)
            {
                h(typeof(Syncer), new StringEventArgs(ex.Message));
            }
        }

        private static void OnSyncEnded(TimeSpan time)
        {
            var h = SyncEnded;
            if (h != null)
            {
                h(typeof(Syncer), new TimeSpanEventArgs(time));
            }
        }

        private DbConnection CreateConnection(Db db)
        {
            try
            {
                if (db == Db.Client)
                {
                    return new SqlCeConnection(clientConStr);
                }
                else
                {
                    switch (serverProviderName)
                    {
                        case sqlCeProvider:
                            return new SqlCeConnection(serverConStr);

                        case sqlServerProvider:
                            return new SqlConnection(serverConStr);

                        default:
                            throw new NotSupportedException();
                    }
                }
            }
            catch (Exception ex)
            {
                PostMessage(ex);

                return new SqlConnection(); // dummy, will throw later
            }
        }

        private void Sync(Db from, Scope scope)
        {
            PostMessage(string.Format("Sync '{0}' from {1}", scope, from));
            using (var serverConn = CreateConnection(Db.Server))
            using (var clientConn = (SqlCeConnection)CreateConnection(Db.Client))
            {
                if (!serverConn.IsAvailable())
                {
                    PostMessage("Невозможно поключиться к {0}", serverConn);
                    return;
                }
                if (!clientConn.IsAvailable())
                {
                    PostMessage("Невозможно поключиться к {0}", clientConn);
                    return;
                }

                Provision(serverConn, scope);
                Provision(clientConn, scope);

                DownloadSyncOrchestrator syncOrchestrator;
                SyncOperationStatistics syncStats;

                var scopeName = scope.ToScopeString();
                RelationalSyncProvider clientProvider = new SqlCeSyncProvider(scopeName, clientConn, prefix);
                RelationalSyncProvider serverProvider;

                if (serverConn is SqlCeConnection)
                    serverProvider = new SqlCeSyncProvider(scopeName, serverConn as SqlCeConnection, prefix);
                else if (serverConn is SqlConnection)
                    serverProvider = new SqlSyncProvider(scopeName, serverConn as SqlConnection, prefix);
                else throw new ArgumentOutOfRangeException();

                switch (from)
                {
                    case Db.Client:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            clientProvider,
                            serverProvider);
                        break;

                    case Db.Server:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            serverProvider,
                            clientProvider);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                try
                {
                    syncStats = syncOrchestrator.Synchronize();

                    PostMessage(string.Format("ChangesApplied: {0}, ChangesFailed: {1}", syncStats.DownloadChangesApplied, syncStats.DownloadChangesFailed));
                }
                catch (Exception ex)
                {
                    PostMessage(ex);
                }
            }
        }

        private void Provision(DbConnection con, Scope scope)
        {
            var scopeDescr = new DbSyncScopeDescription(scope.ToScopeString());
            AddTablesToScopeDescr(scope.ToTableNames(), scopeDescr, con);
            try
            {
                if (con is SqlCeConnection)
                    ProvisionSqlCe(con as SqlCeConnection, scopeDescr);
                else if (con is SqlConnection)
                    ProvisionSqlServer(con as SqlConnection, scopeDescr);
            }
            catch (Exception ex)
            {
                PostMessage(ex);
            }
        }

        private void ProvisionSqlCe(SqlCeConnection con, DbSyncScopeDescription scope)
        {
            var sqlceProv = new SqlCeSyncScopeProvisioning(con, scope);
            sqlceProv.ObjectPrefix = prefix;

            if (!sqlceProv.ScopeExists(scope.ScopeName))
            {
                sqlceProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlceProv.Apply();
                PostMessage(string.Format("Provisioned {0}", con.ConnectionString));
            }
        }

        private void ProvisionSqlServer(SqlConnection con, DbSyncScopeDescription scope)
        {
            var sqlProv = new SqlSyncScopeProvisioning(con, scope);
            sqlProv.ObjectSchema = prefix;

            if (!sqlProv.ScopeExists(scope.ScopeName))
            {
                sqlProv.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                sqlProv.Apply();
                PostMessage(string.Format("Provisioned {0}", con.ConnectionString));
            }
        }

        private void Deprovision(DbConnection con, Scope scope)
        {
            try
            {
                if (con is SqlCeConnection)
                    DeprovisionSqlCe(con as SqlCeConnection, scope.ToScopeString());
                else if (con is SqlConnection)
                    DeprovisionSqlServer(con as SqlConnection, scope.ToScopeString());

                PostMessage(string.Format("Deprovisioned scope '{0}' in '{1}'", scope.ToScopeString(), con.ConnectionString));
            }
            catch (Exception ex)
            {
                PostMessage(ex);
            }
        }

        private void DeprovisionSqlCe(SqlCeConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlCeSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectPrefix = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }

        private void DeprovisionSqlServer(SqlConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectSchema = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }

        private void AddTablesToScopeDescr(string[] tableNames, DbSyncScopeDescription scope, DbConnection connection)
        {
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
                }
                catch (Exception ex)
                {
                    PostMessage(ex);
                }
            }
        }

        private Task GetEndedTask()
        {
            var t = new Task(() => { });
            t.Start();
            return t;
        }

        public class DownloadSyncOrchestrator : SyncOrchestrator
        {
            public DownloadSyncOrchestrator(RelationalSyncProvider from, RelationalSyncProvider to)
            {
                this.RemoteProvider = from;
                this.LocalProvider = to;
                this.Direction = SyncDirectionOrder.Download;

                to.ApplyChangeFailed += (s, e) =>
                {
                    PostMessage(string.Format("ApplyChangeFailed\n {0}\n LocalChange.Rows\n", e.Error));
                    //foreach (DataRow row in e.Conflict.LocalChange.Rows)
                    //{
                    //    PostMessage("{0}", row.ItemArray);

                    //}
                    //PostMessage(" LocalChange.Rows\n");

                    //foreach (DataRow row in e.Conflict.RemoteChange.Rows)
                    //{
                    //    PostMessage("{0}\n", row.ItemArray);

                    //}
                };
                to.DbConnectionFailure += (s, e) =>
                {
                    PostMessage(string.Format("DbConnectionFailure\n {0}", e.FailureException.Message));
                };
            }
        }
    }
}
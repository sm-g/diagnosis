using Diagnosis.Common;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
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
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Syncer));

        private static string prefix = Scopes.syncPrefix;

        private static string server = "Data Source=SERVER\\SQLEXPRESS;Initial Catalog=diagServer; Trusted_Connection=True;";

        private static bool inSync;
        private static object lockInSync = new object();

        public static event EventHandler<StringEventArgs> MessagePosted;

        public static event EventHandler SyncEnded;

        public static bool InSync
        {
            get { return inSync; }
            private set
            {
                lock (lockInSync)
                {
                    inSync = value;
                    if (!value)
                        OnSyncEnded(EventArgs.Empty);
                }
            }
        }

        public static Task SendFrom(Db from)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                if (from == Db.Client)
                {
                    Sync(Db.Client, Scope.user);
                    Sync(Db.Client, Scope.holder);
                    Sync(Db.Client, Scope.hr);
                }
                else
                {
                    Sync(Db.Server, Scope.reference);
                }
            });

            t.Start();
            t.ContinueWith((task) => InSync = false);
            return t;
        }

        public static Task Deprovision(Db db)
        {
            if (InSync)
                return GetEndedTask();

            InSync = true;

            var t = new Task(() =>
            {
                using (var conn = OpenConnection(db))
                {
                    Deprovision(conn, Scope.user);
                    Deprovision(conn, Scope.reference);
                    Deprovision(conn, Scope.hr);
                    Deprovision(conn, Scope.holder);
                }
            });

            t.Start();
            t.ContinueWith((task) => InSync = false);
            return t;
        }

        public static void BeforeMigrate(params string[] tables)
        {
            // TODO server?
            using (var clientConn = OpenConnection(Db.Client))
            {
                foreach (var table in tables)
                {
                    foreach (Scope scope in Enum.GetValues(typeof(Scope)))
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

        private static DbConnection OpenConnection(Db db)
        {
            if (db == Db.Client)
            {
                return new SqlCeConnection(NHibernateHelper.ConnectionString);
            }
            else
            {
                return new SqlConnection(server);
            }
        }

        private static void Sync(Db from, Scope scope)
        {
            PostMessage(string.Format("Sync '{0}' from {1}", scope, from));
            using (var serverConn = (SqlConnection)OpenConnection(Db.Server))
            using (var clientConn = (SqlCeConnection)OpenConnection(Db.Client))
            {
                Provision(serverConn, scope);
                Provision(clientConn, scope);

                DownloadSyncOrchestrator syncOrchestrator;
                SyncOperationStatistics syncStats;

                var scopeName = scope.ToScopeString();
                switch (from)
                {
                    case Db.Client:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            new SqlCeSyncProvider(scopeName, clientConn, prefix),
                            new SqlSyncProvider(scopeName, serverConn, null, prefix));
                        break;

                    case Db.Server:
                        syncOrchestrator = new DownloadSyncOrchestrator(
                            new SqlSyncProvider(scopeName, serverConn, null, prefix),
                            new SqlCeSyncProvider(scopeName, clientConn, prefix));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                try
                {
                    syncStats = syncOrchestrator.Synchronize();

                    PostMessage(string.Format("ChangesApplied: {0}, ChangesFailed: {1}", syncStats.DownloadChangesApplied, syncStats.DownloadChangesFailed));
                    //PostMessage(string.Format("UploadChangesApplied: {0}, UploadChangesFailed: {1}", syncStats.UploadChangesApplied, syncStats.UploadChangesFailed));
                }
                catch (Exception ex)
                {
                    PostMessage(ex.ToString());
                }
            }
        }

        private static void Provision(DbConnection con, Scope scope)
        {
            var scopeDescr = new DbSyncScopeDescription(scope.ToScopeString());
            AddTablesToScopeDescr(scope.ToTableNames(), scopeDescr, con);
            try
            {
                if (con is SqlCeConnection)
                    ProvisionClient(con as SqlCeConnection, scopeDescr);
                else if (con is SqlConnection)
                    ProvisionServer(con as SqlConnection, scopeDescr);
            }
            catch (Exception ex)
            {
                PostMessage(ex.ToString());
            }
        }

        private static void ProvisionClient(SqlCeConnection clientConn, DbSyncScopeDescription scope)
        {
            var clientProvision = new SqlCeSyncScopeProvisioning(clientConn, scope);
            clientProvision.ObjectPrefix = prefix;

            if (!clientProvision.ScopeExists(scope.ScopeName))
            {
                clientProvision.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                clientProvision.Apply();
                PostMessage("Client Provisioned");
            }
        }

        private static void ProvisionServer(SqlConnection serverConn, DbSyncScopeDescription scope)
        {
            var serverConfig = new SqlSyncScopeProvisioning(serverConn, scope);
            serverConfig.ObjectSchema = prefix;

            if (!serverConfig.ScopeExists(scope.ScopeName))
            {
                serverConfig.Apply();
                PostMessage("Server Provisioned");
            }
        }

        private static void Deprovision(DbConnection con, Scope scope)
        {
            try
            {
                if (con is SqlCeConnection)
                    DeprovisionClient(con as SqlCeConnection, scope.ToScopeString());
                else if (con is SqlConnection)
                    DeprovisionServer(con as SqlConnection, scope.ToScopeString());

                PostMessage(string.Format("Deprovisioned scope '{0}' in '{1}'", scope.ToScopeString(), con.ConnectionString));
            }
            catch (Exception ex)
            {
                PostMessage(ex.ToString());
            }
        }

        private static void DeprovisionClient(SqlCeConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlCeSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectPrefix = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }

        private static void DeprovisionServer(SqlConnection con, string scopeName)
        {
            var scopeDeprovisioning = new SqlSyncScopeDeprovisioning(con);
            scopeDeprovisioning.ObjectSchema = prefix;
            scopeDeprovisioning.DeprovisionScope(scopeName);
        }

        private static void AddTablesToScopeDescr(string[] tableNames, DbSyncScopeDescription scope, DbConnection connection)
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
                    PostMessage(ex.ToString());
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

        private static void OnSyncEnded(EventArgs e)
        {
            var h = SyncEnded;
            if (h != null)
            {
                h(typeof(Syncer), e);
            }
        }

        private static Task GetEndedTask()
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
                    PostMessage(string.Format("ApplyChangeFailed\n {0}", e.Error));
                };
                to.DbConnectionFailure += (s, e) =>
                {
                    PostMessage(string.Format("DbConnectionFailure\n {0}", e.FailureException.Message));
                };
            }
        }
    }
}
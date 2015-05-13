using Diagnosis.Common;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnosis.Data.Sync
{
    public class Syncer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Syncer));

        private static Stopwatch sw = new Stopwatch();

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

        /// <summary>
        /// Sync specified scope.
        /// Specify conncetions in correct order in ctor.
        /// We can reverse both <paramref name="from"/> and connections in ctor to get same result.
        /// </summary>
        public async Task SendFrom(Side from, Scope scope)
        {
            await SendFrom(from, scope.ToEnumerable());
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
                            SyncUtil.Deprovision(conn, scope);
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
            Contract.Requires(!InSync);

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
                    SyncUtil.Deprovision(con, scope);
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

        private void SendFromCore(Side from, IEnumerable<Scope> scopes)
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="from"></param>
        /// <param name="scope"></param>
        /// <returns>Could connect</returns>
        private void Sync(Side from, Scope scope, DbConnection serverConn, DbConnection clientConn)
        {
            Poster.PostMessage("Begin sync scope '{0}'", scope);

            SyncUtil.Provision(serverConn, scope, null);
            SyncUtil.Provision(clientConn, scope, serverConn);

            var scopeName = scope.ToScopeString();
            var clientProvider = SyncUtil.CreateProvider(clientConn, scopeName);
            var serverProvider = SyncUtil.CreateProvider(serverConn, scopeName);

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

    }



}
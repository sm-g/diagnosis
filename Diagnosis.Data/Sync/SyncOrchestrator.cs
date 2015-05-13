using Diagnosis.Common;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Sync
{
    internal class DownloadSyncOrchestrator : SyncOrchestrator
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
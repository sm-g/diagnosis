using Diagnosis.Data.Sync;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    public abstract class SyncronizedMigration : Migration
    {
        protected string SqlCeProvider = Syncer.SqlCeProvider;
        protected string SqlServerProvider = Syncer.SqlServerProvider;
        protected string Provider;

        public virtual string[] UpTables { get { return new string[] { }; } }

        public virtual string[] DownTables { get { return new string[] { }; } }

        protected void BeforeUp()
        {
            // deprovision first
            Syncer.BeforeMigrate(ConnectionString, Provider, UpTables);
        }

        protected void BeforeDown()
        {
            Syncer.BeforeMigrate(ConnectionString, Provider, DownTables);
        }
    }
}
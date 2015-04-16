using Diagnosis.Data.Sync;
using FluentMigrator;
using System;
using System.Linq;

namespace Diagnosis.Data.Versions
{
    public abstract class SyncronizedMigration : Migration
    {
        protected readonly string Provider;

        public SyncronizedMigration(string provider)
        {
            Provider = provider;
        }

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
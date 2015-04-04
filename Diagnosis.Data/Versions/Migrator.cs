using Diagnosis.Common;
using Diagnosis.Common.Types;
using Diagnosis.Data.Versions.Server;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using System;
using System.IO;

namespace Diagnosis.Data.Versions
{
    public class Migrator
    {
        private ConnectionInfo connectionInfo;
        private string outputDir;
        private Side side;

        public Migrator(ConnectionInfo conInfo, Side side, string fileOutputDir)
        {
            this.connectionInfo = conInfo;
            this.outputDir = fileOutputDir;
            this.side = side;
        }

        public void MigrateToLatest()
        {
            Do((runner) => runner.MigrateUp());
        }

        public void Rollback(int steps = 1)
        {
            Do((runner) => runner.Rollback(steps));
        }

        private void Do(Action<IMigrationRunner> runnerAct)
        {
            var dir = new DirectoryInfo(outputDir);
            if (!dir.Exists)
                dir.Create();

            var filename = Path.Combine(outputDir, string.Format("migrated-{0:yyyy-MM-dd-HH-mm-ss}.sql", DateTime.Now));
            using (var fs = File.CreateText(filename))
            {
                var announcer = new TextWriterAnnouncer(fs);
                var assembly = typeof(CreateClientDb).Assembly;

                var migrationContext = MakeContext(announcer);
                var options = new MigrationOptions { PreviewOnly = false, Timeout = 15 };
                var processor = MakeProcessor(announcer, options);
                var runner = new MigrationRunner(assembly, migrationContext, processor);
                runnerAct(runner);
            }

            if (new FileInfo(filename).Length == 0)
            {
                File.Delete(filename);
            }
        }

        private RunnerContext MakeContext(TextWriterAnnouncer announcer)
        {
            string db;
            string nmsp;
            if (connectionInfo.ProviderName == Constants.SqlCeProvider)
                db = "sqlserverce";
            else
                db = "sqlserver2014";

            // do not run .Off namespace
            if (side == Side.Client)
                nmsp = typeof(CreateClientDb).Namespace;
            else
                nmsp = typeof(CreateServerSdf).Namespace;

            var migrationContext = new RunnerContext(announcer)
            {
                Database = db,
                Namespace = nmsp
            };
            return migrationContext;
        }

        private IMigrationProcessor MakeProcessor(TextWriterAnnouncer announcer, MigrationOptions options)
        {
            if (connectionInfo.ProviderName == Constants.SqlCeProvider)
            {
                var factory = new FluentMigrator.Runner.Processors.SqlServer.SqlServerCeProcessorFactory();
                var processor = factory.Create(connectionInfo.ConnectionString, announcer, options);
                return processor;
            }
            else
            {
                var factory = new FluentMigrator.Runner.Processors.SqlServer.SqlServer2012ProcessorFactory();
                var processor = factory.Create(connectionInfo.ConnectionString, announcer, options);
                return processor;
            }
        }

        class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }

            public string ProviderSwitches { get; set; }

            public int Timeout { get; set; }
        }
    }
}
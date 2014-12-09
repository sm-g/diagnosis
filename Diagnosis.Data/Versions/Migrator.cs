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
        private string connectionString;
        private string outputDir;

        public Migrator(string conStr, string fileOutputDir)
        {
            this.connectionString = conStr;
            this.outputDir = fileOutputDir;
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
            var filename = Path.Combine(outputDir, string.Format("migrated-{0:yyyy-MM-dd-HH-mm-ss}.sql", DateTime.Now));
            using (var fs = File.CreateText(filename))
            {
                var announcer = new TextWriterAnnouncer(fs);
                var assembly = typeof(CreateClientDb).Assembly;

                var migrationContext = new RunnerContext(announcer)
                {
                    Database = "sqlserverce",
                    Namespace = typeof(CreateClientDb).Namespace
                };

                var options = new MigrationOptions { PreviewOnly = false, Timeout = 15 };
                var factory = new FluentMigrator.Runner.Processors.SqlServer.SqlServerCeProcessorFactory();
                var processor = factory.Create(connectionString, announcer, options);
                var runner = new MigrationRunner(assembly, migrationContext, processor);
                runnerAct(runner);
            }

            if (new FileInfo(filename).Length == 0)
            {
                File.Delete(filename);
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
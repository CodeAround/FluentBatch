using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using CodeAround.FluentBatch.Test.Database.Migration;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

namespace CodeAround.FluentBatch.Test.Database
{
    public class DatabaseSandBox : IDisposable
    {
        private static string _dbServer;
        private string _dbName;
        private string _templateConnectionString = "Data Source={0};{1};Integrated Security=True";
        private string _noDbConnectionString => string.Format(_templateConnectionString, _dbServer, $"");
        private string _databasePrefix => $"{Environment.MachineName}";
        public bool KeepDatabaseAfterTest { get; set; }
        public SqlConnection Connection { get; private set; }

        public void Build(string dbServer, string dbName)
        {
            _dbServer = dbServer;
            _dbName = $"{_databasePrefix}-{dbName}-{Guid.NewGuid().ToString()}";
            CreateDatabase();
            Connection = new SqlConnection(_connectionString);
            Connection.Open();
        }

        private string _connectionString => string.Format(_templateConnectionString, _dbServer, $"Initial Catalog={_dbName}");

        private void CreateDatabase()
        {
            using (SqlConnection sqlConnection = new SqlConnection(_noDbConnectionString))
            {
                sqlConnection.Open();
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                string text = current.Name.Substring(current.Name.IndexOf("\\") + 1);
                SqlMapper.Execute((IDbConnection)sqlConnection, $"CREATE DATABASE [{_dbName}] CONTAINMENT = NONE ON PRIMARY (NAME = N'{_dbName}_sys', FILENAME = N'C:\\Users\\{text}\\{_dbName}_sys.mdf', SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB ), FILEGROUP[Tables] DEFAULT (NAME = N'{_dbName}_data', FILENAME = N'C:\\Users\\{text}\\{_dbName}_data.ndf', SIZE = 8192KB, MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB) LOG ON (NAME = N'{_dbName}_log', FILENAME = N'C:\\Users\\{text}\\{_dbName}_log.ldf', SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )", (object)null, (IDbTransaction)null, (int?)null, (CommandType?)null);
                sqlConnection.Close();
            }
        }

        public void Migrate()
        {
            var serviceProvider = CreateServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }
        }

        private void DeleteDatabase()
        {
            using (SqlConnection sqlConnection = new SqlConnection(_noDbConnectionString))
            {
                sqlConnection.Open();
                IEnumerable<string> enumerable = SqlMapper.Query<string>((IDbConnection)sqlConnection, $"select [name] as database_nane from sys.databases where [name] like '{_databasePrefix}%' order by name", (object)null, (IDbTransaction)null, true, (int?)null, (CommandType?)null);
                foreach (string item in enumerable)
                {
                    if (_dbName == item)
                        SqlMapper.Execute((IDbConnection)sqlConnection, $"ALTER DATABASE [{item}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE " + $"DROP DATABASE [{item}]", (object)null, (IDbTransaction)null, (int?)null, (CommandType?)null);
                }
                sqlConnection.Close();
            }
        }

        private IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQLite support to FluentMigrator
                    .AddSqlServer()
                    // Set the connection string
                    .WithGlobalConnectionString(_connectionString)
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(_001_CreatePersonTable).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        private void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }

        public void Dispose()
        {
            Connection?.Dispose();
            if (!KeepDatabaseAfterTest)
            {
                DeleteDatabase();
            }
        }
    }
}

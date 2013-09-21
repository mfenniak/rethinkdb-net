using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    public class TestBase
    {
        protected IConnection connection;
        protected Process rethinkProcess;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            try
            {
                StartRethinkDb();
                DoTestFixtureSetUp().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("TestFixtureSetUp failed: {0}", e);
                throw;
            }
        }

        private async Task DoTestFixtureSetUp()
        {
            connection = ConfigConnectionFactory.Instance.Get("testCluster");
            connection.Logger = new DefaultLogger(LoggingCategory.Warning, Console.Out);

            await connection.ConnectAsync();

            try
            {
                var dbList = await connection.RunAsync(Query.DbList());
                if (dbList.Contains("test"))
                    await connection.RunAsync(Query.DbDrop("test"));
            }
            catch (Exception)
            {
            }
        }

        private string GetRethinkPath()
        {
            // Process.Start does not seem to take path into account, so make a couple guesses at rethink's location
            var locations = new List<string> {
                "/usr/bin/rethinkdb", "/usr/local/bin/rethinkdb"
            };

            foreach (var guess in locations)
                if (File.Exists(guess))
                    return guess;

            return null;
        }

        private void StartRethinkDb()
        {
            var solutionPath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);

            var dbPath = Path.Combine(solutionPath, "tmp/rethink");

            if(Directory.Exists(dbPath))
               Directory.Delete(dbPath, true);

            var processInfo = new ProcessStartInfo()
            {
                FileName = GetRethinkPath(),
                Arguments = "-d " + dbPath + " --driver-port 55558",
                UseShellExecute = false
            };
            rethinkProcess = Process.Start(processInfo);

            // wait for it to start up. Is there a better way to do this?
            Thread.Sleep(1000);
        }

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
            rethinkProcess.Kill();
        }
    }
}


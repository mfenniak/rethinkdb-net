using System;
using System.Linq;
using NUnit.Framework;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using RethinkDb.Configuration;
using System.Net.Sockets;
using System.Net;

namespace RethinkDb.Test
{
    [SetUpFixture]
    public class IntegrationTestSetup
    {
        protected Process rethinkProcess;

        [SetUp]
        public void Setup()
        {
            StartRethinkDb();
        }

        [TearDown]
        public void Teardown()
        {
            rethinkProcess.Kill();
        }

        private string GetRethinkPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var locations = path.Split(Path.PathSeparator).ToList();

            // Something is modifying PATH when running via xamarin studio, making it exclude several things
            // including /usr/local/bin. EnvironmentVariableTarget.User returns null. 
            // So this uglyness has to stay for now?
            locations.Add("/usr/local/bin");

            foreach (var guess in locations.Select(l => Path.Combine(l, "rethinkdb")))
                if (File.Exists(guess))
                    return guess;

            return null;
        }

        private IPEndPoint GetRethinkEndpoint()
        {
            var clientSection = ConfigurationManager.GetSection("rethinkdb") as RethinkDbClientSection;
            foreach (ClusterElement cluster in clientSection.Clusters)
            {
                if (cluster.Name == "testCluster")
                {
                    // assume a single shard for now
                    foreach (EndPointElement ep in cluster.EndPoints)
                        return new IPEndPoint(IPAddress.Parse(ep.Address), ep.Port);
                }
            }

            return null;
        }

        private bool IsEndpointAvailable(IPEndPoint endpoint)
        {
            using (var c = new TcpClient())
            {
                try
                {
                    c.Connect(endpoint);
                } catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private void StartRethinkDb()
        {
            var rethinkPath = GetRethinkPath();
            // if we can't find rethink, assume the user wants to use a remote instance
            if (rethinkPath == null)
                return;

            var rethinkEndpoint = GetRethinkEndpoint();
            // don't try to start if there is already something on the configured endpoint (it might even be rethinkdb!)
            if (rethinkEndpoint == null || IsEndpointAvailable(rethinkEndpoint))
                return;

            var solutionPath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);

            var dbPath = Path.Combine(solutionPath, "rethink");

            if (Directory.Exists(dbPath))
                Directory.Delete(dbPath, true);

            var processInfo = new ProcessStartInfo()
            {
                FileName = GetRethinkPath(),
                Arguments = "-d " + dbPath + " --cluster-port 55557 --driver-port " + rethinkEndpoint.Port,
                UseShellExecute = false
            };
            rethinkProcess = Process.Start(processInfo);

            // wait for it to start up, but not forever
            int waited = 0;
            while (!IsEndpointAvailable(rethinkEndpoint) && waited < 30)
            {
                Thread.Sleep(250);
                waited++;
            }

            if (waited >= 30)
                throw new Exception("Could not start rethinkdb.");
        }
    }
}


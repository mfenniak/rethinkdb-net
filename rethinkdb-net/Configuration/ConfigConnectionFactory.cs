using System;
using System.Configuration;
using System.Net;
using System.Collections.Generic;

namespace RethinkDb.Configuration
{
    public class ConfigConnectionFactory : IConnectionFactory
    {
        public static readonly ConfigConnectionFactory Instance = new ConfigConnectionFactory();

        internal static readonly Lazy<RethinkDbClientSection> DefaultSettings = new Lazy<RethinkDbClientSection>(() => ConfigurationManager.GetSection("rethinkdb") as RethinkDbClientSection);

        private ConfigConnectionFactory()
        {
        }

        public IConnection Get(string clusterName)
        {
            if (DefaultSettings.Value == null)
                throw new ConfigurationErrorsException("No rethinkdb client configuration section located");

            foreach (ClusterElement connection in DefaultSettings.Value.Clusters)
            {
                if (connection.Name == clusterName)
                {
                    List<EndPoint> endpoints = new List<EndPoint>();
                    foreach (EndPointElement ep in connection.EndPoints)
                    {
                        IPAddress ip;
                        if (IPAddress.TryParse(ep.Address, out ip))
                            endpoints.Add(new IPEndPoint(ip, ep.Port));
                        else
                            endpoints.Add(new DnsEndPoint(ep.Address, ep.Port));
                    }

                    return new Connection(endpoints.ToArray());
                }
            }

            throw new ArgumentException("Cluster name could not be found in configuration", "clusterName");
        }
    }
}


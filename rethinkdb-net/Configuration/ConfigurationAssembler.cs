using System;
using System.Configuration;
using System.Net;
using System.Collections.Generic;

namespace RethinkDb.Configuration
{
    public class ConfigurationAssembler
    {
        internal static readonly Lazy<RethinkDbClientSection> DefaultSettings = new Lazy<RethinkDbClientSection>(() => ConfigurationManager.GetSection("rethinkdb") as RethinkDbClientSection);

        public static IConnectionFactory CreateConnectionFactory(string clusterName)
        {
            if (DefaultSettings.Value == null)
                throw new ConfigurationErrorsException("No rethinkdb client configuration section located");

            foreach (ClusterElement cluster in DefaultSettings.Value.Clusters)
            {
                if (cluster.Name == clusterName)
                {
                    List<EndPoint> endpoints = new List<EndPoint>();
                    foreach (EndPointElement ep in cluster.EndPoints)
                    {
                        IPAddress ip;
                        if (IPAddress.TryParse(ep.Address, out ip))
                            endpoints.Add(new IPEndPoint(ip, ep.Port));
                        else
                            endpoints.Add(new DnsEndPoint(ep.Address, ep.Port));
                    }

                    var connectionFactory = new DefaultConnectionFactory(endpoints);
                    if (!String.IsNullOrEmpty(cluster.AuthorizationKey))
                        connectionFactory.AuthorizationKey = cluster.AuthorizationKey;
                    return connectionFactory;
                }
            }

            throw new ArgumentException("Cluster name could not be found in configuration", "clusterName");
        }
    }
}


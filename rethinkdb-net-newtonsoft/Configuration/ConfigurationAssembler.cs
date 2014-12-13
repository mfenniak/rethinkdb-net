using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RethinkDb.Configuration;
using RethinkDb.ConnectionFactories;
using RethinkDb.Logging;
using RethinkDb.Newtonsoft.Converters;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class ConfigurationAssembler
    {
        internal static readonly Lazy<RethinkDbClientSection> DefaultSettings =
            new Lazy<RethinkDbClientSection>(() => ConfigurationManager.GetSection("rethinkdb") as RethinkDbClientSection);

        public static JsonSerializerSettings DefaultJsonSerializerSettings { get; set; }

        static ConfigurationAssembler()
        {
            DefaultJsonSerializerSettings = new JsonSerializerSettings()
                {
                    Converters =
                        {
                            new TimeSpanConverter()
                        },
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
        }

        public static IConnectionFactory CreateConnectionFactory(string clusterName)
        {
            if (DefaultSettings.Value == null)
                throw new ConfigurationErrorsException("No rethinkdb client configuration section located");

            foreach (ClusterElement cluster in DefaultSettings.Value.Clusters)
            {
                if (cluster.Name == clusterName)
                {
                    IConnectionFactory connectionFactory = CreateDefaultConnectionFactory(cluster);

                    if (cluster.NetworkErrorHandling != null && cluster.NetworkErrorHandling.Enabled)
                        connectionFactory = new ReliableConnectionFactory(connectionFactory);

                    if (cluster.ConnectionPool != null && cluster.ConnectionPool.Enabled)
                        connectionFactory = new ConnectionPoolingConnectionFactory(connectionFactory);
                    else if (cluster.ConnectionPool != null && cluster.ConnectionPool.Enabled && cluster.ConnectionPool.QueryTimeout != 0)
                        connectionFactory = new ConnectionPoolingConnectionFactory(connectionFactory,
                                new TimeSpan(0, 0, cluster.ConnectionPool.QueryTimeout));

                    return connectionFactory;
                }
            }

            throw new ArgumentException("Cluster name could not be found in configuration", "clusterName");
        }

        private static IConnectionFactory CreateDefaultConnectionFactory(ClusterElement cluster)
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

            var connectionFactory = new NewtonsoftConnectionFactory(endpoints);
            
            if (!String.IsNullOrEmpty(cluster.AuthorizationKey))
                connectionFactory.AuthorizationKey = cluster.AuthorizationKey;

            if (cluster.DefaultLogger != null && cluster.DefaultLogger.Enabled)
                connectionFactory.Logger = new DefaultLogger(cluster.DefaultLogger.Category, Console.Out);

            return connectionFactory;
        }
    }
}
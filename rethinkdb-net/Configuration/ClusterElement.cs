using System.Configuration;

namespace RethinkDb.Configuration
{
    public class ClusterElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("authorizationKey")]
        public string AuthorizationKey
        {
            get
            {
                return this["authorizationKey"] as string;
            }
            set
            {
                this["authorizationKey"] = value;
            }
        }

        [ConfigurationProperty("endpoints")]
        public EndPointElementCollection EndPoints
        {
            get
            {
                return this["endpoints"] as EndPointElementCollection;
            }
        }

        [ConfigurationProperty("connectionPool")]
        public ConnectionPoolElement ConnectionPool
        {
            get
            {
                return this["connectionPool"] as ConnectionPoolElement;
            }
        }

        [ConfigurationProperty("networkErrorHandling")]
        public NetworkErrorHandlingElement NetworkErrorHandling
        {
            get
            {
                return this["networkErrorHandling"] as NetworkErrorHandlingElement;
            }
        }

        [ConfigurationProperty("defaultLogger")]
        public DefaultLoggerElement DefaultLogger
        {
            get
            {
                return this["defaultLogger"] as DefaultLoggerElement;
            }
        }
    }
}

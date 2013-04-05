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

        [ConfigurationProperty("endpoints")]
        public EndPointElementCollection EndPoints
        {
            get
            {
                return this["endpoints"] as EndPointElementCollection;
            }
        }
    }
}

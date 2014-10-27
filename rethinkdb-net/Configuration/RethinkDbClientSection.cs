using System.Configuration;

namespace RethinkDb.Configuration
{
    public class RethinkDbClientSection : ConfigurationSection
    {
        [ConfigurationProperty("clusters", IsRequired = true)]
        public ClusterElementCollection Clusters
        {
            get { return (ClusterElementCollection)base["clusters"]; }
        }
    }
}

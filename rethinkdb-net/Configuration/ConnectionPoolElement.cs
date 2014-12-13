using System.Configuration;

namespace RethinkDb.Configuration
{
    public class ConnectionPoolElement : ConfigurationElement
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
            }
        }

        [ConfigurationProperty("queryTimeout", IsRequired = false)]
        public int QueryTimeout
        {
            get
            {
                var returnValue = 0;
                int.TryParse(this["queryTimeout"].ToString(), out returnValue);
                return returnValue;
            }
            set
            {
                this["queryTimeout"] = value;
            }
        }
    }
}

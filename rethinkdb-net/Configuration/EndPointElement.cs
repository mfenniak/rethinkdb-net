using System.Configuration;

namespace RethinkDb.Configuration
{
    public class EndPointElement : ConfigurationElement
    {
        [ConfigurationProperty("address", IsRequired = true, IsKey = true)]
        public string Address
        {
            get
            {
                return this["address"] as string;
            }
            set
            {
                this["address"] = value;
            }
        }

        [ConfigurationProperty("port", IsRequired = true, IsKey = true), IntegerValidator(MinValue = 0, MaxValue = 65535)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }
    }
}

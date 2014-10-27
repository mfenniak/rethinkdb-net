using System.Configuration;
using RethinkDb.Logging;

namespace RethinkDb.Configuration
{
    public class DefaultLoggerElement : ConfigurationElement
    {
        [ConfigurationProperty("enabled")]
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

        [ConfigurationProperty("category", DefaultValue = LoggingCategory.Warning)]
        public LoggingCategory Category
        {
            get
            {
                return (LoggingCategory)this["category"];
            }
            set
            {
                this["category"] = value;
            }
        }
    }
}

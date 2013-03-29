using System;
using System.IO;

namespace RethinkDb
{
    public class DefaultLogger : ILogger
    {
        private LoggingCategory minimumCategory;
        private StringWriter output;

        public DefaultLogger(LoggingCategory minimumCategory, StringWriter output)
        {
            this.minimumCategory = minimumCategory;
            this.output = output;
        }

        public bool IsLoggingEnabled(LoggingCategory category)
        {
            return category >= minimumCategory;
        }

        public void Log (LoggingCategory category, string text)
        {
            if (IsLoggingEnabled(category))
                output.WriteLine(text);
        }
    }
}


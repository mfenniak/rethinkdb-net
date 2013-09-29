using System;
using System.IO;

namespace RethinkDb.Logging
{
    public class DefaultLogger : ILogger
    {
        private LoggingCategory minimumCategory;
        private TextWriter output;

        public DefaultLogger(LoggingCategory minimumCategory, TextWriter output)
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


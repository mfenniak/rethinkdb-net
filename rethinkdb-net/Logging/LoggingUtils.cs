using System;
using System.Collections.Generic;
using System.Linq;

namespace RethinkDb.Logging
{
    static class LoggingUtils
    {
        private static bool SafeIsLoggingEnabled(ILogger logger, LoggingCategory category)
        {
            try
            {
                return logger != null && logger.IsLoggingEnabled(category);
            }
            catch (Exception)
            {
                // Nothing to do here; normally it'd be nice to log an exception, but not if our logger is misbehavin'.
                return false;
            }
        }

        private static void SafeLog(ILogger logger, LoggingCategory category, string format, params object[] formatArgs)
        {
            if (logger == null || !SafeIsLoggingEnabled(logger, category))
                return;

            try
            {
                logger.Log(category, String.Format(format, formatArgs));
            }
            catch (Exception)
            {
                // Nothing to do here; normally it'd be nice to log an exception, but not if our logger is misbehavin'.
            }
        }

        public static bool DebugEnabled(this ILogger logger)
        {
            return SafeIsLoggingEnabled(logger, LoggingCategory.Debug);
        }

        public static bool InformationEnabled(this ILogger logger)
        {
            return SafeIsLoggingEnabled(logger, LoggingCategory.Information);
        }

        public static bool WarningEnabled(this ILogger logger)
        {
            return SafeIsLoggingEnabled(logger, LoggingCategory.Warning);
        }

        public static bool ErrorEnabled(this ILogger logger)
        {
            return SafeIsLoggingEnabled(logger, LoggingCategory.Error);
        }

        public static bool FatalEnabled(this ILogger logger)
        {
            return SafeIsLoggingEnabled(logger, LoggingCategory.Fatal);
        }

        public static void Debug(this ILogger logger, string format, params object[] formatArgs)
        {
            SafeLog(logger, LoggingCategory.Debug, format, formatArgs);
        }

        public static void Information(this ILogger logger, string format, params object[] formatArgs)
        {
            SafeLog(logger, LoggingCategory.Information, format, formatArgs);
        }

        public static void Warning(this ILogger logger, string format, params object[] formatArgs)
        {
            SafeLog(logger, LoggingCategory.Warning, format, formatArgs);
        }

        public static void Error(this ILogger logger, string format, params object[] formatArgs)
        {
            SafeLog(logger, LoggingCategory.Error, format, formatArgs);
        }

        public static void Fatal(this ILogger logger, string format, params object[] formatArgs)
        {
            SafeLog(logger, LoggingCategory.Fatal, format, formatArgs);
        }

        public static string EnumerableToString<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Aggregate("", (s, m) => (s + ", " + m), res => res.Length > 0 ? res.Substring(2) : res);
        }
    }
}


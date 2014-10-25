using System;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class DateTimeExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            RegisterDateTimeAddMethods(expressionConverterFactory);
            RegisterTimeSpanConstructors(expressionConverterFactory);
        }

        public static void RegisterDateTimeAddMethods(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<DateTime, TimeSpan, DateTime>(
                (dt, timespan) => dt.Add(timespan),
                (dt, timespan) => Add(dt, timespan));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, TimeSpan, DateTimeOffset>(
                (dt, timespan) => dt.Add(timespan),
                (dt, timespan) => Add(dt, timespan));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, double, DateTime>(
                (dt, minutes) => dt.AddMinutes(minutes),
                (dt, minutes) => Add(dt, MinutesToSeconds(minutes)));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, double, DateTimeOffset>(
                (dt, minutes) => dt.AddMinutes(minutes),
                (dt, minutes) => Add(dt, MinutesToSeconds(minutes)));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, double, DateTime>(
                (dt, seconds) => dt.AddSeconds(seconds),
                (dt, seconds) => Add(dt, seconds));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, double, DateTimeOffset>(
                (dt, seconds) => dt.AddSeconds(seconds),
                (dt, seconds) => Add(dt, seconds));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, double, DateTime>(
                (dt, hours) => dt.AddHours(hours),
                (dt, hours) => Add(dt, HoursToSeconds(hours)));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, double, DateTimeOffset>(
                (dt, hours) => dt.AddHours(hours),
                (dt, hours) => Add(dt, HoursToSeconds(hours)));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, double, DateTime>(
                (dt, ms) => dt.AddMilliseconds(ms),
                (dt, ms) => Add(dt, MillisecondsToSeconds(ms)));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, double, DateTimeOffset>(
                (dt, ms) => dt.AddMilliseconds(ms),
                (dt, ms) => Add(dt, MillisecondsToSeconds(ms)));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, long, DateTime>(
                (dt, ticks) => dt.AddTicks(ticks),
                (dt, ticks) => Add(dt, TicksToSeconds(ticks)));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, long, DateTimeOffset>(
                (dt, ticks) => dt.AddTicks(ticks),
                (dt, ticks) => Add(dt, TicksToSeconds(ticks)));

            expressionConverterFactory.RegisterTemplateMapping<DateTime, double, DateTime>(
                (dt, days) => dt.AddDays(days),
                (dt, days) => Add(dt, DaysToSeconds(days)));
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, double, DateTimeOffset>(
                (dt, days) => dt.AddDays(days),
                (dt, days) => Add(dt, DaysToSeconds(days)));
        }

        public static void RegisterTimeSpanConstructors(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<long, TimeSpan>(
                (ticks) => new TimeSpan(ticks),
                (ticks) => TicksToSeconds(ticks)
            );
            expressionConverterFactory.RegisterTemplateMapping<int, int, int, TimeSpan>(
                (hours, minutes, seconds) => new TimeSpan(hours, minutes, seconds),
                (hours, minutes, seconds) =>
                    Add(
                        HoursToSeconds(hours),
                        MinutesToSeconds(minutes),
                        seconds
                    )
                );
            expressionConverterFactory.RegisterTemplateMapping<int, int, int, int, TimeSpan>(
                (days, hours, minutes, seconds) => new TimeSpan(days, hours, minutes, seconds),
                (days, hours, minutes, seconds) =>
                    Add(
                        DaysToSeconds(days),
                        HoursToSeconds(hours),
                        MinutesToSeconds(minutes),
                        seconds
                    )
                );
            expressionConverterFactory.RegisterTemplateMapping<int, int, int, int, int, TimeSpan>(
                (days, hours, minutes, seconds, milliseconds) => new TimeSpan(days, hours, minutes, seconds, milliseconds),
                (days, hours, minutes, seconds, milliseconds) =>
                    Add(
                        DaysToSeconds(days),
                        HoursToSeconds(hours),
                        MinutesToSeconds(minutes),
                        seconds,
                        MillisecondsToSeconds(milliseconds)
                    )
                );

            expressionConverterFactory.RegisterTemplateMapping<double, TimeSpan>(
                (days) => TimeSpan.FromDays(days),
                DaysToSeconds
            );
            expressionConverterFactory.RegisterTemplateMapping<double, TimeSpan>(
                (hours) => TimeSpan.FromHours(hours),
                HoursToSeconds
            );
            expressionConverterFactory.RegisterTemplateMapping<double, TimeSpan>(
                (milliseconds) => TimeSpan.FromMilliseconds(milliseconds),
                MillisecondsToSeconds
            );
            expressionConverterFactory.RegisterTemplateMapping<double, TimeSpan>(
                (minutes) => TimeSpan.FromMinutes(minutes),
                MinutesToSeconds
            );
            expressionConverterFactory.RegisterTemplateMapping<double, TimeSpan>(
                (seconds) => TimeSpan.FromSeconds(seconds),
                term => term
            );
            expressionConverterFactory.RegisterTemplateMapping<long, TimeSpan>(
                (ticks) => TimeSpan.FromTicks(ticks),
                TicksToSeconds
            );
        }

        private static Term Binary(Term leftTerm, Term.TermType type, double rightTerm)
        {
            return new Term()
            {
                type = type,
                args = {
                    leftTerm,
                    new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_NUM,
                            r_num = rightTerm
                        }
                    }
                }
            };
        }

        private static Term DaysToSeconds(Term term)
        {
            // seconds per day... doesn't account for days with leap seconds, but, close enough
            return Binary(term, Term.TermType.MUL, 86400);
        }

        private static Term HoursToSeconds(Term term)
        {
            // seconds per hour
            return Binary(term, Term.TermType.MUL, 3600);
        }

        private static Term MinutesToSeconds(Term term)
        {
            return Binary(term, Term.TermType.MUL, 60);
        }

        private static Term MillisecondsToSeconds(Term term)
        {
            return Binary(term, Term.TermType.DIV, 1000);
        }

        private static Term TicksToSeconds(Term term)
        {
            return Binary(term, Term.TermType.DIV, TimeSpan.TicksPerSecond);
        }

        private static Term Add(params Term[] terms)
        {
            if (terms.Length == 0)
                throw new InvalidOperationException("can't add nothing");
            else
            {
                return terms.Aggregate((acc, t) =>
                    new Term()
                    {
                        type = Term.TermType.ADD,
                        args = {
                            acc,
                            t
                        }
                    }
                );
            }
        }
    }
}

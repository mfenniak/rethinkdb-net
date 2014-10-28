using System;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class DateTimeExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            RegisterDateTimeConstructors(expressionConverterFactory);
            RegisterDateTimeAddMethods(expressionConverterFactory);
            RegisterDateTimeAccessors(expressionConverterFactory);
            RegisterTimeSpanConstructors(expressionConverterFactory);
        }

        public static void RegisterDateTimeConstructors(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<DateTime>(
                () => DateTime.UtcNow,
                () => new Term() { type = Term.TermType.NOW });
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset>(
                () => DateTimeOffset.UtcNow,
                () => new Term() { type = Term.TermType.NOW });

            expressionConverterFactory.RegisterTemplateMapping<int, int, int, DateTime>(
                (year, month, day) => new DateTime(year, month, day),
                (year, month, day) => new Term() { type = Term.TermType.TIME, args = { year, month, day, String("Z") } });
            expressionConverterFactory.RegisterTemplateMapping<int, int, int, int, int, int, DateTime>(
                (year, month, day, hour, minute, second) => new DateTime(year, month, day, hour, minute, second),
                (year, month, day, hour, minute, second) => new Term() {
                    type = Term.TermType.TIME,
                    args = { year, month, day, hour, minute, second, String("Z") } });
            expressionConverterFactory.RegisterTemplateMapping<int, int, int, int, int, int, int, DateTime>(
                (year, month, day, hour, minute, second, millisecond) => new DateTime(year, month, day, hour, minute, second, millisecond),
                (year, month, day, hour, minute, second, millisecond) => new Term() {
                    type = Term.TermType.TIME,
                    args = { year, month, day, hour, minute, Add(second, Binary(millisecond, Term.TermType.DIV, 1000)), String("Z") }
                });

            // new DateTimeOffset in .NET creates a DateTimeOffset with either local time offset, or UTC offset,
            // depending upon the DateTime's Kind property.  We can't really support that because we're not actually
            // working with DateTime objects on the RethinkDB side; we're working with ReQL times that already have
            // offsets associated with them.  So... this is basically a NOOP.
            expressionConverterFactory.RegisterTemplateMapping<DateTime, DateTimeOffset>(
                (dt) => new DateTimeOffset(dt),
                (dt) => dt);

            // But creating a DateTimeOffset with a specific timezone isn't a NOOP.  This is re-interpreting the given
            // time at the given timezone (not converting it).
            expressionConverterFactory.RegisterTemplateMapping<DateTime, TimeSpan, DateTimeOffset>(
                (dt, offset) => new DateTimeOffset(dt, offset),
                (dt, offset) => new Term() {
                    type = Term.TermType.TIME,
                    // Well, this is awkward, but... break the time up and construct it again with the specified
                    // TZ offset.
                    args = {
                        new Term() { type = Term.TermType.YEAR, args = { dt } },
                        new Term() { type = Term.TermType.MONTH, args = { dt } },
                        new Term() { type = Term.TermType.DAY, args = { dt } },
                        new Term() { type = Term.TermType.HOURS, args = { dt } },
                        new Term() { type = Term.TermType.MINUTES, args = { dt } },
                        new Term() { type = Term.TermType.SECONDS, args = { dt } },
                        TimeSpanToOffset(offset),
                    }
                });
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

        public static void RegisterDateTimeAccessors(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Year,
                dt => new Term() { type = Term.TermType.YEAR, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Year,
                dt => new Term() { type = Term.TermType.YEAR, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Month,
                dt => new Term() { type = Term.TermType.MONTH, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Month,
                dt => new Term() { type = Term.TermType.MONTH, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Day,
                dt => new Term() { type = Term.TermType.DAY, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Day,
                dt => new Term() { type = Term.TermType.DAY, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Hour,
                dt => new Term() { type = Term.TermType.HOURS, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Hour,
                dt => new Term() { type = Term.TermType.HOURS, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Minute,
                dt => new Term() { type = Term.TermType.MINUTES, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Minute,
                dt => new Term() { type = Term.TermType.MINUTES, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.Second,
                dt => new Term() { type = Term.TermType.SECONDS, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.Second,
                dt => new Term() { type = Term.TermType.SECONDS, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, DayOfWeek>(
                dt => dt.DayOfWeek,
                dt => new Term() { type = Term.TermType.DAY_OF_WEEK, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, DayOfWeek>(
                dt => dt.DayOfWeek,
                dt => new Term() { type = Term.TermType.DAY_OF_WEEK, args = { dt } }
            );

            expressionConverterFactory.RegisterTemplateMapping<DateTime, int>(
                dt => dt.DayOfYear,
                dt => new Term() { type = Term.TermType.DAY_OF_YEAR, args = { dt } }
            );
            expressionConverterFactory.RegisterTemplateMapping<DateTimeOffset, int>(
                dt => dt.DayOfYear,
                dt => new Term() { type = Term.TermType.DAY_OF_YEAR, args = { dt } }
            );
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

        private static Term String(string str)
        {
            return new Term()
            {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = str,
                }
            };
        }

        private static Term CoerceTo(Term term, string type)
        {
            return new Term()
            {
                type = Term.TermType.COERCE_TO,
                args = {
                    term,
                    String(type)
                }
            };
        }

        private static Term Branch(Term test, Term ifTrue, Term ifFalse)
        {
            return new Term()
            {
                type = Term.TermType.BRANCH,
                args = {
                    test,
                    ifTrue,
                    ifFalse
                }
            };
        }

        private static Term Binary(Term leftTerm, Term.TermType type, Term rightTerm)
        {
            return new Term()
            {
                type = type,
                args = {
                    leftTerm,
                    rightTerm
                }
            };
        }

        private static Term Binary(Term leftTerm, Term.TermType type, double rightTerm)
        {
            return Binary(leftTerm, type, new Term()
            {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = rightTerm
                }
            });
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

        public static Term TimeSpanToOffset(Term offset)
        {
            // offset will be a number of seconds on the server-side since that's how we've mapped
            // TimeSpans to ReQL.  This is basically:  str(floor(offset / 60)) + ":" + (offset % 60),
            // but, ReQL doesn't support floor, and requires the timezone to be in the exact format
            // [+-][0-9]{2}:[0-9]{2}.
            return Add(
                TimeSpanToPlusMinus(offset),
                TimeSpanToOffsetHouts(offset),
                String(":"),
                TimeSpanToOffsetMinutes(offset)
            );
        }

        private static Term TimeSpanToPlusMinus(Term offset)
        {
            return new Term()
            {
                type = Term.TermType.BRANCH,
                args = {
                    Binary(offset, Term.TermType.LT, 0),
                    String("-"),
                    String("+")
                }
            };
        }

        private static Term TimeSpanToOffsetHouts(Term offset)
        {
            // (offset - (offset % 3600)) / 3600
            var hours = Binary(
                Binary(offset, Term.TermType.SUB, Binary(offset, Term.TermType.MOD, 3600)),
                Term.TermType.DIV,
                3600);
            // then ensure hours is positive; TimeSpanToPlusMinus takes care of the minus sign if needed
            // to make the padding simpler here
            hours = Branch(
                Binary(hours, Term.TermType.LT, 0),
                Binary(hours, Term.TermType.MUL, -1),
                hours);
            return
                Branch(
                    Binary(hours, Term.TermType.LT, 10),
                    Add(String("0"), CoerceTo(hours, "string")),
                    CoerceTo(hours, "string")
                );
        }

        private static Term TimeSpanToOffsetMinutes(Term offset)
        {
            // minutes is now the total number of minutes, any second precision has been removed
            var minutes = Binary(
                Binary(offset, Term.TermType.SUB, Binary(offset, Term.TermType.MOD, 60)),
                Term.TermType.DIV,
                60);
            // then take out the hours accounted for in TimeSpanToOffsetHours
            minutes = Binary(minutes, Term.TermType.MOD, 60);
            // then ensure minutes is positive; ReSQL -30 % 60 -> -30
            minutes = Branch(
                Binary(minutes, Term.TermType.LT, 0),
                Binary(minutes, Term.TermType.MUL, -1),
                minutes);
            return
                Branch(
                    Binary(minutes, Term.TermType.LE, 10),
                    Add(String("0"), CoerceTo(minutes, "string")), // leading zero
                    CoerceTo(minutes, "string")
                );
        }
    }
}

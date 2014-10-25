using System;
using System.Linq;
using System.Linq.Expressions;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class DateTimeExpressionConverters
    {
        private delegate DateTime AddTimeSpanDelegate(TimeSpan value);
        private delegate DateTime AddDoubleDelegate(double value);
        private delegate DateTime AddLongDelegate(long value);
        private delegate DateTimeOffset AddTimeSpanOffsetDelegate(TimeSpan value);
        private delegate DateTimeOffset AddDoubleOffsetDelegate(double value);
        private delegate DateTimeOffset AddLongOffsetDelegate(long value);
        private delegate TimeSpan FromLongDelegate(long value);
        private delegate TimeSpan FromDoubleDelegate(double value);

        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            DateTime dt;
            DateTimeOffset dto;

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddTimeSpanDelegate)(dt.Add)).Method,
                ConvertAddTimeSpanToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddTimeSpanOffsetDelegate)(dto.Add)).Method,
                ConvertAddTimeSpanToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleDelegate)(dt.AddMinutes)).Method,
                ConvertAddMinutesToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleOffsetDelegate)(dto.AddMinutes)).Method,
                ConvertAddMinutesToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleDelegate)(dt.AddSeconds)).Method,
                ConvertAddSecondsToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleOffsetDelegate)(dto.AddSeconds)).Method,
                ConvertAddSecondsToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleDelegate)(dt.AddHours)).Method,
                ConvertAddHoursToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleOffsetDelegate)(dto.AddHours)).Method,
                ConvertAddHoursToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleDelegate)(dt.AddMilliseconds)).Method,
                ConvertAddMillisecondsToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleOffsetDelegate)(dto.AddMilliseconds)).Method,
                ConvertAddMillisecondsToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddLongDelegate)(dt.AddTicks)).Method,
                ConvertAddTicksSpanToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddLongOffsetDelegate)(dto.AddTicks)).Method,
                ConvertAddTicksSpanToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleDelegate)(dt.AddDays)).Method,
                ConvertAddDaysToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((AddDoubleOffsetDelegate)(dto.AddDays)).Method,
                ConvertAddDaysToTerm);

            expressionConverterFactory.RegisterNewExpressionMapping(
                typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) }),
                ConvertTimeSpanTicksConstructorToTerm);
            expressionConverterFactory.RegisterNewExpressionMapping(
                typeof(TimeSpan).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int) }),
                ConvertTimeSpanHoursMinutesSecondsConstructorToTerm);
            expressionConverterFactory.RegisterNewExpressionMapping(
                typeof(TimeSpan).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                ConvertTimeSpanDaysHoursMinutesSecondsConstructorToTerm);
            expressionConverterFactory.RegisterNewExpressionMapping(
                typeof(TimeSpan).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) }),
                ConvertTimeSpanDaysHoursMinutesSecondsMillisecondsConstructorToTerm);

            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromDoubleDelegate)(TimeSpan.FromDays)).Method,
                ConvertFromDaysToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromDoubleDelegate)(TimeSpan.FromHours)).Method,
                ConvertFromHoursToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromDoubleDelegate)(TimeSpan.FromMilliseconds)).Method,
                ConvertFromMillisecondsToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromDoubleDelegate)(TimeSpan.FromMinutes)).Method,
                ConvertFromMinutesToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromDoubleDelegate)(TimeSpan.FromSeconds)).Method,
                ConvertFromSecondsToTerm);
            expressionConverterFactory.RegisterMethodCallMapping(
                ((FromLongDelegate)(TimeSpan.FromTicks)).Method,
                ConvertFromTicksToTerm);
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

        public static Term ConvertAddTimeSpanToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return Add(recursiveMap(methodCall.Object), recursiveMap(methodCall.Arguments[0]));
        }

        public static Term ConvertDateTimeAddFunctionToTerm(MethodCallExpression callExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, double conversionToSeconds)
        {
            return Add(recursiveMap(callExpression.Object), Binary(recursiveMap(callExpression.Arguments[0]), Term.TermType.MUL, conversionToSeconds));
        }

        public static Term ConvertAddMinutesToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
        }

        public static Term ConvertAddHoursToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
        }

        public static Term ConvertAddMillisecondsToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, (double)TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
        }

        public static Term ConvertAddSecondsToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, 1);
        }

        public static Term ConvertAddTicksSpanToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, 1.0 / TimeSpan.TicksPerSecond);
        }

        public static Term ConvertAddDaysToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return ConvertDateTimeAddFunctionToTerm(methodCall, recursiveMap, TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
        }

        public static Term ConvertTimeSpanTicksConstructorToTerm(NewExpression newExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return TicksToSeconds(recursiveMap(newExpression.Arguments[0]));
        }

        public static Term ConvertTimeSpanHoursMinutesSecondsConstructorToTerm(NewExpression newExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return Add(
                HoursToSeconds(recursiveMap(newExpression.Arguments[0])),
                MinutesToSeconds(recursiveMap(newExpression.Arguments[1])),
                recursiveMap(newExpression.Arguments[2])
            );
        }

        public static Term ConvertTimeSpanDaysHoursMinutesSecondsConstructorToTerm(NewExpression newExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return Add(
                DaysToSeconds(recursiveMap(newExpression.Arguments[0])),
                HoursToSeconds(recursiveMap(newExpression.Arguments[1])),
                MinutesToSeconds(recursiveMap(newExpression.Arguments[2])),
                recursiveMap(newExpression.Arguments[3])
            );
        }

        public static Term ConvertTimeSpanDaysHoursMinutesSecondsMillisecondsConstructorToTerm(NewExpression newExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return Add(
                DaysToSeconds(recursiveMap(newExpression.Arguments[0])),
                HoursToSeconds(recursiveMap(newExpression.Arguments[1])),
                MinutesToSeconds(recursiveMap(newExpression.Arguments[2])),
                recursiveMap(newExpression.Arguments[3]),
                MillisecondsToSeconds(recursiveMap(newExpression.Arguments[4]))
            );
        }

        public static Term ConvertFromDaysToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return DaysToSeconds(recursiveMap(methodCall.Arguments[0]));
        }

        public static Term ConvertFromHoursToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return HoursToSeconds(recursiveMap(methodCall.Arguments[0]));
        }

        public static Term ConvertFromMinutesToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return MinutesToSeconds(recursiveMap(methodCall.Arguments[0]));
        }

        public static Term ConvertFromMillisecondsToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return MillisecondsToSeconds(recursiveMap(methodCall.Arguments[0]));
        }

        public static Term ConvertFromSecondsToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return recursiveMap(methodCall.Arguments[0]);
        }

        public static Term ConvertFromTicksToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return TicksToSeconds(recursiveMap(methodCall.Arguments[0]));
        }
    }
}

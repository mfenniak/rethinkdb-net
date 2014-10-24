using System;
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
        }

        public static Term ConvertAddTimeSpanToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var addTerm = new Term()
            {
                type = Term.TermType.ADD,
            };
            addTerm.args.Add(recursiveMap(methodCall.Object));
            addTerm.args.Add(recursiveMap(methodCall.Arguments[0]));
            return addTerm;
        }

        private static Term ConvertDateTimeAddFunctionToTerm(MethodCallExpression callExpression, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, double conversionToSeconds)
        {
            return new Term() {
                type = Term.TermType.ADD,
                args = {
                    recursiveMap(callExpression.Object),
                    new Term() {
                        type = Term.TermType.MUL,
                        args = {
                            recursiveMap(callExpression.Arguments[0]),
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = conversionToSeconds
                                }
                            }
                        }
                    }
                }
            };
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
    }
}

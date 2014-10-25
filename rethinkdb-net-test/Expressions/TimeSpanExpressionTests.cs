using System;
using NUnit.Framework;
using RethinkDb.QueryTerm;
using FluentAssertions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.Expressions
{
    [TestFixture]
    public class TimeSpanExpressionTests
    {
        IDatumConverterFactory datumConverterFactory;
        IExpressionConverterFactory expressionConverterFactory;
        IQueryConverter queryConverter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                TimeSpanDatumConverterFactory.Instance,
                DateTimeDatumConverterFactory.Instance
            );
            expressionConverterFactory = new RethinkDb.Expressions.DefaultExpressionConverterFactory();
            queryConverter = new QueryConverter(datumConverterFactory, expressionConverterFactory);
        }

        [Test]
        public void TimeSpanTicksConstructor()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<long, TimeSpan>(queryConverter, i => new TimeSpan(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<long, long>(queryConverter, i => i / TimeSpan.TicksPerSecond));
        }

        [Test]
        public void TimeSpanHoursMinutesSecondsConstructor()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<int, TimeSpan>(queryConverter, i => new TimeSpan(i, i, i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 3600) + (i * 60) + i));
        }

        [Test]
        public void TimeSpanDaysHoursMinutesSecondsConstructor()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<int, TimeSpan>(queryConverter, i => new TimeSpan(i, i, i, i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 86400) + (i * 3600) + (i * 60) + i));
        }

        [Test]
        public void TimeSpanDaysHoursMinutesSecondsMillisecondsConstructor()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<int, TimeSpan>(queryConverter, i => new TimeSpan(i, i, i, i, i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 86400) + (i * 3600) + (i * 60) + i + (i / 1000)));
        }

        [Test]
        public void TimeSpanFromDays()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<double, TimeSpan>(queryConverter, i => TimeSpan.FromDays(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 86400)));
        }

        [Test]
        public void TimeSpanFromHours()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<double, TimeSpan>(queryConverter, i => TimeSpan.FromHours(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 3600)));
        }

        [Test]
        public void TimeSpanFromMilliseconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<double, TimeSpan>(queryConverter, i => TimeSpan.FromMilliseconds(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i / 1000)));
        }

        [Test]
        public void TimeSpanFromMinutes()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<double, TimeSpan>(queryConverter, i => TimeSpan.FromMinutes(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => (i * 60)));
        }

        [Test]
        public void TimeSpanFromSeconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<double, TimeSpan>(queryConverter, i => TimeSpan.FromSeconds(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<int, int>(queryConverter, i => i));
        }

        [Test]
        public void TimeSpanFromTicks()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<long, TimeSpan>(queryConverter, i => TimeSpan.FromTicks(i));
            expr.ShouldBeEquivalentTo(
                ExpressionUtils.CreateFunctionTerm<long, long>(queryConverter, i => (i / TimeSpan.TicksPerSecond)));
        }
    }
}

using System;
using NUnit.Framework;
using RethinkDb.QueryTerm;
using FluentAssertions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.Expressions
{
    [TestFixture]
    public class GuidExpressionTests
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
        public void NewGuid()
        {
            var expr = ExpressionUtils.CreateValueTerm<Guid>(queryConverter, () => Guid.NewGuid());
            expr.ShouldBeEquivalentTo(
                new Term() {
                    type = Term.TermType.UUID,
                }
            );
        }
    }
}


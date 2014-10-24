using System;
using NUnit.Framework;
using System.Linq.Expressions;
using RethinkDb.Expressions;
using System.Net;
using RethinkDb.Spec;
using NSubstitute;

namespace RethinkDb.Test
{
    [TestFixture]
    public class DefaultExpressionConverterFactoryTests
    {
        [Test]
        public void CustomBinaryExpressionConverterTest()
        {
            var datumConverterFactory = Substitute.For<IDatumConverterFactory>();
            var expresionConverterFactory = new DefaultExpressionConverterFactory();
            expresionConverterFactory.RegisterBinaryExpressionMapping<DateTime, DateTime>(ExpressionType.Subtract, CustomBinaryExpressionConverter);

            var expressionConverter = expresionConverterFactory.CreateExpressionConverter<TimeSpan>(datumConverterFactory);
            var term = expressionConverter.CreateFunctionTerm(() => DateTime.Now - DateTime.Now);
            Assert.That(term, Is.Not.Null);
            Assert.That(term.type, Is.EqualTo(Term.TermType.DATUM)); // not SUB; our custom converter was involved
        }

        private Term CustomBinaryExpressionConverter(
            BinaryExpression expr, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
            IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return new Term()
            {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Woot woot!",
                }
            };
        }

        [Test]
        public void CustomUnaryExpressionConverterTest()
        {
            var datumConverterFactory = Substitute.For<IDatumConverterFactory>();
            var expresionConverterFactory = new DefaultExpressionConverterFactory();
            expresionConverterFactory.RegisterUnaryExpressionMapping<bool>(ExpressionType.Not, CustomUnaryExpressionConverter);

            bool value = new Random().NextDouble() > 0.5;
            var expressionConverter = expresionConverterFactory.CreateExpressionConverter<bool>(datumConverterFactory);
            var term = expressionConverter.CreateFunctionTerm(() => !value);
            Assert.That(term, Is.Not.Null);
            Assert.That(term.type, Is.EqualTo(Term.TermType.DATUM)); // not NOT; our custom converter was involved
        }

        private Term CustomUnaryExpressionConverter(
            UnaryExpression expr, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
            IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            return new Term()
            {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Woot woot!",
                }
            };
        }
    }
}


using System;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using RethinkDb.Spec;
using RethinkDb.QueryTerm;

namespace RethinkDb.Test.QueryTests
{
    [TestFixture]
    public class ReplaceQueryTests
    {
        private IDatumConverterFactory datumConverterFactory;
        private IExpressionConverterFactory expressionConverterFactory = new RethinkDb.Expressions.DefaultExpressionConverterFactory();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = Substitute.For<IDatumConverterFactory>();

            var stringDatum = new Datum() {
                type = Datum.DatumType.R_STR,
                r_str = "Jackpot!",
            };

            var stringDatumConverter = Substitute.For<IDatumConverter<string>>();
            stringDatumConverter
                .ConvertObject("Jackpot!")
                .Returns(stringDatum);
            stringDatumConverter
                .ConvertDatum(Arg.Is<Datum>(d => d.type == stringDatum.type && d.r_str == stringDatum.r_str))
                .Returns("Jackpot!");

            IDatumConverter<string> value;
            datumConverterFactory
                .TryGet<string>(datumConverterFactory, out value)
                .Returns(args => {
                        args[1] = stringDatumConverter;
                        return true;
                    });
        }

        [Test]
        public void NonAtomicTrue()
        {
            var sequenceQuery = Substitute.For<IMutableSingleObjectQuery<string>>();

            var query = new ReplaceQuery<string>(
                sequenceQuery,
                "Jackpot!",
                true);

            var term = query.GenerateTerm(datumConverterFactory, expressionConverterFactory);

            var nonAtomicArgs = term.optargs.Where(kv => kv.key == "non_atomic");
            Assert.That(nonAtomicArgs.Count(), Is.EqualTo(1));

            var nonAtomicArg = nonAtomicArgs.Single();

            Assert.That(nonAtomicArg.val, Is.Not.Null);
            Assert.That(nonAtomicArg.val.type, Is.EqualTo(Term.TermType.DATUM));
            Assert.That(nonAtomicArg.val.datum, Is.Not.Null);
            Assert.That(nonAtomicArg.val.datum.type, Is.EqualTo(Datum.DatumType.R_BOOL));
            Assert.That(nonAtomicArg.val.datum.r_bool, Is.True);
        }

        [Test]
        public void NonAtomicFalse()
        {
            var sequenceQuery = Substitute.For<IMutableSingleObjectQuery<string>>();

            var query = new ReplaceQuery<string>(
                sequenceQuery,
                "Jackpot!",
                false);

            var term = query.GenerateTerm(datumConverterFactory, expressionConverterFactory);

            var nonAtomicArgs = term.optargs.Where(kv => kv.key == "non_atomic");
            Assert.That(nonAtomicArgs.Count(), Is.EqualTo(0));
        }
    }
}


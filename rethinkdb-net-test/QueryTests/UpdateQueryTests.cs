using System;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using RethinkDb.Spec;
using RethinkDb.QueryTerm;

namespace RethinkDb.Test.QueryTests
{
    [TestFixture]
    public class UpdateQueryTests
    {
        private IDatumConverterFactory datumConverterFactory;
        private IExpressionConverterFactory expressionConverterFactory = new RethinkDb.Expressions.DefaultExpressionConverterFactory();
        private IQueryConverter queryConverter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = Substitute.For<IDatumConverterFactory>();
            queryConverter = new QueryConverter(datumConverterFactory, expressionConverterFactory);

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
                .TryGet<string>(queryConverter, out value)
                .Returns(args => {
                        args[1] = stringDatumConverter;
                        return true;
                    });
            IDatumConverter value2;
            datumConverterFactory
                .TryGet(typeof(string), queryConverter, out value2)
                .Returns(args => {
                        args[2] = stringDatumConverter;
                        return true;
                    });
        }

        [Test]
        public void NonAtomicTrue()
        {
            var sequenceQuery = Substitute.For<ISequenceQuery<string>>();

            var query = new UpdateQuery<string>(
                sequenceQuery,
                s => "woot",
                true);

            var term = query.GenerateTerm(queryConverter);

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
            var sequenceQuery = Substitute.For<ISequenceQuery<string>>();

            var query = new UpdateQuery<string>(
                sequenceQuery,
                s => "woot",
                false);

            var term = query.GenerateTerm(queryConverter);

            var nonAtomicArgs = term.optargs.Where(kv => kv.key == "non_atomic");
            Assert.That(nonAtomicArgs.Count(), Is.EqualTo(0));
        }

        [Test]
        public void UpdateAndReturnChanges()
        {
            var sequenceQuery = Substitute.For<ISequenceQuery<string>>();

            var query = new UpdateAndReturnValueQuery<string>(
                sequenceQuery,
                s => "woot",
                false);

            var term = query.GenerateTerm(queryConverter);

            var returnChangesArgs = term.optargs.Where(kv => kv.key == "return_changes");
            Assert.That(returnChangesArgs.Count(), Is.EqualTo(1));

            var returnChangesArg = returnChangesArgs.Single();

            Assert.That(returnChangesArg.val, Is.Not.Null);
            Assert.That(returnChangesArg.val.type, Is.EqualTo(Term.TermType.DATUM));
            Assert.That(returnChangesArg.val.datum, Is.Not.Null);
            Assert.That(returnChangesArg.val.datum.type, Is.EqualTo(Datum.DatumType.R_BOOL));
            Assert.That(returnChangesArg.val.datum.r_bool, Is.True);
        }
    }
}


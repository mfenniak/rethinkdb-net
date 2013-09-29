using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using RethinkDb.Spec;
using NSubstitute;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class TupleDatumConverterTests
    {
        private IDatumConverterFactory datumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = Substitute.For<IDatumConverterFactory>();

            string[] strs = { "one", "two", "three", "left", "right" };

            var stringDatumConverter = Substitute.For<IDatumConverter>();
            foreach (var i in strs)
            {
                var dtm = i.ToDatum();
                stringDatumConverter
                    .ConvertObject(i)
                        .Returns(dtm);
                stringDatumConverter
                    .ConvertDatum(Arg.Is<Datum>(d => d.type == dtm.type && d.r_str == dtm.r_str))
                        .Returns(i);
            }

            IDatumConverter value;
            datumConverterFactory
                .TryGet(typeof(string), datumConverterFactory, out value)
                    .Returns(args =>
                             {
                        args[2] = stringDatumConverter;
                        return true;
                    });
        }

        [Test]
        public void ConvertDatum_Array_ReturnsValue()
        {
            string[] expected = {"one","two"};
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_ARRAY};
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            var value = TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);

            Assert.That(value, Is.EqualTo(new Tuple<string, string> ("one", "two")));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConvertDatum_Array_ExceptionOnSizeMismatch()
        {
            string[] expected = {"one","two","three"};
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_ARRAY};
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ExceptionOnUnsupportedType()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_NUM};
            TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_Object_ExceptionOnInvalidTupleSize()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_OBJECT};
            TupleDatumConverterFactory.Instance.Get<Tuple<string, string, string>>(datumConverterFactory).ConvertDatum(datum);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConvertDatum_Object_ExceptionOnInvalidKey()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_OBJECT};
            datum.r_object.Add(new Datum.AssocPair { key = "hi", val = null });
            datum.r_object.Add(new Datum.AssocPair { key = "group", val = null });

            TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);
        }

        [Test]
        public void ConvertDatum_Object_LeftRight()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_OBJECT};
            datum.r_object.Add(new Datum.AssocPair { key = "left", val = "left".ToDatum() });
            datum.r_object.Add(new Datum.AssocPair { key = "right", val = "right".ToDatum() });

            var value = TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);

            Assert.That(value, Is.EqualTo(new Tuple<string, string> ("left", "right")));

        }

        [Test]
        public void ConvertDatum_Object_GroupReduction()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_OBJECT};
            datum.r_object.Add(new Datum.AssocPair { key = "group", val = "left".ToDatum() });
            datum.r_object.Add(new Datum.AssocPair { key = "reduction", val = "right".ToDatum() });

            var value = TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);

            Assert.That(value, Is.EqualTo(new Tuple<string, string> ("left", "right")));
        }

        [Test]
        public void ConvertDatum_DefaultOnNull()
        {
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_NULL};
            var value = TupleDatumConverterFactory.Instance.Get<Tuple<string, string>>(datumConverterFactory).ConvertDatum(datum);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void ConvertDatum_Array_ReturnsNullValue()
        {
            string[] expected = {"one","two", null};
            var datum = new RethinkDb.Spec.Datum {type = RethinkDb.Spec.Datum.DatumType.R_ARRAY};
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            var value = TupleDatumConverterFactory.Instance.Get<Tuple<string, string, string>>(datumConverterFactory).ConvertDatum(datum);

            Assert.That(value, Is.EqualTo(new Tuple<string, string, string> ("one", "two", null)));
        }
    }
    
}

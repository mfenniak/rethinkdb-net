using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RethinkDb.Spec;
using NSubstitute;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class ArrayDatumConverterTests
    {
        private IDatumConverterFactory datumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = Substitute.For<IDatumConverterFactory>();

            int[] ints = {1, 2, 3};
           
            var intDatumConverter = Substitute.For<IDatumConverter>();
            foreach(int i in ints)
            {
                var dtm = i.ToDatum();
                intDatumConverter
                    .ConvertObject(i)
                    .Returns(dtm);
                intDatumConverter
                    .ConvertDatum(Arg.Is<Datum>(d => d.type == dtm.type && d.r_num == dtm.r_num))
                    .Returns(i);
            }

            string[] strs = { "one", "two", "three" };

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
                .TryGet(typeof(int), datumConverterFactory, out value)
                .Returns(args =>
                {
                    args[2] = intDatumConverter;
                    return true;
                });
            datumConverterFactory
                .TryGet(typeof(string), datumConverterFactory, out value)
                .Returns(args =>
                {
                    args[2] = stringDatumConverter;
                    return true;
                });
        }

        [Test]
        public void ConvertDatum_ReturnsValue()
        {
            int[] expected = {1, 2, 3};

            var datum = new Datum {type = Datum.DatumType.R_ARRAY};
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            var value = ArrayDatumConverterFactory.Instance.Get<int[]>(datumConverterFactory).ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertDatum_ReturnsValueReferenceType()
        {
            string[] expected = {"one", "two", "three"};

            var datum = new Datum {type = Datum.DatumType.R_ARRAY};
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            var value = ArrayDatumConverterFactory.Instance.Get<string[]>(datumConverterFactory).ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertDatum_ReturnsNullWithReferenceType()
        {
            string[] expected = { "one", "two", null, "three" };

            var datum = new Datum { type = Datum.DatumType.R_ARRAY };
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));

            var value = ArrayDatumConverterFactory.Instance.Get<string[]>(datumConverterFactory).ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertDatum_Empty()
        {
            var datum = new Datum { type = Datum.DatumType.R_ARRAY };
            var value = ArrayDatumConverterFactory.Instance.Get<int[]>(datumConverterFactory).ConvertDatum(datum);
            Assert.That(value, Is.Empty);
        }

        [Test]
        public void ConvertDatum_NullElementWithNotNullResult()
        {
            var expected = new List<int> { 1, 2, 3 };

            var datum = new Datum { type = Datum.DatumType.R_ARRAY };
            datum.r_array.AddRange(expected.Select(DatumHelpers.ToDatum));
            datum.r_array.Add(null);

            var value = ArrayDatumConverterFactory.Instance.Get<int[]>(datumConverterFactory).ConvertDatum(datum);

            expected.Add(0);
            Assert.That(value, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertObject()
        {
            int[] expected = {1, 2, 3};
            var obj = ArrayDatumConverterFactory.Instance.Get<int[]>(datumConverterFactory).ConvertObject(expected);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo(Datum.DatumType.R_ARRAY));
            Assert.That(obj.r_array.Select(r => r.r_num), Is.EqualTo(expected));
        }
    }
}


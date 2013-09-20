using System;
using NUnit.Framework;
using NSubstitute;

namespace RethinkDb.Test
{
    [TestFixture]
    public class AbstractDatumConverterFactoryTests
    {
        public class TestDatumConverterFactory : AbstractDatumConverterFactory
        {
            public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
            {
                if (typeof(T) == typeof(int))
                {
                    datumConverter = Substitute.For<IDatumConverter<T>>();
                    return true;
                }
                else
                {
                    datumConverter = null;
                    return false;
                }
            }
        }

        [Test]
        public void BaselineGenericTryGet()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter<int> dc;
            Assert.That(dcf.TryGet<int>(dcf, out dc), Is.True);
            Assert.That(dc, Is.Not.Null);
        }

        [Test]
        public void NonGenericTryGetSuccess()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter dc;
            Assert.That(dcf.TryGet(typeof(int), dcf, out dc), Is.True);
            Assert.That(dc, Is.Not.Null);
        }

        [Test]
        public void NonGenericTryGetFailure()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter dc;
            Assert.That(dcf.TryGet(typeof(string), dcf, out dc), Is.False);
            Assert.That(dc, Is.Null);
        }
    }
}


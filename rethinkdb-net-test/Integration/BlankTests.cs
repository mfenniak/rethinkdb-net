using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class BlankTests : TestBase
    {
        [Test]
        public void DbCreateTest()
        {
            DoDbCreateTest().Wait();
        }

        private async Task DoDbCreateTest()
        {
            var resp = await connection.RunAsync(Query.DbCreate("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.DbsCreated, Is.EqualTo(1));

            var dbList = await connection.RunAsync(Query.DbList());
            Assert.That(dbList, Is.Not.Null);
            Assert.That(dbList, Contains.Item("test"));

            resp = await connection.RunAsync(Query.DbDrop("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.DbsDropped, Is.EqualTo(1));
        }

        [Test]
        public void ExprPassthrough()
        {
            DoExprPassthrough().Wait();
        }

        private async Task DoExprPassthrough()
        {
            var obj = new TestObject() {
                Id = "123",
                Name = "456",
                SomeNumber = 789,
                Children = new TestObject[] {
                    new TestObject() { Id = "987", Name="654", SomeNumber = 321 },
                },
                ChildrenList = new List<TestObject> {
                    new TestObject() { Id = "987", Name = "654", SomeNumber = 321 },
                },
                ChildrenIList = new List<TestObject> {
                    new TestObject() { Id = "987", Name = "654", SomeNumber = 321 },
                },
            };
            var resp = await connection.RunAsync(Query.Expr(obj));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp, Is.EqualTo(obj));
        }

        [Test]
        public void ExprExpression()
        {
            DoExprExpression(() => 1.0 + 2.0, 3.0).Wait();
        }

        private async Task DoExprExpression<T>(Expression<Func<T>> objectExpr, T value)
        {
            var resp = await connection.RunAsync(Query.Expr(objectExpr));
            Assert.That(resp, Is.EqualTo(value));
        }

        [Test]
        public void ExprSequence()
        {
            DoExprSequence(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoExprSequence<T>(IEnumerable<T> enumerable)
        {
            var asyncEnumerable = connection.RunAsync(Query.Expr(enumerable));
            var count = 0;
            while (true)
            {
                if (!await asyncEnumerable.MoveNext())
                    break;
                ++count;
                Assert.That(asyncEnumerable.Current, Is.EqualTo(count));
            }
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void AsyncEnumeratorReset()
        {
            DoAsyncEnumeratorReset(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoAsyncEnumeratorReset<T>(IEnumerable<T> enumerable)
        {
            var asyncEnumerable = connection.RunAsync(Query.Expr(enumerable));

            asyncEnumerable.Reset();

            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(1.0);

            asyncEnumerable.Reset();

            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(1.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(2.0);

            asyncEnumerable.Reset();

            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(1.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(2.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(3.0);

            asyncEnumerable.Reset();

            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(1.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(2.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(3.0);
            (await asyncEnumerable.MoveNext()).Should().BeFalse();

            asyncEnumerable.Reset();

            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(1.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(2.0);
            (await asyncEnumerable.MoveNext()).Should().BeTrue();
            asyncEnumerable.Current.Should().Be(3.0);
            (await asyncEnumerable.MoveNext()).Should().BeFalse();
        }

        [Test]
        public void ExprNth()
        {
            DoExprNth(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoExprNth<T>(IEnumerable<T> enumerable)
        {
            var resp = await connection.RunAsync(Query.Expr(enumerable).Nth(1));
            Assert.That(resp, Is.EqualTo(2));
        }

        [Test]
        public void ExprDistinct()
        {
            DoExprDistinct().Wait();
        }

        private async Task DoExprDistinct()
        {
            var asyncEnumerable = connection.RunAsync(Query.Expr((IEnumerable<double>)new double[] { 1, 2, 3, 2, 1 }).Distinct());
            var count = 0;
            while (true)
            {
                if (!await asyncEnumerable.MoveNext())
                    break;
                ++count;
                Assert.That(asyncEnumerable.Current, Is.EqualTo(count));
            }
            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public void AnonymousTypeExpression()
        {
            var anonValue = connection.Run(Query.Expr(() => new { Prop1 = 1.0, Prop2 = 2.0, Prop3 = "3" }));
            Assert.That(anonValue.Prop1, Is.EqualTo(1.0));
            Assert.That(anonValue.Prop2, Is.EqualTo(2.0));
            Assert.That(anonValue.Prop3, Is.EqualTo("3"));
        }

        [Test]
        public void AnonymousTypeValue()
        {
            var anonValue = connection.Run(Query.Expr(new { Prop1 = 1.0, Prop2 = 2.0, Prop3 = "3" }));
            Assert.That(anonValue.Prop1, Is.EqualTo(1.0));
            Assert.That(anonValue.Prop2, Is.EqualTo(2.0));
            Assert.That(anonValue.Prop3, Is.EqualTo("3"));
        }

        [Test]
        public void DatumDefaultValues()
        {
            Assert.That(connection.Run(Query.Expr(false)), Is.False);
            Assert.That(connection.Run(Query.Expr(true)), Is.True);

            Assert.That(connection.Run(Query.Expr(0)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(1)), Is.EqualTo(1));

            Assert.That(connection.Run(Query.Expr("")), Is.EqualTo(""));
            Assert.That(connection.Run(Query.Expr("a")), Is.EqualTo("a"));
        }

        private const bool boolValConst = true;
        private const int intValConst = 0;
        private const string strValConst = "Hello";
        private static bool boolValStaticField = true;
        private static int intValStaticField = 0;
        private static string strValStaticField = "Hello";
        private static bool boolValStaticProperty { get { return true; } }
        private static int intValStaticProperty { get { return 0; } }
        private static string strValStaticProperty { get { return "Hello"; } }
        private bool boolValInstanceField = true;
        private int intValInstanceField = 0;
        private string strValInstanceField = "Hello";
        private bool boolValInstanceProperty { get { return true; } }
        private int intValInstanceProperty { get { return 0; } }
        private string strValInstanceProperty { get { return "Hello"; } }

        [Test]
        public void ExprVariableReferences()
        {
            bool boolVal = true;
            Assert.That(connection.Run(Query.Expr(() => boolVal)), Is.True);

            int intVal = 0;
            Assert.That(connection.Run(Query.Expr(() => intVal)), Is.EqualTo(0));

            string strVal = "Hello";
            Assert.That(connection.Run(Query.Expr(() => strVal)), Is.EqualTo("Hello"));
        }

        [Test]
        public void ExprConstReferences()
        {
            Assert.That(connection.Run(Query.Expr(() => boolValConst)), Is.True);
            Assert.That(connection.Run(Query.Expr(() => intValConst)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(() => strValConst)), Is.EqualTo("Hello"));
        }

        [Test]
        public void ExprStaticFieldReferences()
        {
            Assert.That(connection.Run(Query.Expr(() => boolValStaticField)), Is.True);
            Assert.That(connection.Run(Query.Expr(() => intValStaticField)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(() => strValStaticField)), Is.EqualTo("Hello"));
        }

        [Test]
        public void ExprStaticPropertyReferences()
        {
            Assert.That(connection.Run(Query.Expr(() => boolValStaticProperty)), Is.True);
            Assert.That(connection.Run(Query.Expr(() => intValStaticProperty)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(() => strValStaticProperty)), Is.EqualTo("Hello"));
        }

        [Test]
        public void ExprInstanceFieldReferences()
        {
            Assert.That(connection.Run(Query.Expr(() => boolValInstanceField)), Is.True);
            Assert.That(connection.Run(Query.Expr(() => intValInstanceField)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(() => strValInstanceField)), Is.EqualTo("Hello"));
        }

        [Test]
        public void ExprInstancePropertyReferences()
        {
            Assert.That(connection.Run(Query.Expr(() => boolValInstanceProperty)), Is.True);
            Assert.That(connection.Run(Query.Expr(() => intValInstanceProperty)), Is.EqualTo(0));
            Assert.That(connection.Run(Query.Expr(() => strValInstanceProperty)), Is.EqualTo("Hello"));
        }

        // Unit test for async/sync deadlock, mfenniak/rethinkdb-net#112.
        [Test]
        [Timeout(1000)]
        public void MixAsyncSyncPatternDeadlock()
        {
            DoMixAsyncSyncPatternDeadlock().Wait();
        }

        private async Task DoMixAsyncSyncPatternDeadlock()
        {
            await connection.RunAsync(Query.Expr(false));
            connection.Run(Query.Expr(false));
        }

        // testing of obsolete Query.Now
        #pragma warning disable 0618
        [Test]
        public void Now()
        {
            var now = connection.Run(Query.Now());
            Assert.That(now, Is.GreaterThan(DateTimeOffset.UtcNow.AddSeconds(-10)));
            Assert.That(now, Is.LessThan(DateTimeOffset.UtcNow.AddSeconds(10)));
        }
        #pragma warning restore 0618

        [Test]
        public void ExpressionNow()
        {
            var now = connection.Run(Query.Expr(() => DateTimeOffset.UtcNow));
            Assert.That(now, Is.GreaterThan(DateTimeOffset.UtcNow.AddSeconds(-10)));
            Assert.That(now, Is.LessThan(DateTimeOffset.UtcNow.AddSeconds(10)));
        }

        [Test]
        public void ConstructedError()
        {
#pragma warning disable 0429

            connection.Run(Query.Expr(() => 100 > 50 ? 1000 : ReQLExpression.Error<int>("unexpected error")));

            Action action = () => {
                connection.Run(Query.Expr(() => 50 > 100 ? 1000 : ReQLExpression.Error<int>("expected error"))); };
            action.ShouldThrow<RethinkDbRuntimeException>()
                .WithMessage("Runtime error: expected error");

#pragma warning restore 0429
        }

        [Test]
        public void ExpressionDate()
        {
            var date = connection.Run(Query.Expr(() => new DateTime(2014, 2, 3)));
            Assert.That(date, Is.EqualTo(new DateTime(2014, 2, 3)));
        }

        [Test]
        public void ExpressionDateTime()
        {
            var datetime = connection.Run(Query.Expr(() => new DateTime(2014, 2, 3, 7, 30, 15)));
            Assert.That(datetime, Is.EqualTo(new DateTime(2014, 2, 3, 7, 30, 15)));
        }

        [Test]
        public void ExpressionDateTimeOffset()
        {
            var datetime = connection.Run(Query.Expr(() => new DateTimeOffset(new DateTime(2014, 2, 3, 7, 30, 15))));
            Assert.That(datetime, Is.EqualTo(new DateTimeOffset(new DateTime(2014, 2, 3, 7, 30, 15, DateTimeKind.Utc))));
        }

        [Test]
        public void ExpressionDateTimeOffsetReinterpret()
        {
            var datetime = connection.Run(Query.Expr(() => new DateTimeOffset(new DateTime(2014, 2, 3, 7, 30, 15), new TimeSpan(-7, 30, 0))));
            Assert.That(datetime, Is.EqualTo(new DateTimeOffset(new DateTime(2014, 2, 3, 7, 30, 15), new TimeSpan(-7, 30, 0))));
        }

        [Test]
        public void ExpressionDateTimeWithMilliseconds()
        {
            var datetimems = connection.Run(Query.Expr(() => new DateTime(2014, 2, 3, 7, 30, 15, 500)));
            Assert.That(datetimems, Is.EqualTo(new DateTime(2014, 2, 3, 7, 30, 15, 500)));
        }

        [Test]
        public void RegexpMatch()
        {
            var match = connection.Run(Query.Expr(() => "hello".Match("l+")));
            match.MatchedString.Should().Be("ll");
            match.Start.Should().Be(2);
            match.End.Should().Be(4);
        }
    }
}

using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.Test
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
            var resp = await connection.Run(Query.DbCreate("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Created, Is.EqualTo(1));

            var dbList = await connection.Run(Query.DbList());
            Assert.That(dbList, Is.Not.Null);
            Assert.That(dbList, Contains.Item("test"));

            resp = await connection.Run(Query.DbDrop("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Dropped, Is.EqualTo(1));
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
                    new TestObject() { Id = "987", Name = "654", SomeNumber = 321 },
                },
            };
            var resp = await connection.Run(Query.Expr(obj));
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
            var resp = await connection.Run(Query.Expr(objectExpr));
            Assert.That(resp, Is.EqualTo(value));
        }

        [Test]
        public void ExprSequence()
        {
            DoExprSequence(new double[] { 1, 2, 3 }).Wait();
        }

        private async Task DoExprSequence<T>(IEnumerable<T> enumerable)
        {
            var retval = await connection.Run(Query.Expr(enumerable));
            Assert.That(retval.ToList(), Is.EqualTo(enumerable.ToList()));
        }
    }
}


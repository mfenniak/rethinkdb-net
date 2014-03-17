using System.Runtime.Serialization;
using NUnit.Framework;
using FluentAssertions;
using RethinkDb.DatumConverters;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb.Test.Expressions
{
    // The purpose of these unit tests is to handle reported issue #169, wherein a level of indirection in the types
    // being used in expression trees in the .NET CLR can sometimes cause useless Convert nodes to be added to the
    // expression tree.  rethinkdb-net needs to be able to handle these convert nodes.
    //
    // .NET CLR will insert Convert nodes into the expression tree in the IndirectCastByTypeConstraint... and
    // DirectCastWithTypeConstraint... unit tests, whereas mono does not.  The DirectIntentionalCastToExactSameType...
    // unit test under mono replicates what the .NET CLR does, so that we can be sure the case works even when running
    // tests on mono.

    [TestFixture]
    public class GenericTypeConstraintExpressionTests
    {
        IDatumConverterFactory datumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                DataContractDatumConverterFactory.Instance
            );
        }

        private static void AssertFunctionIsGetFieldSomeNumberSingleParameter(Term expr)
        {
            var funcTerm =
                new Term() {
                type = Term.TermType.FUNC,
                args = {
                    new Term() {
                        type = Term.TermType.MAKE_ARRAY,
                        args = {
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = 2,
                                }
                            }
                        }
                    },
                    new Term() {
                        type = Term.TermType.GET_FIELD,
                        args = {
                            new Term() {
                                type = Term.TermType.VAR,
                                args = {
                                    new Term() {
                                        type = Term.TermType.DATUM,
                                        datum = new Datum() {
                                            type = Datum.DatumType.R_NUM,
                                            r_num = 2,
                                        }
                                    }
                                }
                            },
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_STR,
                                    r_str = "number",
                                }
                            },
                        }
                    },
                }
            };
            expr.ShouldBeEquivalentTo(funcTerm);
        }

        private static void AssertFunctionIsGetFieldSomeNumberDoubleParameter(Term expr)
        {
            var funcTerm =
                new Term() {
                type = Term.TermType.FUNC,
                args = {
                    new Term() {
                        type = Term.TermType.MAKE_ARRAY,
                        args = {
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = 3,
                                }
                            },
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = 4,
                                }
                            }
                        }
                    },
                    new Term() {
                        type = Term.TermType.GET_FIELD,
                        args = {
                            new Term() {
                                type = Term.TermType.VAR,
                                args = {
                                    new Term() {
                                        type = Term.TermType.DATUM,
                                        datum = new Datum() {
                                            type = Datum.DatumType.R_NUM,
                                            r_num = 3,
                                        }
                                    }
                                }
                            },
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_STR,
                                    r_str = "number",
                                }
                            },
                        }
                    },
                }
            };
            expr.ShouldBeEquivalentTo(funcTerm);
        }

        public interface ITestInterface
        {
            double SomeNumber { get; set; }
        }

        [DataContract]
        public class TestObject : ITestInterface
        {
            [DataMember(Name = "id", EmitDefaultValue = false)]
            public string Id  { get; set; }

            [DataMember(Name = "number")]
            public double SomeNumber { get; set; }
        }

        [Test]
        public void DirectIntentionalCastToExactSameTypeSingleParameter()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<TestObject, double>(datumConverterFactory, o => ((ITestInterface)o).SomeNumber);
            AssertFunctionIsGetFieldSomeNumberSingleParameter(expr);
        }

        [Test]
        public void IndirectCastByTypeConstraintSingleParameter()
        {
            DoIndirectCastByTypeConstraintSingleParameter<TestObject>();
        }

        private void DoIndirectCastByTypeConstraintSingleParameter<T>() where T : ITestInterface
        {
            var expr = ExpressionUtils.CreateFunctionTerm<T, double>(datumConverterFactory, o => o.SomeNumber);
            AssertFunctionIsGetFieldSomeNumberSingleParameter(expr);
        }

        [Test]
        public void DirectCastWithTypeConstraintSingleParameter()
        {
            DoIndirectCastByTypeConstraintSingleParameter<TestObject>();
        }

        private void DoDirectCastWithTypeConstraintSingleParameter<T>() where T : ITestInterface
        {
            var expr = ExpressionUtils.CreateFunctionTerm<T, double>(datumConverterFactory, o => ((ITestInterface)o).SomeNumber);
            AssertFunctionIsGetFieldSomeNumberSingleParameter(expr);
        }

        [Test]
        public void DirectIntentionalCastToExactSameTypeDoubleParameter()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<TestObject, TestObject, double>(datumConverterFactory, (o1, o2) => ((ITestInterface)o1).SomeNumber);
            AssertFunctionIsGetFieldSomeNumberDoubleParameter(expr);
        }

        [Test]
        public void IndirectCastByTypeConstraintDoubleParameter()
        {
            DoIndirectCastByTypeConstraintDoubleParameter<TestObject>();
        }

        private void DoIndirectCastByTypeConstraintDoubleParameter<T>() where T : ITestInterface
        {
            var expr = ExpressionUtils.CreateFunctionTerm<T, T, double>(datumConverterFactory, (o1, o2) => o1.SomeNumber);
            AssertFunctionIsGetFieldSomeNumberDoubleParameter(expr);
        }

        [Test]
        public void DirectCastWithTypeConstraintDoubleParameter()
        {
            DoIndirectCastByTypeConstraintDoubleParameter<TestObject>();
        }

        private void DoDirectCastWithTypeConstraintDoubleParameter<T>() where T : ITestInterface
        {
            var expr = ExpressionUtils.CreateFunctionTerm<T, T, double>(datumConverterFactory, (o1, o2) => ((ITestInterface)o1).SomeNumber);
            AssertFunctionIsGetFieldSomeNumberDoubleParameter(expr);
        }
    }
}


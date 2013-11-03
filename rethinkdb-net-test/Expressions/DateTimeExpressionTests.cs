using System;
using NUnit.Framework;
using RethinkDb.QueryTerm;
using FluentAssertions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.Expressions
{
    [TestFixture]
    public class DateTimeExpressionTests
    {
        IDatumConverterFactory datumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                TimeSpanDatumConverterFactory.Instance,
                DateTimeDatumConverterFactory.Instance
            );
        }

        private void AssertAddFunction(Term expr, TimeSpan ts)
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
                            type = Term.TermType.ADD,
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
                                        type = Datum.DatumType.R_NUM,
                                        r_num = ts.TotalSeconds,
                                    }
                                },
                            }
                        },
                    }
                };
            expr.ShouldBeEquivalentTo(funcTerm);
        }

        private void AssertAddFunctionWithConversion(Term expr, double value, double conversion)
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
                            type = Term.TermType.ADD,
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
                                    type = Term.TermType.MUL,
                                    args = {
                                        new Term() {
                                            type = Term.TermType.DATUM,
                                            datum = new Datum() {
                                                type = Datum.DatumType.R_NUM,
                                                r_num = value,
                                            }
                                        },
                                        new Term() {
                                            type = Term.TermType.DATUM,
                                            datum = new Datum() {
                                                type = Datum.DatumType.R_NUM,
                                                r_num = conversion,
                                            }
                                        },
                                    }
                                }
                            }
                        },
                    }
                };
            expr.ShouldBeEquivalentTo(funcTerm);
        }

        [Test]
        public void DateTimePlusTimeSpan()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt + TimeSpan.FromSeconds(50));
            AssertAddFunction(expr, TimeSpan.FromSeconds(50));
        }

        [Test]
        public void DateTimeAddTimeSpan()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.Add(TimeSpan.FromSeconds(51)));
            AssertAddFunction(expr, TimeSpan.FromSeconds(51));
        }

        [Test]
        public void DateTimeAddDays()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddDays(1));
            AssertAddFunctionWithConversion(expr, 1, TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddHours()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddHours(1));
            AssertAddFunctionWithConversion(expr, 1, TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddMilliseconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddMilliseconds(100));
            AssertAddFunctionWithConversion(expr, 100, TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddMinutes()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddMinutes(23));
            AssertAddFunctionWithConversion(expr, 23, TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddSeconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddSeconds(123));
            AssertAddFunctionWithConversion(expr, 123, TimeSpan.TicksPerSecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddTicks()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(datumConverterFactory, dt => dt.AddTicks(1));
            AssertAddFunctionWithConversion(expr, 1, 1.0 / TimeSpan.TicksPerSecond);
        }
    }
}


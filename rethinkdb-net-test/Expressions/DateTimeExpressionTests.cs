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
        IExpressionConverterFactory expressionConverterFactory;
        IQueryConverter queryConverter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                TimeSpanDatumConverterFactory.Instance,
                DateTimeDatumConverterFactory.Instance
            );
            expressionConverterFactory = new RethinkDb.Expressions.DefaultExpressionConverterFactory();
            queryConverter = new QueryConverter(datumConverterFactory, expressionConverterFactory);
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
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt + TimeSpan.FromSeconds(50));
            AssertAddFunction(expr, TimeSpan.FromSeconds(50));
        }

        [Test]
        public void DateTimeOffsetPlusTimeSpan()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt + TimeSpan.FromSeconds(50));
            AssertAddFunction(expr, TimeSpan.FromSeconds(50));
        }

        [Test]
        public void DateTimeAddTimeSpan()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.Add(TimeSpan.FromSeconds(51)));
            AssertAddFunction(expr, TimeSpan.FromSeconds(51));
        }

        [Test]
        public void DateTimeOffsetAddTimeSpan()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.Add(TimeSpan.FromSeconds(51)));
            AssertAddFunction(expr, TimeSpan.FromSeconds(51));
        }

        [Test]
        public void DateTimeAddDays()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddDays(1));
            AssertAddFunctionWithConversion(expr, 1, (double)TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddDays()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddDays(1));
            AssertAddFunctionWithConversion(expr, 1, (double)TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddHours()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddHours(1));
            AssertAddFunctionWithConversion(expr, 1, (double)TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddHours()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddHours(1));
            AssertAddFunctionWithConversion(expr, 1, (double)TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddMilliseconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddMilliseconds(100));
            AssertAddFunctionWithConversion(expr, 100, (double)TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddMilliseconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddMilliseconds(100));
            AssertAddFunctionWithConversion(expr, 100, (double)TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddMinutes()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddMinutes(23));
            AssertAddFunctionWithConversion(expr, 23, (double)TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddMinutes()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddMinutes(23));
            AssertAddFunctionWithConversion(expr, 23, (double)TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddSeconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddSeconds(123));
            AssertAddFunctionWithConversion(expr, 123, (double)TimeSpan.TicksPerSecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddSeconds()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddSeconds(123));
            AssertAddFunctionWithConversion(expr, 123, (double)TimeSpan.TicksPerSecond / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeAddTicks()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTime, DateTime>(queryConverter, dt => dt.AddTicks(1));
            AssertAddFunctionWithConversion(expr, 1, 1.0 / TimeSpan.TicksPerSecond);
        }

        [Test]
        public void DateTimeOffsetAddTicks()
        {
            var expr = ExpressionUtils.CreateFunctionTerm<DateTimeOffset, DateTimeOffset>(queryConverter, dt => dt.AddTicks(1));
            AssertAddFunctionWithConversion(expr, 1, 1.0 / TimeSpan.TicksPerSecond);
        }
    }
}


using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReduceQuery<T> : ISingleObjectQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Expression<Func<T, T, T>> reduceFunction;
        private readonly bool baseProvided;
        private readonly T @base;

        public ReduceQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            this.sequenceQuery = sequenceQuery;
            this.reduceFunction = reduceFunction;
            this.baseProvided = false;
        }

        public ReduceQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T @base)
           : this (sequenceQuery, reduceFunction)
        {
            this.baseProvided = true;
            this.@base = @base;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var reduceTerm = new Term()
            {
                type = Term.TermType.REDUCE,
            };
            reduceTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            reduceTerm.args.Add(ExpressionUtils.CreateFunctionTerm<T, T, T>(datumConverterFactory, reduceFunction));

            if (this.baseProvided)
            {
                reduceTerm.optargs.Add(new Term.AssocPair() {
                    key = "base",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = datumConverterFactory.Get<T>().ConvertObject(@base)
                    }
                });
            }

            return reduceTerm;
        }
    }
}

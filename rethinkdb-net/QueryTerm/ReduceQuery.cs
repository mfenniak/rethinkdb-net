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

        public ReduceQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            this.sequenceQuery = sequenceQuery;
            this.reduceFunction = reduceFunction;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var reduceTerm = new Term()
            {
                type = Term.TermType.REDUCE,
            };
            reduceTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            reduceTerm.args.Add(ExpressionUtils.CreateFunctionTerm<T, T, T>(datumConverterFactory, reduceFunction));
            return reduceTerm;
        }
    }
}

using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ReduceQuery<T> : ISingleObjectQuery<T>
    {
        private ISequenceQuery<T> sequenceQuery;
        private Expression<Func<T, T, T>> reduceFunction;
        private T seed;

        public ReduceQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T seed)
        {
            this.sequenceQuery = sequenceQuery;
            this.reduceFunction = reduceFunction;
            this.seed = seed;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var reduceTerm = new Term()
            {
                type = Term.TermType.REDUCE,
            };
            reduceTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));

            if (reduceFunction.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");

            var body = reduceFunction.Body;
            reduceTerm.args.Add(ExpressionUtils.MapLambdaToFunction<T, T>(datumConverterFactory, (LambdaExpression)reduceFunction));

            reduceTerm.optargs.Add(new Term.AssocPair() {
                key = "base",
                val = new Term() {
                    type = Term.TermType.DATUM,
                    datum = datumConverterFactory.Get<T>().ConvertObject(seed)
                }
            });

            return reduceTerm;
        }
    }
}

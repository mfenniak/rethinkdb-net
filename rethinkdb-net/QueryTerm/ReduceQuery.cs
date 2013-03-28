using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ReduceQuery<TOriginal, TReduce> : ISingleObjectQuery<TReduce>
    {
        private ISequenceQuery<TOriginal> sequenceQuery;
        private Expression<Func<TReduce, TOriginal, TReduce>> reduceFunction;
        private TReduce seed;

        public ReduceQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TReduce, TOriginal, TReduce>> reduceFunction, TReduce seed)
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
            reduceTerm.args.Add(ExpressionUtils.MapLambdaToFunction<TReduce, TOriginal>(datumConverterFactory, (LambdaExpression)reduceFunction));

            reduceTerm.optargs.Add(new Term.AssocPair() {
                key = "base",
                val = new Term() {
                    type = Term.TermType.DATUM,
                    datum = datumConverterFactory.Get<TReduce>().ConvertObject(seed)
                }
            });

            return reduceTerm;
        }
    }
}

using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MapQuery<TOriginal, TTarget> : ISequenceQuery<TTarget>
    {
        private readonly ISequenceQuery<TOriginal> sequenceQuery;
        private readonly Expression<Func<TOriginal, TTarget>> mapExpression;

        public MapQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            this.sequenceQuery = sequenceQuery;
            this.mapExpression = mapExpression;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var mapTerm = new Term()
            {
                type = Term.TermType.MAP,
            };
            mapTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));

            if (mapExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");
            mapTerm.args.Add(ExpressionUtils.ConvertMapFunctionToTerm<TOriginal, TTarget>(datumConverterFactory, (LambdaExpression)mapExpression));

            return mapTerm;
        }
    }
}

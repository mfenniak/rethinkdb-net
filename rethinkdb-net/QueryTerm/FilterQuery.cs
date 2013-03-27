using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class FilterQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Expression<Func<T, bool>> filterExpression;

        public FilterQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, bool>> filterExpression)
        {
            this.sequenceQuery = sequenceQuery;
            this.filterExpression = filterExpression;
        }

        public Term GenerateTerm()
        {
            var filterTerm = new Term()
            {
                type = Term.TermType.FILTER,
            };
            filterTerm.args.Add(sequenceQuery.GenerateTerm());

            if (filterExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");

            var body = filterExpression.Body;
            filterTerm.args.Add(MapLambdaToTerm(body));

            return filterTerm;
        }

        private Term MapLambdaToTerm(Expression lambdaBody)
        {
            var funcTerm = new Term() {
                type = Term.TermType.FUNC
            };

            var parametersTerm = new Term() {
                type = Term.TermType.MAKE_ARRAY,
            };
            parametersTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 2
                }
            });
            funcTerm.args.Add(parametersTerm);
            funcTerm.args.Add(ExpressionUtils.MapExpressionToTerm<T>(lambdaBody));
            return funcTerm;
        }

    }
}

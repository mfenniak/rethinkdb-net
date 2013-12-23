using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class EqJoinQuery<TLeft, TRight> : ISequenceQuery<Tuple<TLeft, TRight>>
    {
        private readonly ISequenceQuery<TLeft> leftQuery;
        private readonly ISequenceQuery<TRight> rightQuery;
        private readonly Expression<Func<TLeft, object>> leftMemberReferenceExpression;
        private readonly string indexName;

        public EqJoinQuery(ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery, string indexName)
        {
            this.leftQuery = leftQuery;
            this.leftMemberReferenceExpression = leftMemberReferenceExpression;
            this.rightQuery = rightQuery;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.EQ_JOIN,
            };
            term.args.Add(leftQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            term.args.Add(GetMemberName(datumConverterFactory));
            term.args.Add(rightQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));

            if (!String.IsNullOrEmpty(indexName))
            {
                term.optargs.Add(new Term.AssocPair() {
                    key = "index",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = indexName
                        },
                    }
                });
            }

            return term;
        }

        private Term GetMemberName(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TLeft>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            if (leftMemberReferenceExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type " + leftMemberReferenceExpression.Type + "; expected Lambda");

            var body = leftMemberReferenceExpression.Body;
            MemberExpression memberExpr;

            if (body.NodeType == ExpressionType.MemberAccess)
                memberExpr = (MemberExpression)body;
            else
                throw new NotSupportedException("Unsupported expression type " + body.NodeType + "; expected MemberAccess");

            if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                throw new NotSupportedException("Unrecognized member access pattern");

            var fieldReference = new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
                }
            };

            return fieldReference;
        }
    }
}

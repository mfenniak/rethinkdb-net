using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class HasFieldsSequenceQuery<T> : HasFieldsQuery<T>, ISequenceQuery<T>
    {
        public HasFieldsSequenceQuery(ISequenceQuery<T> query, IEnumerable<Expression<Func<T, object>>> fields)
            : base(query, fields)
        {
        }
    }

    public class HasFieldsSingleObjectQuery<T> : HasFieldsQuery<T>, ISingleObjectQuery<bool>
    {
        public HasFieldsSingleObjectQuery(ISingleObjectQuery<T> query, IEnumerable<Expression<Func<T, object>>> fields)
            : base(query, fields)
        {
        }
    }

    public class HasFieldsQuery<T> : IQuery
    {
        private readonly IQuery query;
        private readonly IEnumerable<Expression<Func<T, object>>> fields;

        protected HasFieldsQuery(IQuery query, IEnumerable<Expression<Func<T, object>>> fields)
        {
            this.query = query;
            this.fields = fields;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var hasFieldsTerm = new Term()
            {
                type = Term.TermType.HAS_FIELDS,
            };
            hasFieldsTerm.args.Add(query.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            hasFieldsTerm.args.AddRange(GetMembers(datumConverterFactory));
            return hasFieldsTerm;
        }

        private IEnumerable<Term> GetMembers(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<T>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            foreach (var memberReferenceExpression in fields)
            {
                if (memberReferenceExpression.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type " + memberReferenceExpression.Type + "; expected Lambda");

                var body = memberReferenceExpression.Body;
                MemberExpression memberExpr;

                if (body.NodeType == ExpressionType.MemberAccess)
                    memberExpr = (MemberExpression)body;
                else
                    throw new NotSupportedException("Unsupported expression type " + body.NodeType + "; expected MemberAccess or Call");

                if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                    throw new NotSupportedException("Unrecognized member access pattern");

                var fieldReference = new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
                    }
                };

                yield return fieldReference;
            }
        }
    }
}
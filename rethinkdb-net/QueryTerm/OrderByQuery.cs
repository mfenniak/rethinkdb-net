using RethinkDb.Spec;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RethinkDb.QueryTerm
{
    public class OrderByQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Expression<Func<T, object>>[] memberReferenceExpressions;

        public OrderByQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            this.sequenceQuery = sequenceQuery;
            this.memberReferenceExpressions = memberReferenceExpressions;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var orderByTerm = new Term()
            {
                type = Term.TermType.ORDERBY,
            };
            orderByTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            orderByTerm.args.AddRange(GetMembers(datumConverterFactory));
            return orderByTerm;
        }

        private IEnumerable<Term> GetMembers(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<T>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            foreach (var memberReferenceExpression in memberReferenceExpressions)
            {
                if (memberReferenceExpression.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type " + memberReferenceExpression.Type + "; expected Lambda");

                var body = memberReferenceExpression.Body;
                MemberExpression memberExpr;
                string direction = null;

                if (body.NodeType == ExpressionType.Call)
                {
                    MethodCallExpression methodCall = (MethodCallExpression)body;
                    if (methodCall.Method.Equals(typeof(Query).GetMethod("Asc", BindingFlags.Static | BindingFlags.Public)))
                        direction = "asc";
                    else if (methodCall.Method.Equals(typeof(Query).GetMethod("Desc", BindingFlags.Static | BindingFlags.Public)))
                         direction = "desc";
                    else
                        throw new NotSupportedException("Unsupported OrderBy method call; only Query.Asc and Query.Desc are suported");

                    if (methodCall.Arguments.Count != 1)
                        throw new NotSupportedException("Only a single argument is supported to Query.Asc/Query.Desc");

                    var arg = methodCall.Arguments[0];
                    if (arg.NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("Unsupported expression type " + body.NodeType + "; expected MemberAccess");

                    memberExpr = (MemberExpression)arg;
                }
                else if (body.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpr = (MemberExpression)body;
                }
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

                if (direction == "asc")
                {
                    var newFieldRef = new Term() {
                        type = Term.TermType.ASC,
                    };
                    newFieldRef.args.Add(fieldReference);
                    fieldReference = newFieldRef;
                }
                else if (direction == "desc")
                {
                    var newFieldRef = new Term() {
                        type = Term.TermType.DESC,
                    };
                    newFieldRef.args.Add(fieldReference);
                    fieldReference = newFieldRef;
                }

                yield return fieldReference;
            }
        }
    }
}
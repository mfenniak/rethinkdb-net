using RethinkDb.Spec;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RethinkDb.QueryTerm
{
    public class OrderByQuery<T> : IOrderedSequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Tuple<Expression<Func<T, object>>, OrderByDirection>[] orderByMembers;

        public OrderByQuery(ISequenceQuery<T> sequenceQuery, params Tuple<Expression<Func<T, object>>, OrderByDirection>[] orderByMembers)
        {
            this.sequenceQuery = sequenceQuery;
            this.orderByMembers = orderByMembers;
        }

        public ISequenceQuery<T> SequenceQuery
        {
            get { return sequenceQuery; }
        }

        public IEnumerable<Tuple<Expression<Func<T, object>>, OrderByDirection>> OrderByMembers
        {
            get { return orderByMembers; }
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

            foreach (var orderByMember in orderByMembers)
            {
                var memberReferenceExpression = orderByMember.Item1;
                var direction = orderByMember.Item2;

                if (memberReferenceExpression.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type " + memberReferenceExpression.Type + "; expected Lambda");

                var body = memberReferenceExpression.Body;
                MemberExpression memberExpr;

                if (body.NodeType == ExpressionType.Convert)
                {
                    // If we're order-bying a primitive, the expr will be a cast to object for the Asc/Desc method call
                    if (body.Type == typeof(object))
                        body = ((UnaryExpression)body).Operand;
                }

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

                if (direction == OrderByDirection.Ascending)
                {
                    var newFieldRef = new Term() {
                        type = Term.TermType.ASC,
                    };
                    newFieldRef.args.Add(fieldReference);
                    fieldReference = newFieldRef;
                }
                else if (direction == OrderByDirection.Descending)
                {
                    var newFieldRef = new Term() {
                        type = Term.TermType.DESC,
                    };
                    newFieldRef.args.Add(fieldReference);
                    fieldReference = newFieldRef;
                }
                else
                    throw new NotSupportedException();

                yield return fieldReference;
            }
        }
    }
}
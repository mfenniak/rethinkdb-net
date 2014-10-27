using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class OrderByQuery<T> : IOrderedSequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly OrderByTerm<T>[] orderByMembers;

        public OrderByQuery(ISequenceQuery<T> sequenceQuery, params OrderByTerm<T>[] orderByMembers)
        {
            this.sequenceQuery = sequenceQuery;
            this.orderByMembers = orderByMembers;
        }

        public ISequenceQuery<T> SequenceQuery
        {
            get { return sequenceQuery; }
        }

        public IEnumerable<OrderByTerm<T>> OrderByMembers
        {
            get { return orderByMembers; }
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            Term indexOrderBy;

            var orderByTerm = new Term()
            {
                type = Term.TermType.ORDER_BY,
            };
            orderByTerm.args.Add(sequenceQuery.GenerateTerm(queryConverter));
            orderByTerm.args.AddRange(GetMembers(queryConverter, out indexOrderBy));

            if (indexOrderBy != null)
            {
                orderByTerm.optargs.Add(new Term.AssocPair() {
                    key = "index",
                    val = indexOrderBy
                });
            }

            return orderByTerm;
        }

        private IEnumerable<Term> GetMembers(IQueryConverter queryConverter, out Term indexOrderBy)
        {
            var datumConverter = queryConverter.Get<T>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            indexOrderBy = null;
            List<Term> retval = new List<Term>(orderByMembers.Length);

            foreach (var orderByMember in orderByMembers)
            {
                var memberReferenceExpression = orderByMember.Expression;
                var direction = orderByMember.Direction;

                if (!String.IsNullOrEmpty(orderByMember.IndexName))
                {
                    if (indexOrderBy != null)
                        throw new InvalidOperationException("Sorting by multiple indexes is not supported");

                    var indexReference = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = orderByMember.IndexName
                        }
                    };
                    if (direction == OrderByDirection.Ascending)
                    {
                        var newFieldRef = new Term() {
                            type = Term.TermType.ASC,
                        };
                        newFieldRef.args.Add(indexReference);
                        indexOrderBy = newFieldRef;
                    }
                    else if (direction == OrderByDirection.Descending)
                    {
                        var newFieldRef = new Term() {
                            type = Term.TermType.DESC,
                        };
                        newFieldRef.args.Add(indexReference);
                        indexOrderBy = newFieldRef;
                    }
                    else
                        throw new NotSupportedException();
                }
                else
                {
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

                    retval.Add(fieldReference);
                }
            }

            return retval;
        }
    }
}
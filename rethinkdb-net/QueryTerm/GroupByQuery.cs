using System;
using RethinkDb.Spec;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.QueryTerm
{
    public abstract class GroupByQueryBase<TObject, TReductionType>
    {
        private readonly ISequenceQuery<TObject> sequenceQuery;
        private readonly IGroupByReduction<TReductionType> reductionObject;
        private readonly IEnumerable<Expression> memberReferences;

        protected GroupByQueryBase(ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, params Expression[] groupMemberReferences)
        {
            this.sequenceQuery = sequenceQuery;
            this.reductionObject = reductionObject;
            this.memberReferences = groupMemberReferences;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.GROUPBY,
            };
            term.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));

            var propertyTerm = new Term() {
                type = Term.TermType.MAKE_ARRAY
            };
            foreach (var memberReference in memberReferences)
            {
                propertyTerm.args.Add(new Term() {
                    type = Term.TermType.DATUM,
                    datum = GetMemberName(memberReference, datumConverterFactory)
                });
            }
            term.args.Add(propertyTerm);

            term.args.Add(reductionObject.GenerateReductionObject(datumConverterFactory));

            return term;
        }

        private Datum GetMemberName(Expression memberReference, IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TObject>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            if (memberReference.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type " + memberReference.Type + "; expected Lambda");

            var body = ((LambdaExpression)memberReference).Body;
            MemberExpression memberExpr;

            if (body.NodeType == ExpressionType.MemberAccess)
                memberExpr = (MemberExpression)body;
            else
                throw new NotSupportedException("Unsupported expression type " + body.NodeType + "; expected MemberAccess");

            if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                throw new NotSupportedException("Unrecognized member access pattern");

            return new Datum() {
                type = Datum.DatumType.R_STR,
                r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
            };
        }
    }

    public class GroupByQuery<TObject, TReductionType, TGroupByType1>
        : GroupByQueryBase<TObject, TReductionType>, ISequenceQuery<Tuple<Tuple<TGroupByType1>, TReductionType>>
    {
        public GroupByQuery(ISequenceQuery<TObject> sequenceQuery,
                            IGroupByReduction<TReductionType> reductionObject,
                            Expression<Func<TObject, TGroupByType1>> groupMemberReference1)
            : base(sequenceQuery, reductionObject, groupMemberReference1)
        {
        }
    }

    public class GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2>
        : GroupByQueryBase<TObject, TReductionType>, ISequenceQuery<Tuple<Tuple<TGroupByType1, TGroupByType2>, TReductionType>>
    {
        public GroupByQuery(ISequenceQuery<TObject> sequenceQuery,
                            IGroupByReduction<TReductionType> reductionObject,
                            Expression<Func<TObject, TGroupByType1>> groupMemberReference1,
                            Expression<Func<TObject, TGroupByType2>> groupMemberReference2)
            : base(sequenceQuery, reductionObject, groupMemberReference1, groupMemberReference2)
        {
        }
    }

    public class GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2, TGroupByType3>
        : GroupByQueryBase<TObject, TReductionType>, ISequenceQuery<Tuple<Tuple<TGroupByType1, TGroupByType2, TGroupByType3>, TReductionType>>
    {
        public GroupByQuery(ISequenceQuery<TObject> sequenceQuery,
                            IGroupByReduction<TReductionType> reductionObject,
                            Expression<Func<TObject, TGroupByType1>> groupMemberReference1,
                            Expression<Func<TObject, TGroupByType2>> groupMemberReference2,
                            Expression<Func<TObject, TGroupByType3>> groupMemberReference3)
            : base(sequenceQuery, reductionObject, groupMemberReference1, groupMemberReference2, groupMemberReference3)
        {
        }
    }
}

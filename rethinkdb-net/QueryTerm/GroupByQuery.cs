using System;
using RethinkDb.Spec;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class GroupByQuery<TObject, TReductionType, TGroupByType1> : ISequenceQuery<Tuple<Tuple<TGroupByType1>, TReductionType>>
    {
        private readonly ISequenceQuery<TObject> sequenceQuery;
        private readonly IGroupByReduction<TReductionType> reductionObject;
        private readonly Expression<Func<TObject, TGroupByType1>> groupMemberReference1;

        public GroupByQuery(ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupByType1>> groupMemberReference1)
        {
            this.sequenceQuery = sequenceQuery;
            this.reductionObject = reductionObject;
            this.groupMemberReference1 = groupMemberReference1;
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
            propertyTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = GetMemberName(datumConverterFactory)
            });
            term.args.Add(propertyTerm);

            term.args.Add(reductionObject.GenerateReductionObject(datumConverterFactory));

            return term;
        }

        private Datum GetMemberName(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TObject>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            if (groupMemberReference1.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type " + groupMemberReference1.Type + "; expected Lambda");

            var body = groupMemberReference1.Body;
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
}


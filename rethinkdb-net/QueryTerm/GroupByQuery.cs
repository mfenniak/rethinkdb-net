using System;
using RethinkDb.Spec;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.QueryTerm
{
    public abstract class GroupByQueryBase<TObject, TReductionType>
    {
        private readonly ISequenceQuery<TObject> sequenceQuery;
        private readonly IGroupByReduction<TReductionType> reductionObject;
        private readonly Expression groupKeyConstructor;

        protected GroupByQueryBase(ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression groupKeyConstructor)
        {
            this.sequenceQuery = sequenceQuery;
            this.reductionObject = reductionObject;
            this.groupKeyConstructor = groupKeyConstructor;
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

            if (groupKeyConstructor.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type " + groupKeyConstructor.NodeType + "; expected Lambda");

            var body = ((LambdaExpression)groupKeyConstructor).Body;
            if (body.NodeType != ExpressionType.New)
                throw new NotSupportedException("GroupByQuery expects an expression in the form of: new { key1 = ...[, keyN = ...] }");

            var newExpression = (NewExpression)body;
            if (!AnonymousTypeDatumConverterFactory.Instance.IsTypeSupported(newExpression.Type))
                throw new NotSupportedException(String.Format("Unsupported type in New expression: {0}; only anonymous types are supported", newExpression.Type));

            foreach (var property in newExpression.Type.GetProperties().Select((p, i) => new { Property = p, Index = i }))
            {
                var key = property.Property.Name;
                var value = GetMemberName(newExpression.Arguments[property.Index], datumConverterFactory);
                if (key != value.r_str)
                    throw new Exception(String.Format("Anonymous type property name ({0}) must equal the member name ({1})", key, value.r_str));
                propertyTerm.args.Add(new Term() {
                    type = Term.TermType.DATUM,
                    datum = value,
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

            MemberExpression memberExpr;
            if (memberReference.NodeType == ExpressionType.MemberAccess)
                memberExpr = (MemberExpression)memberReference;
            else
                throw new NotSupportedException("Unsupported expression type " + memberReference.NodeType + "; expected MemberAccess");

            if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                throw new NotSupportedException("Unrecognized member access pattern");

            return new Datum() {
                type = Datum.DatumType.R_STR,
                r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
            };
        }
    }

    public class GroupByQuery<TObject, TReductionType, TGroupKeyType>
        : GroupByQueryBase<TObject, TReductionType>, ISequenceQuery<Tuple<TGroupKeyType, TReductionType>>
    {
        public GroupByQuery(ISequenceQuery<TObject> sequenceQuery,
                            IGroupByReduction<TReductionType> reductionObject,
                            Expression<Func<TObject, TGroupKeyType>> groupKeyConstructor)
            : base(sequenceQuery, reductionObject, groupKeyConstructor)
        {
        }
    }
}

using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class AvgReduction<TObject> : IGroupByReduction<double>
    {
        private readonly Expression<Func<TObject, double>> numericMemberReference;

        public AvgReduction(Expression<Func<TObject, double>> numericMemberReference)
        {
            this.numericMemberReference = numericMemberReference;
        }

        public Term GenerateReductionObject(IDatumConverterFactory datumConverterFactory)
        {
            var retval = new Term() {
                type = Term.TermType.MAKE_OBJ
            };
            retval.optargs.Add(new Term.AssocPair() {
                key = "AVG",
                val = new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = GetMemberName(datumConverterFactory)
                    }
                }
            });
            return retval;
        }

        private string GetMemberName(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TObject>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

            if (numericMemberReference.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type " + numericMemberReference.Type + "; expected Lambda");

            var body = ((LambdaExpression)numericMemberReference).Body;
            MemberExpression memberExpr;

            if (body.NodeType == ExpressionType.MemberAccess)
                memberExpr = (MemberExpression)body;
            else
                throw new NotSupportedException("Unsupported expression type " + body.NodeType + "; expected MemberAccess");

            if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                throw new NotSupportedException("Unrecognized member access pattern");

            return fieldConverter.GetDatumFieldName(memberExpr.Member);
        }
    }
}


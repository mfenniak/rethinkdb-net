using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MapQuery<TOriginal, TTarget> : ISequenceQuery<TTarget>
    {
        private readonly ISequenceQuery<TOriginal> sequenceQuery;
        private readonly Expression<Func<TOriginal, TTarget>> mapExpression;

        public MapQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            this.sequenceQuery = sequenceQuery;
            this.mapExpression = mapExpression;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var mapTerm = new Term()
            {
                type = Term.TermType.MAP,
            };
            mapTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));

            if (mapExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");

            var body = mapExpression.Body;
            if (body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException("Only MemberInit expression type is supported inside lambda");

            var memberInit = (MemberInitExpression)body;
            if (memberInit.Type != typeof(TTarget))
                throw new InvalidOperationException("Only expression types matching the table type are supported");
            else if (memberInit.NewExpression.Arguments.Count != 0)
                throw new NotSupportedException("Constructors will not work here, only field member initialization");

            mapTerm.args.Add(MapMemberInitToTerm(datumConverterFactory, memberInit));

            return mapTerm;
        }

        private Term MapMemberInitToTerm(IDatumConverterFactory datumConverterFactory, MemberInitExpression memberInit)
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

            var makeObjTerm = new Term() {
                type = Term.TermType.MAKE_OBJ,
            };
            funcTerm.args.Add(makeObjTerm);

            foreach (var binding in memberInit.Bindings)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        makeObjTerm.optargs.Add(MapMemberAssignmentToMakeObjArg(datumConverterFactory, (MemberAssignment)binding));
                        break;
                    case MemberBindingType.ListBinding:
                    case MemberBindingType.MemberBinding:
                        throw new NotSupportedException("Binding type not currently supported");
                }
            }

            return funcTerm;
        }

        private Term.AssocPair MapMemberAssignmentToMakeObjArg(IDatumConverterFactory datumConverterFactory, MemberAssignment memberAssignment)
        {
            var retval = new Term.AssocPair();

            var datumConverter = datumConverterFactory.Get<TTarget>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = ExpressionUtils.MapExpressionToTerm<TOriginal>(datumConverterFactory, memberAssignment.Expression);

            return retval;
        }
    }
}

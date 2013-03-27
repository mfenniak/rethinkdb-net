using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateQuery<T> : IWriteQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceTerm;
        private readonly IMutableSingleObjectQuery<T> singleObjectTerm;
        private readonly Expression<Func<T, T>> updateExpression;

        public UpdateQuery(ISequenceQuery<T> tableTerm, Expression<Func<T, T>> updateExpression)
        {
            this.sequenceTerm = tableTerm;
            this.updateExpression = updateExpression;
        }

        public UpdateQuery(IMutableSingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression)
        {
            this.singleObjectTerm = singleObjectTerm;
            this.updateExpression = updateExpression;
        }

        public Term GenerateTerm(IDatumConverter<T> datumConverter)
        {
            var updateTerm = new Term()
            {
                type = Term.TermType.UPDATE,
            };
            if (singleObjectTerm != null)
                updateTerm.args.Add(singleObjectTerm.GenerateTerm());
            else
                updateTerm.args.Add(sequenceTerm.GenerateTerm());

            if (updateExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");

            var body = updateExpression.Body;
            if (body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException("Only MemberInit expression type is supported inside lambda");

            var memberInit = (MemberInitExpression)body;
            if (memberInit.Type != typeof(T))
                throw new InvalidOperationException("Only expression types matching the table type are supported");
            else if (memberInit.NewExpression.Arguments.Count != 0)
                throw new NotSupportedException("Constructors will not work here, only field member initialization");

            updateTerm.args.Add(MapMemberInitToTerm(memberInit));

            return updateTerm;
        }

        private Term MapMemberInitToTerm(MemberInitExpression memberInit)
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
                        makeObjTerm.optargs.Add(MapMemberAssignmentToMakeObjArg((MemberAssignment)binding));
                        break;
                    case MemberBindingType.ListBinding:
                    case MemberBindingType.MemberBinding:
                        throw new NotSupportedException("Binding type not currently supported");
                }
            }

            return funcTerm;
        }

        private Term.AssocPair MapMemberAssignmentToMakeObjArg(MemberAssignment memberAssignment)
        {
            var retval = new Term.AssocPair();

            var datumConverter = DataContractDatumConverterFactory.Instance.Get<T>();

            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = ExpressionUtils.MapExpressionToTerm<T>(memberAssignment.Expression);

            return retval;
        }
    }
}

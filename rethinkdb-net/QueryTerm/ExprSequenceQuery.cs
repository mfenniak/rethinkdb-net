using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ExprSequenceQuery<T> : ISequenceQuery<T>
    {
        private readonly IEnumerable<T> enumerable;

        public ExprSequenceQuery(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var sequenceTerm = new Term()
            {
                type = Term.TermType.MAKE_ARRAY,
            };

            foreach (var obj in enumerable)
            {
                var datumTerm = new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = datumConverterFactory.Get<T>().ConvertObject(obj)
                };
                sequenceTerm.args.Add(datumTerm);
            }

            return sequenceTerm;
        }
    }
}


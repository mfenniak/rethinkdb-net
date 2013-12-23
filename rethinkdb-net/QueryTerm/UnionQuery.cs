using RethinkDb.Spec;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.QueryTerm
{
    public class UnionQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> query1;
        private readonly ISequenceQuery<T> query2;

        public UnionQuery(ISequenceQuery<T> query1, ISequenceQuery<T> query2)
        {
            this.query1 = query1;
            this.query2 = query2;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.UNION,
            };
            term.args.Add(query1.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            term.args.Add(query2.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            return term;
        }
    }
}

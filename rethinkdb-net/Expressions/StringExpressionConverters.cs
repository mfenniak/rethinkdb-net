using System;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class StringExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<string, string>(
                s => s.ToLowerInvariant(),
                s => new Term() { type = Term.TermType.DOWNCASE, args = { s } });

            expressionConverterFactory.RegisterTemplateMapping<string, string>(
                s => s.ToUpperInvariant(),
                s => new Term() { type = Term.TermType.UPCASE, args = { s } });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class DictionaryExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<Dictionary<string, object>, string, bool>(
                (d, k) => d.ContainsKey(k),
                (d, k) => new Term() { type = Term.TermType.HAS_FIELDS, args = { d, k } });

            expressionConverterFactory.RegisterTemplateMapping<Dictionary<string, object>, Dictionary<string, object>.KeyCollection>(
                (d) => d.Keys,
                (d) => new Term() { type = Term.TermType.KEYS, args = { d } });
        }
    }
}

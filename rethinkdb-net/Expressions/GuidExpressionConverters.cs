using System;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class GuidExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<Guid>(
                () => Guid.NewGuid(),
                () => new Term() { type = Term.TermType.UUID });
        }
    }
}

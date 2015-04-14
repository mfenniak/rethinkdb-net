using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class NullableExpressionConverters
    {
        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            expressionConverterFactory.RegisterTemplateMapping<int?, bool>(
                v => v.HasValue,
                v => new Term() {
                    type = Term.TermType.NE,
                    args = {
                        v,
                        new Term() { type = Term.TermType.DATUM, datum = new Datum() { type = Datum.DatumType.R_NULL } }
                    }
                }
            );
        }
    }
}

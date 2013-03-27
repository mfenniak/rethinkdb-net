using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbListQuery : ISingleObjectQuery<string[]>
    {
        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var dbTerm = new Term()
            {
                type = Term.TermType.DB_LIST,
            };
            return dbTerm;
        }
    }
}
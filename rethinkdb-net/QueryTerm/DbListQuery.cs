using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbListQuery : ISingleObjectQuery<string[]>
    {
        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var dbTerm = new Term()
            {
                type = Term.TermType.DB_LIST,
            };
            return dbTerm;
        }
    }
}
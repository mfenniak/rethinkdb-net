using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableListQuery : ISingleObjectQuery<string[]>
    {
        private readonly DbQuery dbTerm;

        public TableListQuery(DbQuery dbTerm)
        {
            this.dbTerm = dbTerm;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_LIST,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm(datumConverterFactory));
            return tableTerm;
        }
    }
}
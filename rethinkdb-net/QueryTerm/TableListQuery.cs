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

        Spec.Term ISingleObjectQuery<string[]>.GenerateTerm()
        {
            var tableTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.TABLE_LIST,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm());
            return tableTerm;
        }
    }
}
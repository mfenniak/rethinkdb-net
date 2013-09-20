using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class IndexListQuery<TTable> : ISequenceQuery<string>
    {
        private readonly ITableQuery<TTable> tableTerm;

        public IndexListQuery(ITableQuery<TTable> tableTerm)
        {
            this.tableTerm = tableTerm;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var indexList = new Term()
            {
                type = Term.TermType.INDEX_LIST,
            };
            indexList.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
            return indexList;
        }
    }
}

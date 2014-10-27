using RethinkDb.Spec;
using System.Linq.Expressions;
using System;

namespace RethinkDb.QueryTerm
{
    public class IndexCreateQuery<TTable, TIndexExpression> : IWriteQuery<DmlResponse>
    {
        private readonly ITableQuery<TTable> tableTerm;
        private readonly string indexName;
        private readonly Expression<Func<TTable, TIndexExpression>> indexExpression;
        private readonly bool multiIndex;

        public IndexCreateQuery(ITableQuery<TTable> tableTerm, string indexName, Expression<Func<TTable, TIndexExpression>> indexExpression, bool multiIndex)
        {
            this.tableTerm = tableTerm;
            this.indexName = indexName;
            this.indexExpression = indexExpression;
            this.multiIndex = multiIndex;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var indexCreate = new Term()
            {
                type = Term.TermType.INDEX_CREATE,
            };
            indexCreate.args.Add(tableTerm.GenerateTerm(queryConverter));
            indexCreate.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = indexName
                },
            });
            indexCreate.args.Add(ExpressionUtils.CreateFunctionTerm(queryConverter, indexExpression));
            if (multiIndex)
            {
                indexCreate.optargs.Add( new Term.AssocPair
                    {
                        key = "multi",
                        val = new Term
                            {
                                type = Term.TermType.DATUM,
                                datum = new Datum
                                    {
                                        type = Datum.DatumType.R_BOOL,
                                        r_bool = this.multiIndex
                                    }
                            }
                    } );
            }
            return indexCreate;
        }
    }
}


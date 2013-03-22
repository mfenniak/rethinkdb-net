using System.Collections.Generic;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableQuery<T> : ISequenceQuery<T>
    {
        private readonly DbQuery dbTerm;
        private readonly string table;
        private readonly bool useOutdated;

        public TableQuery(DbQuery dbTerm, string table, bool useOutdated)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.useOutdated = useOutdated;
        }

        public GetQuery<T> Get(string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(this, primaryKey, primaryAttribute);
        }

        public InsertQuery<T> Insert(T @object, bool upsert = false)
        {
            return new InsertQuery<T>(this, new T[] { @object }, upsert);
        }

        public InsertQuery<T> Insert(IEnumerable<T> @objects, bool upsert = false)
        {
            return new InsertQuery<T>(this, @objects, upsert);
        }

        public DeleteQuery<T> Delete()
        {
            return new DeleteQuery<T>(this);
        }

        public BetweenQuery<T> Between(string leftKey, string rightKey)
        {
            return new BetweenQuery<T>(this, leftKey, rightKey);
        }

        public BetweenQuery<T> Between(double? leftKey, double? rightKey)
        {
            return new BetweenQuery<T>(this, leftKey, rightKey);
        }

        public Term GenerateTerm()
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm());
            tableTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = table,
                    }
                }
            );
            if (useOutdated)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "use_outdated",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = useOutdated,
                        }
                    }
                });
            }
            return tableTerm;
        }
    }
}
using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IQuery
    {
        Spec.Term GenerateTerm();
    }

    public class NullQuery : IQuery
    {
        Spec.Term IQuery.GenerateTerm()
        {
            throw new NotImplementedException();
        }
    }

    public class SequenceQuery : IQuery
    {
        Spec.Term IQuery.GenerateTerm()
        {
            throw new NotImplementedException();
        }
    }

    public class GetQuery : IQuery
    {
        private readonly IQuery tableTerm;
        private readonly string primaryKey;
        private readonly string primaryAttribute;

        public GetQuery(IQuery tableTerm, string primaryKey, string primaryAttribute)
        {
            this.tableTerm = tableTerm;
            this.primaryKey = primaryKey;
            this.primaryAttribute = primaryAttribute;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var getTerm = new Spec.Term() {
                type = Spec.Term.TermType.GET,
            };
            getTerm.args.Add(tableTerm.GenerateTerm());
            getTerm.args.Add(
                new Spec.Term() {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum() {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = primaryKey,
                    }
                }
            );
            if (primaryAttribute != null)
            {
                getTerm.optargs.Add(new Spec.Term.AssocPair() {
                    key = "attribute",
                    val = new Spec.Term() {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum() {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = primaryAttribute,
                        }
                    }
                });
            }
            return getTerm;
        }
    }

    public class TableQuery : IQuery
    {
        private readonly IQuery dbTerm;
        private readonly string table;
        private readonly bool useOutdated;

        public TableQuery(IQuery dbTerm, string table, bool useOutdated)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.useOutdated = useOutdated;
        }

        public GetQuery Get(string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery(this, primaryKey, primaryAttribute);
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var tableTerm = new Spec.Term() {
                type = Spec.Term.TermType.TABLE,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm());
            tableTerm.args.Add(
                new Spec.Term() {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum() {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = table,
                    }
                }
            );
            if (useOutdated)
            {
                tableTerm.optargs.Add(new Spec.Term.AssocPair() {
                    key = "use_outdated",
                    val = new Spec.Term() {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum() {
                            type = Spec.Datum.DatumType.R_BOOL,
                            r_bool = useOutdated,
                        }
                    }
                });
            }
            return tableTerm;
        }
    }

    public class DbQuery : IQuery
    {
        private readonly string db;

        public DbQuery(string db)
        {
            this.db = db;
        }

        public Query TableCreate(string table)
        {
            return null;
        }

        public Query TableDrop(string table)
        {
            return null;
        }

        public Query TableList()
        {
            return null;
        }

        public TableQuery Table(string table, bool useOutdated = false)
        {
            return new TableQuery(this, table, useOutdated);
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var dbTerm = new Spec.Term() {
                type = Spec.Term.TermType.DB,
            };
            dbTerm.args.Add(
                new Spec.Term() {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum() {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = db,
                    }
                }
            );
            return dbTerm;
        }
    }

    public class Query : IQuery
    {
        public static DbQuery Db(string db)
        {
            return new DbQuery(db);
        }

        public static NullQuery DbCreate(string db)
        {
            return null;
        }

        public static NullQuery DbDrop(string db)
        {
            return null;
        }

        public static SequenceQuery DbList()
        {
            return null;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            throw new NotImplementedException();
        }
    }
}

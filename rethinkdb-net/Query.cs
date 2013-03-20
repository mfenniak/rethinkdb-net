using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IQuery
    {
        Spec.Term GenerateTerm();
    }

    public interface ISingleObjectQuery : IQuery
    {
    }

    public interface IDmlQuery : ISingleObjectQuery
    {
    }

    public interface ISequenceQuery : IQuery
    {
    }

    public interface IWriteQuery<T> : IQuery
    {
        Spec.Term GenerateTerm(IDatumConverter<T> converter);
    }

    public class GetQuery : ISingleObjectQuery
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

    public class InsertQuery<T> : IWriteQuery<T>
    {
        private readonly IQuery tableTerm;
        private readonly IEnumerable<T> objects;
        private readonly bool upsert;

        public InsertQuery(IQuery tableTerm, IEnumerable<T> objects, bool upsert)
        {
            this.tableTerm = tableTerm;
            this.objects = objects;
            this.upsert = upsert;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            throw new InvalidOperationException("Use GenerateTerm(IDatumConverter<T>)");
        }

        Spec.Term IWriteQuery<T>.GenerateTerm(IDatumConverter<T> converter)
        {
            var insertTerm = new Spec.Term() {
                type = Spec.Term.TermType.INSERT,
            };
            insertTerm.args.Add(tableTerm.GenerateTerm());

            var objectArray = new Spec.Datum() {
                type = Spec.Datum.DatumType.R_ARRAY,
            };
            foreach (var obj in objects)
            {
                objectArray.r_array.Add(converter.ConvertObject(obj));
            }
            insertTerm.args.Add(new Spec.Term() {
                type = Spec.Term.TermType.DATUM,
                datum = objectArray,
            });

            if (upsert)
            {
                insertTerm.optargs.Add(new Spec.Term.AssocPair() {
                    key = "upsert",
                    val = new Spec.Term() {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum() {
                            type = Spec.Datum.DatumType.R_BOOL,
                            r_bool = upsert,
                        }
                    }
                });
            }

            return insertTerm;
        }
    }

    public class TableQuery : ISequenceQuery
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

        public InsertQuery<T> Insert<T>(T @object, bool upsert = false)
        {
            return new InsertQuery<T>(this, new T[] { @object }, upsert);
        }

        public InsertQuery<T> Insert<T>(T[] @objects, bool upsert = false)
        {
            return new InsertQuery<T>(this, @objects, upsert);
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

    public class TableCreateQuery : IDmlQuery
    {
        private readonly IQuery dbTerm;
        private readonly string table;

        public TableCreateQuery(IQuery dbTerm, string table)
        {
            this.dbTerm = dbTerm;
            this.table = table;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var tableTerm = new Spec.Term() {
                type = Spec.Term.TermType.TABLE_CREATE,
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
            return tableTerm;
        }
    }

    public class TableDropQuery : IDmlQuery
    {
        private readonly IQuery dbTerm;
        private readonly string table;

        public TableDropQuery(IQuery dbTerm, string table)
        {
            this.dbTerm = dbTerm;
            this.table = table;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var tableTerm = new Spec.Term() {
                type = Spec.Term.TermType.TABLE_DROP,
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

        public TableCreateQuery TableCreate(string table)
        {
            return new TableCreateQuery(this, table);
        }

        public TableDropQuery TableDrop(string table)
        {
            return new TableDropQuery(this, table);
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

    public class DbCreateQuery : IDmlQuery
    {
        private readonly string db;

        public DbCreateQuery(string db)
        {
            this.db = db;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var dbTerm = new Spec.Term() {
                type = Spec.Term.TermType.DB_CREATE,
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

    public class DbDropQuery : IDmlQuery
    {
        private readonly string db;

        public DbDropQuery(string db)
        {
            this.db = db;
        }

        Spec.Term IQuery.GenerateTerm()
        {
            var dbTerm = new Spec.Term() {
                type = Spec.Term.TermType.DB_DROP,
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

    public class DbListQuery : ISingleObjectQuery
    {
        Spec.Term IQuery.GenerateTerm()
        {
            var dbTerm = new Spec.Term() {
                type = Spec.Term.TermType.DB_LIST,
            };
            return dbTerm;
        }
    }

    public class Query : IQuery
    {
        public static DbQuery Db(string db)
        {
            return new DbQuery(db);
        }

        public static DbCreateQuery DbCreate(string db)
        {
            return new DbCreateQuery(db);
        }

        public static DbDropQuery DbDrop(string db)
        {
            return new DbDropQuery(db);
        }

        public static DbListQuery DbList()
        {
            return new DbListQuery();
        }

        Spec.Term IQuery.GenerateTerm()
        {
            throw new NotImplementedException();
        }
    }
}

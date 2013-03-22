using System;
using System.Collections.Generic;
using System.ComponentModel;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T>
    {
        Term GenerateTerm();
    }

    [ImmutableObject(true)]
    public interface IDmlQuery : ISingleObjectQuery<DmlResponse>
    {
    }

    [ImmutableObject(true)]
    public interface ISequenceQuery<T>
    {
        Term GenerateTerm();
    }

    [ImmutableObject(true)]
    public interface IWriteQuery<T>
    {
        Term GenerateTerm(IDatumConverter<T> converter);
    }

    public static class Query
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
    }
}

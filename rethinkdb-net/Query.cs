using System;
using System.Collections.Generic;
using System.ComponentModel;
using RethinkDb.QueryTerm;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T>
    {
        Spec.Term GenerateTerm();
    }

    [ImmutableObject(true)]
    public interface IDmlQuery : ISingleObjectQuery<DmlResponse>
    {
    }

    [ImmutableObject(true)]
    public interface ISequenceQuery<T>
    {
        Spec.Term GenerateTerm();
    }

    [ImmutableObject(true)]
    public interface IWriteQuery<T>
    {
        Spec.Term GenerateTerm(IDatumConverter<T> converter);
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

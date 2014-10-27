using System;
using System.Collections.Generic;

namespace RethinkDb
{
    // This interface is used to support $reql_type$=GROUPED_DATA being returned from the server.  In order to use
    // grouping capabilities (eg. .Group() queries), a datum converter must always be registered to read this type
    // from the server.  GroupingDictionaryDatumConverterFactory is provided to support this.
    public interface IGroupingDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
    }
}

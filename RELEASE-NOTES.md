# rethinkdb-net Release Notes

## Unreleased

### Features

* Added support for [DataMember] attributes on properties, whereas previously on fields were supported.  #87.

* Updated order-by API using .OrderBy, .OrderByDescending, .ThenBy, and .ThenByDescending is compatible with LINQ .NET syntax.

* Added Query.Where(), Query.Select(), and Query.Take() alias methods for compatibility with LINQ and IEnumerable<T> extension methods.  

* Added support for nonAtomic flag to Query.Update & Query.Replace. #5.

### Bugfixes

* Fixed incorrect parameterization in Logger.Debug call in Connection.cs; thanks @rvlieshout for the patch.  #105.


## 0.1.2

### Features

* Initial versioned release, see [README](https://github.com/mfenniak/rethinkdb-net/blob/f6bc5c9b499153d7a1a16e9f5bf3a2969742199b/README.md) for initial feature set documentation.

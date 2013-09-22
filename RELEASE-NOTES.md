# rethinkdb-net Release Notes

## Next Release

### API Changes

* RethinkDb.Query now returns and accepts interfaces only for all operations.  This allows a consuming application to never reference `RethinkDb.QueryTerm` namespace unless they want to extend or implement their own query term; it also allows such an extension to work seemlessly with natively implemented operations.  [Issue #134](https://github.com/mfenniak/rethinkdb-net/issues/134)

### Bugfixes

* Treat null and empty the same for datacenter name in table create, secondary index names, and primary key attributes.  Null and empty are both treated as not provided options.  [Issue #131](https://github.com/mfenniak/rethinkdb-net/issues/131)  Thanks to Jonathan Channon (@jchannon) for reporting the issue.

* Fix exception when using anonymous types in tuples that affected Microsoft .NET; primarily this manifested in GroupBy queries not working.  [Issue #133](https://github.com/mfenniak/rethinkdb-net/issues/133)

* Fix connection timeout so that it can timeout during socket connect; previously it could only timeout between endpoint connect attempts.  [Issue #62](https://github.com/mfenniak/rethinkdb-net/issues/62)

### Performance

* Removed usage of reflection in constant & client-side conversions.  [Issue #57](https://github.com/mfenniak/rethinkdb-net/issues/57)

* Removed usage of reflection in array datum converter.  [Issue #56](https://github.com/mfenniak/rethinkdb-net/issues/56)

* Removed usage of reflection from anonymous type datum converter.

### Internals

* `IDatumConverter` and `IDatumConverterFactory` now support non-generic operations.  This is primarily an internal change, but anyone implementing their own datum converters can use the abstract base-classes to be compatible with this change.  [Issue #135](https://github.com/mfenniak/rethinkdb-net/issues/135)

* Converted automated tests to startup RethinkDB on-demand for integration tests.  Thanks to Greg Lincoln (@tetious) for the patch.  [Issue #124](https://github.com/mfenniak/rethinkdb-net/issues/124), [PR #137](https://github.com/mfenniak/rethinkdb-net/pull/137)

* Changed and hopefully improved behavior in cleanup of read dispatcher thread when closing a Connection.  It now dispatches exceptions to any pending queries from that connection, and issues various logging messages depending upon how the dispatcher thread terminated. [Issue #64](https://github.com/mfenniak/rethinkdb-net/issues/64)


## 0.4.2.0

### Bugfixes

* Fix deadlocks when using synchronous API under ASP.NET.  [Issue #130](https://github.com/mfenniak/rethinkdb-net/issues/130)  Thanks to Jonathan Channon (@jchannon) for reporting the issue and providing testing feedback.


## 0.4.1.0

### Minor Changes

* Update protobuf-net dependency to version 2.0.0.666, and specify version dependency in nuspec file.


## 0.4.0.0

### Features

* Support using array Length properties in expressions.  Similar to [Issue #82](https://github.com/mfenniak/rethinkdb-net/issues/82), but not for any IEnumerable yet.

* Support for filter & count operations on enumerables w/ Query.Filter & Query.Count, part of [Issue #82](https://github.com/mfenniak/rethinkdb-net/issues/82); thanks John Weber (@jweber) for the patch.

* Support for appending to arrays with Query.Update, part of [Issue #108](https://github.com/mfenniak/rethinkdb-net/issues/108), and finishes support for the TenMinuteGuide test case.

* New query operations to support the return_vals ReQL option (.UpdateAndReturnValue, .DeleteAndReturnValue, and .ReplaceAndReturnValue), will return the original and updated value during a record update/delete/replace operation.  [Issue #114](https://github.com/mfenniak/rethinkdb-net/issues/114)

* Support directly referencing the parameter in a single-parameter lambda expression, rather than just supporting a member access on the parameter. [Issue #127](https://github.com/mfenniak/rethinkdb-net/issues/127); thanks John Weber (@jweber) for the patch.

### Minor Changes

* Add AuthorizationKey to the IConnection interface. [Issue #125](https://github.com/mfenniak/rethinkdb-net/issues/125)

### Bugfixes

* Allow client-side evaluation of unrecognized expression nodes, like variable, field, or property references.  Not supporting this previously was a huge oversight.  [Issue #122](https://github.com/mfenniak/rethinkdb-net/issues/122)


## 0.3.1.0

### Features

* Upgrade to support RethinkDB version 1.8. [Issue #119](https://github.com/mfenniak/rethinkdb-net/issues/119)

* GroupBy() changed to take an anonymous type as the grouping key, rather than multiple expressions referencing data contract members.  For example:

        Query.GroupBy(
            Query.Avg<Person>(p => p.IQ),
            p => p.Country,
            p => p.Province
        )
    
    becomes
    
        Query.GroupBy(
            Query.Avg<Person>(p => p.IQ),
            p => new { Country = p.Country, Province = p.Province }
        )

* Added support for 'leftBound' and 'rightBound' parameters to Query.Between. [Issue #118](https://github.com/mfenniak/rethinkdb-net/issues/118)

* Converted DateTime and DateTimeOffset datum converters to use RethinkDB 1.8's new pseudotype format for TIME.

* Added support for Query.Now() to retrieve server-side time.

* Added support for serializing System.Uri objects. [Issue #86](https://github.com/mfenniak/rethinkdb-net/issues/86)


### Bugfixes

* Fixed performance issue w/ multithreaded shared connections, which would also cause deadlock in mixed async&sync code.  Thanks to @AshD for reporting the issue.  [PR #112](https://github.com/mfenniak/rethinkdb-net/pull/112)


## 0.2.0.0

### Features

* Added support for [DataMember] attributes on properties, whereas previously on fields were supported.  [PR #87](https://github.com/mfenniak/rethinkdb-net/pull/87)

* Updated order-by API using .OrderBy, .OrderByDescending, .ThenBy, and .ThenByDescending is compatible with LINQ .NET syntax.  Permits queries to be constructed w/ LINQ syntax:

        var query = from o in testTable
            orderby o.SomeNumber, o.Id ascending
            select o;
        foreach (var obj in connection.Run(query))
        {
            ...
        }


* Added Query.Where(), Query.Select(), and Query.Take() alias methods for compatibility with LINQ and IEnumerable<T> extension methods.  Permits queries to be constructed w/ LINQ syntax:

        var query = from n in testTable
            where n.SomeNumber == 3 && n.SomeNumber != 4
            select n.SomeNumber;
        foreach (var obj in connection.Run(query))
        {
            ...
        }

* Added support for nonAtomic flag to Query.Update & Query.Replace. [Issue #5](https://github.com/mfenniak/rethinkdb-net/issues/5)

* Add support for RethinkDB 1.6's basic authentication mechanism.  "authenticationKey" optional parameter can be added to the <cluster> configuration element, or the AuthenticationKey property can be set on the Connection object.  [PR #110](https://github.com/mfenniak/rethinkdb-net/pull/110)

### Bugfixes

* Fixed incorrect parameterization in Logger.Debug call in Connection.cs; thanks @rvlieshout for the patch.  [PR #105](https://github.com/mfenniak/rethinkdb-net/pull/105)


## 0.1.2

### Features

* Initial versioned release, see [README](https://github.com/mfenniak/rethinkdb-net/blob/f6bc5c9b499153d7a1a16e9f5bf3a2969742199b/README.md) for initial feature set documentation.

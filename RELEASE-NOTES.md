# rethinkdb-net Release Notes

## 0.7.0.0 (2013-11-02)

### Features

* (rethinkdb-net-newtonsoft): Introduction of a new assembly, RethinkDb.Newtonsoft.dll, that introduces an alternative to the provided DataContract-based approach to serializing .NET objects to RethinkDB; this new library allows for the use of the same semantics that the Newtonsoft.Json library uses to convert objects.  As the new assembly has dependencies on Newtonsoft.Json that not all users of RethinkDB may desire, we will be distributing this additional assembly as a new NuGet package named rethinkdb-net-newtonsoft.  To use the new library, an alternative implementation of the `ConfigurationAssembler` class has been created in the namespace `RethinkDb.Newtonsoft.Configuration`; using this assembler will create connection factories that reference the new serializer capabilities.  Big thanks to Brian Chavez (@bchavez) for the implementation of this new feature.  [PR #151](https://github.com/mfenniak/rethinkdb-net/issues/151) & [Issue #149](https://github.com/mfenniak/rethinkdb-net/issues/149)

### Bugfixes

* Permit [DataMember(EmitDefaultValue=false)] on non-primitive value types (structs), like System.Guid.  [Issue #142](https://github.com/mfenniak/rethinkdb-net/issues/142).


## 0.6.0.0 (2013-10-21)

### Features

* Add support for Query.HasFields to check if a single record has non-null field values, or to filter a query based upon records having non-null fields. [PR #139](https://github.com/mfenniak/rethinkdb-net/pull/139)

* Added two new forms of connection pooling; one which reduces the need to disconnect and reconnect all the time by using persistent connections, and another which monitors for network disconnects and can retry queries against a new network connection.  [PR #73](https://github.com/mfenniak/rethinkdb-net/issues/73)

* Extended configuration to support the new connection pooling modes, as part of [PR #73](https://github.com/mfenniak/rethinkdb-net/issues/73):

    ```xml
<cluster name="testCluster">
    <defaultLogger enabled="true" category="Warning"/>
    <connectionPool enabled="true"/>
    <networkErrorHandling enabled="true" />
    <endpoints>
        <endpoint address="127.0.0.1" port="55558"/>
    </endpoints>
</cluster>
```

* Support converting enum values to RethinkDB datums with numeric value serialization.  [Issue #143](https://github.com/mfenniak/rethinkdb-net/issues/143).  Thanks to @berlotte for the initial implementation of this feature.

* Support for List&lt;T&gt; and IList&lt;T&gt; serialization and basic filtering.  [Issue #145](https://github.com/mfenniak/rethinkdb-net/issues/145).  Thanks to @berlotte for the initial implementation of this feature.

### API Changes

* Changed how connection factories are created from configuration; previously a connection factory called ConfigConnectionFactory existed, but now a static class called the RethinkDb.Configuration.ConfigurationAssembler will create a connection factory for a specified cluster from the current configuration file.  Refactoring done as part of [pull request #73](https://github.com/mfenniak/rethinkdb-net/issues/73) for connection pooling.  Example code changes:

    Previously:

        connection = RethinkDb.Configuration.ConfigConnectionFactory.Instance.Get("testCluster");

    Now:

        IConnectionFactory connectionFactory = ConfigurationAssembler.CreateConnectionFactory("testCluster");
        connection = connectionFactory.Get();

* Added IDispoable interface to the IConnection interface.  Connections retrieves from a connection factory should be Disposed now, especially is using connection pooling, to return the connections to the pool.  Part of [pull request #73](https://github.com/mfenniak/rethinkdb-net/issues/73).

* Moved methods related to the establishment of connections out of IConnection and into a derived interface, IConnectableConnection.  This reflects the fact that the connection factories will typically return connected connections.  Refactoring done as part of [pull request #73](https://github.com/mfenniak/rethinkdb-net/issues/73) for connection pooling.

* Moved the entire synchronous API, and some redundant default-value style methods, out of IConnection, IConnectableConnection, and IConnectionFactory, and into static extension methods of those interfaces.  This makes it easier to create new implementations of these interfaces with less duplicated code.  Refactoring done as part of [pull request #73](https://github.com/mfenniak/rethinkdb-net/issues/73) for connection pooling.

* Created a base-interface, IScalarQuery&lt;T&gt;, for IWriteQuery and ISingleObjectQuery, allowing the removal of duplicate methods on IConnection & Connection.  Refactoring done as part of [pull request #73](https://github.com/mfenniak/rethinkdb-net/issues/73) for connection pooling.

* Create new namespaces RethinkDb.DatumConverters (for all datum converter) and RethinkDb.Logging (for logging requirements).  This cleans up the RethinkDb namespace and simplifies the API for library users.  [Issue #141](https://github.com/mfenniak/rethinkdb-net/issues/141)

* Removed extension method ReQLExpression.Filter&lt;T&gt;(this T[], Func&lt;T, bool&gt;), and replaced it with support for server-side execution of LINQ's Enumerable&lt;T&gt;(this IEnumerable&lt;T&gt;, Func&lt;T, bool&gt;).  Part of [Issue #145](https://github.com/mfenniak/rethinkdb-net/issues/145).

### Internals

* Better unit test coverage of builtin datum converters.  [Issue #60](https://github.com/mfenniak/rethinkdb-net/issues/60), [PR #140](https://github.com/mfenniak/rethinkdb-net/pull/140)


## 0.5.0.0 (2013-09-22)

### Features

* Add support for Query.Sample to retrieve a uniform random distribution of values from a query.  [Issue #109](https://github.com/mfenniak/rethinkdb-net/issues/109)

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


## 0.4.2.0 (2013-09-17)

### Bugfixes

* Fix deadlocks when using synchronous API under ASP.NET.  [Issue #130](https://github.com/mfenniak/rethinkdb-net/issues/130)  Thanks to Jonathan Channon (@jchannon) for reporting the issue and providing testing feedback.


## 0.4.1.0 (2013-09-17)

### Minor Changes

* Update protobuf-net dependency to version 2.0.0.666, and specify version dependency in nuspec file.


## 0.4.0.0 (2013-09-11)

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


## 0.3.1.0 (2013-08-18)

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


## 0.2.0.0 (2013-06-13)

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


## 0.1.2.0 (2013-05-17)

### Features

* Initial versioned release, see [README](https://github.com/mfenniak/rethinkdb-net/blob/f6bc5c9b499153d7a1a16e9f5bf3a2969742199b/README.md) for initial feature set documentation.

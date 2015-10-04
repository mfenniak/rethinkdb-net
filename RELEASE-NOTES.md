# rethinkdb-net Release Notes

## 0.12.0.0 (2015-10-04)

### Features

* Compound secondary indexes are now supported the IndexDefine API.  This allows you to create a .NET object that represents a RethinkDB index over multiple columns (using ```table.IndexDefine("name", obj => obj.FirstField, obj => obj.SecondField)```), which can then be used to create the index (```conn.Run(index.IndexCreate())```), and query the index (```conn.Run(index.GetAll(index.Key("first field type", 123)))```).  Thanks to @nkreipke for the patch. [PR #229](https://github.com/mfenniak/rethinkdb-net/pull/229) / [PR #235](https://github.com/mfenniak/rethinkdb-net/pull/235)

* Added working Reset function to all query enumerators to reissue the query from the beginning.  [Issues #148](https://github.com/mfenniak/rethinkdb-net/issues/148)

* Added IsEmpty query; ```query.IsEmpty()``` will return true or false if the query has records.  Thanks to @nkreipke for the patch.  [PR #226](https://github.com/mfenniak/rethinkdb-net/pull/226) / [PR #231](https://github.com/mfenniak/rethinkdb-net/pull/231)

* Remove restriction on the array Append operation that prevented adding scalar and array values to arrays in updates; thanks to @nkreipke for the patch.  [PR #227](https://github.com/mfenniak/rethinkdb-net/pull/227) / [PR #232](https://github.com/mfenniak/rethinkdb-net/pull/232) / [PR #228](https://github.com/mfenniak/rethinkdb-net/pull/228 / [PR #233](https://github.com/mfenniak/rethinkdb-net/pull/233)

* Allow the use of ```new ...() {...}``` expressions in the expression tree at locations other than the root; eg. ```table.Update(t => t.Value > 500 ? new Whatever() { Value = t.Value + 100 } : t)``` to do a conditional update.  For this specific example though, a Filter and then an Update is probably more efficient.  Thanks to @nkreipke for the patch.  [PR #230](https://github.com/mfenniak/rethinkdb-net/pull/230 / [PR #237](https://github.com/mfenniak/rethinkdb-net/pull/237)

### Bugfixes

* Fixed [issue #220](https://github.com/mfenniak/rethinkdb-net/issues/220) in [PR #236](https://github.com/mfenniak/rethinkdb-net/pull/236)


## 0.11.0.0 (2015-04-18)

### Features

* Add support for free-form objects or fields in the form of ```Dictionary<string, object>```, or ```Dictionary<string, TValue>``` without the use of rethinkdb-net-newtonsoft, and including a bunch of dictionary-specific functionality. [PR #214](https://github.com/mfenniak/rethinkdb-net/pull/214)

  * ContainsValue method can be used in expressions, particularly filtering and map expression.  eg. ```table.Filter(o => o.Properties.Contains("key"))```
  * Keys and Values properties can be used to retrieve the keys and values of a dictionary as an array.
  * Dictionary accessor can be used to access a key value, eg. ```table.Filter(o => o.Properties["key"] > 5)```
  * New extension method ```SetValue``` on Dictionary can be used to add and update a value to a dictionary during an Update query, eg. ```table.Update(o => new User() { Properties = o.Properties.SetValue("new_key", "new_value") })``` will add or set the field "new_value" on the object "Properties".
  * New extension method ```Without``` on Dictionary can be used to exclude a field from a table query.
  * New automatic logic to determine the most appropriate native type to create when reading a ```Dictionary<string, object>``` from the server.

* Supports RethinkDB's regular expression matching of strings, eg. ```table.Filter(o => o.Email != null && o.Email.Match(".*@(.*)") != null)```.  [Issue #107](https://github.com/mfenniak/rethinkdb-net/issues/107) & [PR #208](https://github.com/mfenniak/rethinkdb-net/pull/208)

* Expressions can now reference member variables of server-side calculated data, eg. ```table.Filter(o => o.Email.Match(".*@(.*)").Groups[0].MatchedString == "google.com")``` would filter for all records with an e-mail address that has a domain of "google.com".  [PR #209](https://github.com/mfenniak/rethinkdb-net/pull/209)

* Added changefeed support for point / single-document changefeed, eg. ```table.Get(...key...).Changes()```.  [Issue #197](https://github.com/mfenniak/rethinkdb-net/issues/197) & [PR #210](https://github.com/mfenniak/rethinkdb-net/pull/210)

* Added changefeed support for ordering by an index and limiting the result set, eg. ```table.OrderBy(...index...).Limit(1)``` will return all changes to the smallest record in the index.  [PR #210](https://github.com/mfenniak/rethinkdb-net/pull/210)

* Added changefeed support for union queries.  [PR #210](https://github.com/mfenniak/rethinkdb-net/pull/210)

* Added changefeed support for min and max queries against an index.  [PR #210](https://github.com/mfenniak/rethinkdb-net/pull/210)

* Added UpdateAndReturnChanges and DeleteAndReturnChanges overloads that take an ISequenceQuery to operate over multiple rows. [PR #212](https://github.com/mfenniak/rethinkdb-net/pull/212)

### Breaking Changes

* Upgraded default RethinkDB protocol to version 0.4, which is only supported on RethinkDB 2.0 and above.  To use rethinkdb-net with an earlier version of RethinkDB, change the connection's Protocol property to ```RethinkDb.Protocols.Version_0_3_Json.Instance```.  [PR #211](https://github.com/mfenniak/rethinkdb-net/pull/211)

* Changed DmlResponse<T>.Changes to return an empty array rather than null if no changes were made. [PR #213](https://github.com/mfenniak/rethinkdb-net/pull/213)


## 0.10.0.0 (2015-04-14)

### Features

* Added support for RethinkDB changefeeds via ```table.Changes()```.  This allows a client application to monitor and receive changes to a RethinkDB table as they happen.  [Issue #180](https://github.com/mfenniak/rethinkdb-net/issues/180)

* Added a "queryTimeout" option to the connectionPool configuration element, which is the time in seconds that each query is allowed to execute before it is timed out from the client.  Thanks to Lucas Jans for the contribution.  [PR #188](https://github.com/mfenniak/rethinkdb-net/pull/188)

* Add support for Enumerable.Any() and Enumerable.Contains() methods to be mapped into a RethinkDB "contains" query term.  [Issue #196](https://github.com/mfenniak/rethinkdb-net/issues/196)

* Add a new "StreamChanges" and "StreamChangesAsync" extension methods to IConnection, which do not use the connection's query timeout when executing server-side commands.  [Issue #193](https://github.com/mfenniak/rethinkdb-net/issues/193)

* Allow the Changes query command to be run against any query that returns a sequence of objects, rather than just a table.  This is supported as-of RethinkDB 1.16.  [Issue #190](https://github.com/mfenniak/rethinkdb-net/issues/190)

* Compatibility with RethinkDB 1.16.  [PR #200](https://github.com/mfenniak/rethinkdb-net/pull/200)

* Compatibility with RethinkDB 2.0.  [PR #207](https://github.com/mfenniak/rethinkdb-net/pull/207)

### Breaking Changes

* RethinkDB 1.16 changed the DML response to use the fields "dbs_created", "dbs_dropped", "tables_created", and "tables_dropped" in preference over the "created" and "dropped" fields.  To support this, RethinkDb.DmlREsponse now has four additional fields that map to these values.  The old fields, Created and Dropped, are still used to report on index operations.  [PR #200](https://github.com/mfenniak/rethinkdb-net/pull/200)

* Fields in DmlResponse class have been changed from type ```double``` to type ```uint``` where they represent the count of things. [PR #205](https://github.com/mfenniak/rethinkdb-net/issues/205)

### Bugfixes

* Fix errors that occur when writing obvious filters like ```... == null``` and ```field.HasValue```.  [Issue #203](https://github.com/mfenniak/rethinkdb-net/issues/203) & [PR #204](https://github.com/mfenniak/rethinkdb-net/issues/204)


## 0.9.1.0 (2014-10-31)

### Features

* Added support for the ternary conditional operator in expressions.  [Issue #47](https://github.com/mfenniak/rethinkdb-net/issues/47)

* Runtime errors in expressions can be added with ReQLExpression.Error() in an expression tree.  [Issue #49](https://github.com/mfenniak/rethinkdb-net/issues/49)

* Now supports most DateTime and DateTimeOffset constructors server-side to create ReQL time objects.  This allows for an operation like ```table.Group(obj => new DateTime(obj.CreatedAt.Year, obj.CreatedAt.Month, obj.CreatedAt.Day))```; this would group all the records in a table by the date they were created. [PR #184](https://github.com/mfenniak/rethinkdb-net/issues/184) & [Issue #154](https://github.com/mfenniak/rethinkdb-net/issues/154)

* Support indexing into a server-side array with [...]; eg. ```table.Map(obj => obj.Array[obj.Array.Length - 1])```.  [Issue #108](https://github.com/mfenniak/rethinkdb-net/issues/108)

### Breaking Changes

* Added support for RethinkDB's binary format for byte[] conversion.  If byte[] was previously used by a client application, reading and writing it will begin using a different and incompatible data format.  Either migrate your data to the new format, or construct a datum converter that doesn't include the new BinaryDatumConverterFactory in it. [Issue #178](https://github.com/mfenniak/rethinkdb-net/issues/178)

* Fixed a bug in how DateTimeOffset objects are serialized to and from RethinkDB; they previously assumed the offset was incorporated into the epoch_time, where that was incorrect.  This may change data being retrieved or stored with this data type.  [PR #184](https://github.com/mfenniak/rethinkdb-net/issues/184)


## 0.9.0.0 (2014-10-26)

### Features

* The type-safe object model for secondary indexes has been expanded to incldude multi-indexes.  Calling table.IndexDefineMulti will return an IMultiIndex<TRecord, TIndexType> interface that can be used in multi-index operations, such as GetAll, Between, and EqJoin.  [PR #174](https://github.com/mfenniak/rethinkdb-net/issues/174)

* It's now possible to customize and configure how rethinkdb-net converts expression trees to RethinkDB terms by creating a DefaultExpressionConverterFactory and calling Register...() methods on it to configure how operators, method calls, and member accesses are converted into RethinkDB terms.  The customized expression converter factory can then be used by setting the QueryConverter property of a Connection.  [PR #183](https://github.com/mfenniak/rethinkdb-net/issues/183)

* Hard-coded types in query operators are optionally provided by rethinkdb-net, but can be input instead.  For example, we provide Query.Now() that returns a DateTimeOffset; if you'd prefer DateTime or some other type, you can use Query.Now<YourType>().  As long as the datum converter on your connection can convert the results from the server into YourType, the query will work as you'd expect.  [PR #183](https://github.com/mfenniak/rethinkdb-net/issues/183)

* TimeSpan constructors are now supported in expressions (new TimeSpan(...), and TimeSpan.From[Days/Hours/Minutes/Seconds/Milliseconds/Ticks]) allowing for queries like the below.  [Issue #153](https://github.com/mfenniak/rethinkdb-net/issues/153)

  ```C#
  table.Filter(r => r.CreatedAt + TimeSpan.FromDays(r.ExpireInDays) < Query.Now())
  ```

* Server-side GUID generation is now supported; eg. ```table.Update(record => new Record() { Id = Guid.NewGuid() })``` will actually generate unique guids for all updated records in table, rather than evaluating client-side to a single value.  [Issue #182](https://github.com/mfenniak/rethinkdb-net/issues/182)

* Accessors to common properties on DateTime and DateTimeOffset can now be performed server-side.  For example, the query ```table.Filter(record => record.CreatedAt.Year == 2014)``` is now possible.  [Issue #120](https://github.com/mfenniak/rethinkdb-net/issues/120)

* DateTime.UtcNow and DateTimeOffset.UtcNow are executed server-side when accessed inside an expression tree. [Issue #120](https://github.com/mfenniak/rethinkdb-net/issues/120)

### Compatibility

* Added support for RethinkDB's JSON-based client driver protocol.  The JSON protocol is now the default protocol, but the protocol to be used is configurable on the connection objects.  [PR #176](https://github.com/mfenniak/rethinkdb-net/issues/176)

### Breaking Changes

* Updated Insert command to reflect RethinkDB's deprecation of upsert and addition of the new conflict parameter. [PR #177](https://github.com/mfenniak/rethinkdb-net/issues/177)

* IDatumConverterFactory has been replaced with IQueryConverter in APIs where the requirement is a tool to convert client-side queries into RethinkDB terms.  IQueryConverter is a composed interface containing IDatumConverterFactory and IExpressionConverterFactory.   [PR #183](https://github.com/mfenniak/rethinkdb-net/issues/183)


## 0.8.0.0 (2014-10-20)

### Features

* Upgrade to support RethinkDB version 1.15. [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173) & [Issue #171](https://github.com/mfenniak/rethinkdb-net/issues/171)

  * New Group method can be used for grouping on an index value, or between 1 and 3 different key values.

  * Count aggregate now supports a predicate for counting only matching rows.

  * Max, Min, Avg, Sum, Count, and Contains aggregates are now fully supported.  Previously only Avg and Sum aggregates were supported.

* Support for serializing and deserializing TimeSpan data types, which was added to the Newtonsoft serializer but not the basic serialization implementation. [PR #152](https://github.com/mfenniak/rethinkdb-net/issues/152)

* Expressions now support the addition of DateTime and TimeSpan objects, as well as DateTime and DateTimeOffset's Add methods (eg. AddHours, AddDays).  [PR #152](https://github.com/mfenniak/rethinkdb-net/issues/152), [Issue #158](https://github.com/mfenniak/rethinkdb-net/issues/158)  Note, AddMonths is not supported.

* Support for multi-index creation.  [Issue #160](https://github.com/mfenniak/rethinkdb-net/issues/160) & [PR #161](https://github.com/mfenniak/rethinkdb-net/issues/161)

* Support for OrderBy on indexes.  [Issue #162](https://github.com/mfenniak/rethinkdb-net/issues/162)

* A type-safe object model has been added for secondary indexes.  Calling table.IndexDefine("index-name", o => o.IndexedField) will return an IIndex<TRecord, TIndexType> interface.  This object can be used to create or drop the index (.IndexCreate, .IndexDrop), but more importantly provides type-consistency for operations that can use the index, such as .GetAll, .Between, .OrderBy, .EqJoin, and .Group.  It also removes the need for explicitly specifying the index type in the generic .Group method.  [Issue #163](https://github.com/mfenniak/rethinkdb-net/issues/163).

### Breaking Changes

* [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173) contained a number of breaking changes to maintain consistency with RethinkDB driver changes on other platforms and remove functionality that is no longer supported by RethinkDB.

  * Remove base parameter from Reduce(); it's been removed in RethinkDB and instead an error occurs when attempting to reduce an empty sequence, and the only element is returned when reducing a single-element sequence.  Part of [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173).

  * UpdateAndReturnValues, InsertAndReturnValues, and DeleteAndReturnValues have all been renamed to "...ReturnChanges", and their return value has changed to support returning multiple changes.  These changes are for compatibility and to maintain consistency with other RethinkDB drivers.  Part of [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173).

  * GroupedMapReduce has been removed for consistency with other RethinkDB drivers.  .Group(...).Map(...).Reduce(...) can be used as an alternative.  Part of [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173).

  * GroupBy and its prebuilt aggregates have been removed for consistency with other RethinkDB drivers.  .Group() followed by an aggregate can be used instead.  Part of [PR #173](https://github.com/mfenniak/rethinkdb-net/issues/173).

* Explicitly supplying 'null' for the query method GetAll's indexName parameter (eg. .GetAll("3", null)) is now ambiguous between a "string" and "IIndex" second parameter; to resolve this, disambiguate the function call with an explicit cast (eg. .GetAll("3", (string)null)).  [Issue #163](https://github.com/mfenniak/rethinkdb-net/issues/163).

* OrderBy and OrderByDescending overloads that took both a memberReferenceExpression and an indexName were removed.  The index always has priority, so rewrite such queries to have an OrderBy the index, and .ThenBy() the member field if required.  [Issue #163](https://github.com/mfenniak/rethinkdb-net/issues/163).


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

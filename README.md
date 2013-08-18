<a href="http://teamcity.codebetter.com/viewType.html?buildTypeId=bt991&guest=1"><img src="http://teamcity.codebetter.com/app/rest/builds/buildType:(id:bt991)/statusIcon" alt="RethinkDB-Net Build Status"/></a>

This is a prototype of a RethinkDB client driver written in C# for the .NET platform.  This driver utilizes .NET 4.5 and C# 5.0.

Currently this driver is capable of the following things:
  
  * Connecting to a RethinkDB database. For example:
    ```
    IConnection connection;
    connection = ConfigConnectionFactory.Instance.Get("testCluster");
    connection.Logger = new DefaultLogger(LoggingCategory.Debug, Console.Out);
      
    await connection.ConnectAsync();      
    ```

  * A decent number of RethinkDB's queries, joins, transformations, aggregations, and reductions:

    * DbList
    * DbCreate
    * DbDrop
    * TableList
    * TableCreate
    * TableDrop
    * Table
    * Get
    * Between
    * Filter
    * Update
    * Delete
    * Replace
    * Count
    * OrderBy
    * Skip
    * Limit
    * Slice
    * Nth
    * InnerJoin
    * OuterJoin
    * EqJoin
    * Zip
    * Distinct
    * Union
    * Map
    * ConcatMap
    * Reduce
    * GroupBy
    * GroupedMapReduce

  * Filter, Update, Inner/Outer/EqJoin, Map, Reduce, etc. can be built using C# expressions (with limitations) that are compile-time safe, and are automatically translated into RethinkDB's query language.  For example, `Query.Db("db").Table<ObjectDefinition>("objects").Update(o => new ObjectDefinition { Name = o.Name + " (new name!)" })`.

  * Converting data into objects and objects into data; non-primitive objects are marked up using [DataContract] and [DataMember] attributes similar to WCF data contracts.

  * References to object attributes are compile-time verified by using C# expression trees.  For example, `Query.Db("db").Table<ObjectDefinition>("objects").OrderBy(o => o.Name)`.  Applies to OrderBy, EqJoin, and GroupBy.

  * Anonymous types can be used in functions like Map and Reduce.  For example:
    ```
    Query
      .Db("db")
      .Table<ObjectDefinition>("objects")
      .Map(od => new { Sum = od.Value, Count = 1.0 })
      .Reduce((left, right) => new { Sum = left.Sum + right.Sum, Count = left.Count + right.Count })
    ```

  * Strong compile-time safety using generics.  Every query operation knows what type it returns, and whether it returns an object or an enumerable.

  * Performing absolutely everything asynchronously, with a synchronous API too if desired.

  * Reading streaming / chunked datasets using an async iterator.

  * Being 100% compatible with Mono (3.0+).

  * Support for serialized object types with the provided DataContract-based datum converter classes (all .NET primitives supported)

For examples of usage, you can look at the unit tests in https://github.com/mfenniak/rethinkdb-net/tree/master/rethinkdb-net-test/Integration/Documentation.  These tests are C# versions of the code snippets from RethinkDB's official documentation.

Currently this driver is lacking in the following areas:

  * Does not support schema-free / free-format objects.  Although the object conversion routines are interfaced out so that they can be replaced with something different as required, the first goal is to provide a client that works well in a native C# environment, and that implies type safety and structure.

  * Supporting free-format objects will require alternative interfaces to OrderBy, EqJoin, and GroupBy, all of which use expression trees for attribute references.

  * RethinkDB manipulations (pluck / merge / without / append / contains) and control structures (branch / forEach / error / coerceTo / typeOf) are not currently supported.  Some manipulations (pluck / merge / without / append) are maybe not really compatible with the strong typing approach this library currently supports.

  * Documentation.  You're reaching the end of it, and it probably hasn't helped you at all.


I welcome pull requests.  This is just the start of a RethinkDB client for .NET.  It's nowhere near the end.

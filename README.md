rethinkdb-net is a RethinkDB client driver written in C# for the .NET platform.  This driver utilizes .NET 4.5 and C# 5.0.

[![Circle CI](https://circleci.com/gh/mfenniak/rethinkdb-net.svg?style=svg)](https://circleci.com/gh/mfenniak/rethinkdb-net)
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/mfenniak/rethinkdb-net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Tiny Example
------------

Main.cs:

```c#
using System;
using System.Linq;
using System.Runtime.Serialization;
using RethinkDb;
using RethinkDb.Configuration;

[DataContract]
public class Person
{
    public static IDatabaseQuery Db = Query.Db("test");
    public static ITableQuery<Person> Table = Db.Table<Person>("people");

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public Guid Id;

    [DataMember]
    public string Name;
}

public static class MainClass
{
    private static IConnectionFactory connectionFactory =
        ConfigurationAssembler.CreateConnectionFactory("example");

    public static void Main(string[] args)
    {
        var conn = connectionFactory.Get();

        // Create DB if needed
        if (!conn.Run(Query.DbList()).Contains("test"))
            conn.Run(Query.DbCreate("test"));

        // Create table if needed
        if (!conn.Run(Person.Db.TableList()).Contains("people"))
            conn.Run(Person.Db.TableCreate("people"));

        // Read all the contents of the table
        foreach (var person in conn.Run(Person.Table))
            Console.WriteLine("Id: {0}, Name: {1}", person.Id, person.Name);

        // Insert a new record
        conn.Run(Person.Table.Insert(new Person() { Name = "Jack Black" }));
    }
}
```

App.config:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <section name="rethinkdb" type="RethinkDb.Configuration.RethinkDbClientSection, RethinkDb"/>
    </configSections>
    <rethinkdb>
        <clusters>
            <cluster name="example">
                <defaultLogger enabled="true" category="Warning"/>
                <connectionPool enabled="true"/>
                <networkErrorHandling enabled="true" />
                <endpoints>
                    <endpoint address="127.0.0.1" port="28015"/>
                </endpoints>
            </cluster>
        </clusters>
    </rethinkdb>
</configuration>
```

Capabilities
------------

Currently this driver is capable of the following things:
  
  * Connecting to a RethinkDB database.

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
    * Group

  * Filter, Update, Inner/Outer/EqJoin, Map, Reduce, etc. can be built using C# expressions (with limitations) that are compile-time safe, and are automatically translated into RethinkDB's query language.  For example, `Query.Db("db").Table<ObjectDefinition>("objects").Update(o => new ObjectDefinition { Name = o.Name + " (new name!)" })`.

  * Converting data into objects and objects into data; non-primitive objects are marked up using [DataContract] and [DataMember] attributes similar to WCF data contracts.

  * Alternatively, the RethinkDb driver also supports [Newtonsoft Json.NET](https://github.com/mfenniak/rethinkdb-net/wiki/Newtonsoft-Serialization) object serialization.

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

  * Support for schema-free tables, or sub-sections of objects, by using ```Dictionary<string, object>``` types.  While you lose some of the the type-safety this library provides by making use of this functionality, it is incredibly powerful to have objects that are composed of some strongly-formed data and some loosely-formed data in this manner.

Currently this project is really lacking in documentation.  For examples of usage, you can look at the unit tests in [Integration Documentation](https://github.com/mfenniak/rethinkdb-net/tree/master/rethinkdb-net-test/Integration/Documentation).  These tests are C# versions of the code snippets from RethinkDB's official documentation.  There are also a couple small example programs in the Examples directory.

I welcome pull requests.  This is just the start of a RethinkDB client for .NET.  It's nowhere near the end.

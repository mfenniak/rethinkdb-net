This is a prototype of a RethinkDB client driver written in C# for the .NET platform.  This driver utilizes .NET 4.5 and C# 5.0.


Currently this driver is capable of the following things:
  
  * Connecting to a RethinkDB database.
  
  * Executing a handful of read queries:
    * Query.DbList -- lists databases
    * Query.Db().Table() -- reads an entire table
    * Query.Db().Table().Get() -- reads a single object by primary key

  * Executing a handful of modifying queries:
    * Query.DbCreate()
    * Query.DbDrop()
    * Query.Db().TableCreate()
    * Query.Db().TableDrop()
    * Query.Db().Table().Insert()

  * Reading streaming / chunked datasets (only applies to Query.Db().Table()).

  * Converting data into objects and objects into data; non-primtiive objects are marked up using [DataContract] and [DataMember] attributes similar to WCF data contracts, and only object fields are supported.

  * Performing absolutely everything asynchronously.

  * Being 100% compatible with Mono (3.0+).

Currently this driver is lacking in the following areas:

  * Does not support schema-free / free-format objects.  Although, the object conversion routines are interfaced out so that they can be replaced with something better / different as required.

  * Limited support for serialized object types with the providedDataContract-based datum converter classes.  No "int"s, for example; only doubles.

  * Does not do very many ReQL query operations, like, say, updates... or filtering.

  * Does not have a synchronous interface for performing actions.  Generally this would be easy to add on top of the asynchronous API.

  * Connection objects are not currently safe for concurrent access.  I know!  Ridiculous!  Why create an async interface for something that's not concurrent anyways?  Because this is an issue that can be fixed.  I think.

  * Documentation.  You're reaching the end of it, and it probably hasn't helped you at all.


I welcome pull requests.  This is just the start of a RethinkDB client for .NET.  It's nowhere near the end.

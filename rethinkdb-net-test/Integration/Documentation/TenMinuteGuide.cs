using System;
using RethinkDb.Test.Integration;
using NUnit.Framework;

namespace RethinkDb.Test.Integration.Documentation
{
    // C# implementation of queries documented in http://www.rethinkdb.com/docs/guide/python/
    [TestFixture]
    public class TenMinuteGuide : TestBase
    {
        [Test]
        public void QuerySequence()
        {
            {
                var res = connection.Run(Query.DbCreate("test"));
                Assert.That(res.Created, Is.EqualTo(1));
            }

            // Create a new table
            {
                var res = connection.Run(Query.Db("test").TableCreate("authors"));
                Assert.That(res.Created, Is.EqualTo(1));
            }

            var table = Query.Db("test").Table<Author>("authors");
            string pk1;

            // Insert data
            {
                var res =  connection.Run(table.Insert(
                    new Author[] {
                        new Author {
                            Name = "William Adama",
                            TVShow = "Battlestar Galactica",
                            Posts = new Post[] {
                                new Post { Title = "Decommissioning speech", Content = "The Cylon War is long over..." },
                                new Post { Title = "We are at war", Content = "Moments ago, this ship received..." },
                                new Post { Title = "The new Earth", Content = "The discoveries of the past few days..." }
                            }
                        },
                        new Author {
                            Name = "Laura Roslin",
                            TVShow = "Battlestar Galactica",
                            Posts = new Post[] {
                                new Post { Title = "The oath of office", Content = "I, Laura Roslin, ..." },
                                new Post { Title = "They look like us", Content = "The Cylons have the ability..." },
                            }
                        },
                        new Author {
                            Name = "Jean-Luc Picard",
                            TVShow = "Star Trek TNG",
                            Posts = new Post[] {
                                new Post { Title = "Civil rights", Content = "There are some words I've known since..." },
                            }
                        },
                    }
                ));
                Assert.That(res.Inserted, Is.EqualTo(3));
                Assert.That(res.GeneratedKeys, Has.Length.EqualTo(3));
                pk1 = res.GeneratedKeys[0];
            }

            // All documents in a table
            {
                int count = 0;
                foreach (var rec in connection.Run(table))
                    ++count;
                Assert.That(count, Is.EqualTo(3));
            }

            // Filter documents based on a condition
            {
                int count = 0;
                foreach (var rec in connection.Run(table.Filter(r => r.Name == "William Adama")))
                {
                    Assert.That(rec.Name, Is.EqualTo("William Adama"));
                    ++count;
                }
                Assert.That(count, Is.EqualTo(1));

                count = 0;
                foreach (var rec in connection.Run(table.Filter(r => r.Posts.Length > 2)))
                {
                    Assert.That(rec.Name, Is.EqualTo("William Adama"));
                    ++count;
                }
                Assert.That(count, Is.EqualTo(1));
            }

            // Retrieve documents by primary key
            {
                var rec = connection.Run(table.Get(pk1));
                Assert.That(rec, Is.Not.Null);
            }

            // Update documents
            {
                var res = connection.Run(table.Update(r => new Author() { Type = "fictional" }));
                Assert.That(res.Replaced, Is.EqualTo(3));

                res = connection.Run(
                    table.Filter(r => r.Name == "William Adama").Update(r => new Author() { Rank = "Admiral" }));
                Assert.That(res.Replaced, Is.EqualTo(1));

                // Issue #108
                res = connection.Run(
                    table
                    .Filter(r => r.Name == "Jean-Luc Picard")
                    .Update(r => new Author() {
                        Posts = r.Posts.Append(new Post { Title = "Shakespear", Content = "What a piece of work is man..." })
                    })
                );
                Assert.That(res.Replaced, Is.EqualTo(1));
            }

            // Delete documents
            {
                var res = connection.Run(table.Filter(r => r.Posts.Length < 3).Delete());
                Assert.That(res.Deleted, Is.EqualTo(2));
            }
        }
    }
}


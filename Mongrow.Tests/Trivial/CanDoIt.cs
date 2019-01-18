using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Mongrow.Steps;
using NUnit.Framework;
using Testy;
#pragma warning disable 1998

namespace Mongrow.Tests.Trivial
{
    [TestFixture]
    public class CanDoIt : FixtureBase
    {
        [Test]
        public void CanRunSingleMigration()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var migrator = new Migrator(database, new[] { new InsertSingleDocument() });

            migrator.Execute();

            var docs = database.GetCollection<BsonDocument>("docs").Find(d => true).ToList();
            Assert.That(docs.Count, Is.EqualTo(1), "Expected a single document to have been inserted");
            Assert.That(docs.First()["what"].AsString, Is.EqualTo("text"));
        }

        [Test]
        public void RunningSameMigrationTwiceDoesNotFail()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var migrator = new Migrator(database, new[] { new InsertSingleDocument() });
            migrator.Execute();

            migrator.Execute();

            var docs = database.GetCollection<BsonDocument>("docs").Find(d => true).ToList();
            Assert.That(docs.Count, Is.EqualTo(1), "Expected a single document to have been inserted");
            Assert.That(docs.First()["what"].AsString, Is.EqualTo("text"));
        }

        [Test]
        public void AddingMigrationWithoutStepAttributeFails()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var steps = new IStep[]
            {
                new InsertSingleDocument(),
                new DoesNotHaveTheAttribute()
            };

            var exception = Assert.Throws<ArgumentException>(() => new Migrator(database, steps));

            Console.WriteLine(exception);
        }

        [Test]
        public void AddingTwoMigrationsWithSameIdFails()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var steps = new IStep[]
            {
                new InsertSingleDocument(),
                new HasSameIdAsTheOtherOne()
            };

            var exception = Assert.Throws<ArgumentException>(() => new Migrator(database, steps));

            Console.WriteLine(exception);
        }

        [Step(1)]
        class InsertSingleDocument : IStep
        {
            public async Task Execute(IMongoDatabase database)
            {
                await database.GetCollection<BsonDocument>("docs").InsertOneAsync(new BsonDocument
                {
                    { "what", "text" }
                });
            }
        }

        [Step(1)]
        class HasSameIdAsTheOtherOne : IStep
        {
            public async Task Execute(IMongoDatabase database)
            {
            }
        }

        class DoesNotHaveTheAttribute : IStep
        {
            public async Task Execute(IMongoDatabase database)
            {
            }
        }
    }
}

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
    public class BasicStuff : FixtureBase
    {
        static readonly Options DefaultOptions = new Options(logAction: Console.WriteLine, verboseLogAction: Console.WriteLine);

        [Test]
        public void FailsIfWeTryToAddMigrationThatShouldHaveBeenExecutedBefore()
        {
            var database = MongoTest.GetCleanTestDatabase();

            var step2 = new InterleavingMigration2();
            var fullListOfSteps = new IStep[]
            {
                new InterleavingMigration1(),
                step2,
                new InterleavingMigration3(),
            };

            var listMissingMiddleStep = fullListOfSteps.Except(new[] { step2 });

            new Migrator(database, listMissingMiddleStep, DefaultOptions).Execute();

            var exception = Assert.Throws<ArgumentException>(() => new Migrator(database, fullListOfSteps, DefaultOptions).Execute());

            Console.WriteLine(exception);
        }

        [Step(1)] class InterleavingMigration1 : IStep { public async Task Execute(IMongoDatabase database, ILog log) { } }
        [Step(2)] class InterleavingMigration2 : IStep { public async Task Execute(IMongoDatabase database, ILog log) { } }
        [Step(3)] class InterleavingMigration3 : IStep { public async Task Execute(IMongoDatabase database, ILog log) { } }

        [Test]
        public void CanRunSingleMigration()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var migrator = new Migrator(database, new[] { new InsertSingleDocument() }, DefaultOptions);

            migrator.Execute();

            var docs = database.GetCollection<BsonDocument>("docs").Find(d => true).ToList();
            Assert.That(docs.Count, Is.EqualTo(1), "Expected a single document to have been inserted");
            Assert.That(docs.First()["what"].AsString, Is.EqualTo("text"));
        }

        [Test]
        public void RunningSameMigrationTwiceDoesNotFail()
        {
            var database = MongoTest.GetCleanTestDatabase();
            var migrator = new Migrator(database, new[] { new InsertSingleDocument() }, DefaultOptions);
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

            var exception = Assert.Throws<ArgumentException>(() => new Migrator(database, steps, DefaultOptions));

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

            var exception = Assert.Throws<ArgumentException>(() => new Migrator(database, steps, DefaultOptions));

            Console.WriteLine(exception);
        }

        [Step(1, Decription = "Just inserts into 'docs' a doc with 'what'='text'")]
        class InsertSingleDocument : IStep
        {
            public async Task Execute(IMongoDatabase database, ILog log)
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
            public async Task Execute(IMongoDatabase database, ILog log)
            {
            }
        }

        class DoesNotHaveTheAttribute : IStep
        {
            public async Task Execute(IMongoDatabase database, ILog log)
            {
            }
        }
    }
}

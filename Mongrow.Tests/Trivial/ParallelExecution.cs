using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongrow.Steps;
using NUnit.Framework;
using Testy;

namespace Mongrow.Tests.Trivial
{
    [TestFixture]
    public class ParallelExecution : FixtureBase
    {
        Migrator _migrator;
        IMongoDatabase _database;

        protected override void SetUp()
        {
            _database = MongoTest.GetCleanTestDatabase();
            _migrator = new Migrator(_database, new IStep[]
            {
                new Migr1(),
                new Migr2(),
                new Migr3(),
                new Migr4(),
                new Migr5(),
                new Migr6(),
                new Migr7(),
                new Migr8(),
                new Migr9(),
                new Migr10(),
                new Migr11(),
                new Migr12(),
                new Migr13(),
                new Migr14(),
                new Migr15(),
                new Migr16(),
                new Migr17(),
                new Migr18(),
                new Migr19(),
                new Migr20(),
            }, new Options(logAction: Console.WriteLine));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public async Task TryThis(int parallelism)
        {
            await Task.WhenAll(Enumerable.Range(0, parallelism)
                .Select(async n =>
                {
                    Console.WriteLine($"Starting migration {n}...");
                    await _migrator.ExecuteAsync();
                    Console.WriteLine($"Migration {n} is done!");
                }));

            var collection = _database.GetCollection<Execution>("executions");

            var executions = await collection.Find(e => true).ToListAsync();

            Assert.That(executions.Count, Is.EqualTo(20));

            var actualExecutions = executions.OrderBy(e => e.Sequence).Select(e => new { e.Number, e.BranchSpec }).ToArray();

            var expectedExecutions = new[]
            {
                new{Number = 1, BranchSpec = "master"},
                new{Number = 2, BranchSpec = "master"},
                new{Number = 3, BranchSpec = "master"},
                new{Number = 4, BranchSpec = "master"},
                new{Number = 5, BranchSpec = "master"},
                new{Number = 6, BranchSpec = "master"},
                new{Number = 7, BranchSpec = "master"},
                new{Number = 8, BranchSpec = "master"},
                new{Number = 9, BranchSpec = "master"},
                new{Number = 10, BranchSpec = "master"},
                new{Number = 11, BranchSpec = "master"},
                new{Number = 12, BranchSpec = "master"},
                new{Number = 13, BranchSpec = "master"},
                new{Number = 14, BranchSpec = "master"},
                new{Number = 15, BranchSpec = "master"},
                new{Number = 16, BranchSpec = "master"},
                new{Number = 17, BranchSpec = "master"},
                new{Number = 18, BranchSpec = "master"},
                new{Number = 19, BranchSpec = "master"},
                new{Number = 20, BranchSpec = "master"},
            };

            Assert.That(actualExecutions, Is.EqualTo(expectedExecutions));
        }

        abstract class GenericStep : IStep
        {
            static long _sequenceNumber;

            public async Task Execute(IMongoDatabase database, ILog log)
            {
                var number = (int)Interlocked.Increment(ref _sequenceNumber);
                var attribute = GetType().GetCustomAttributes(true).OfType<StepAttribute>().First();

                await database.GetCollection<Execution>("executions").InsertOneAsync(new Execution
                {
                    Id = Guid.NewGuid().ToString(),
                    Number = attribute.Number,
                    BranchSpec = attribute.BranchSpec,
                    Sequence = number
                });
            }
        }

        [Step(1)] class Migr1 : GenericStep { }
        [Step(2)] class Migr2 : GenericStep { }
        [Step(3)] class Migr3 : GenericStep { }
        [Step(4)] class Migr4 : GenericStep { }
        [Step(5)] class Migr5 : GenericStep { }
        [Step(6)] class Migr6 : GenericStep { }
        [Step(7)] class Migr7 : GenericStep { }
        [Step(8)] class Migr8 : GenericStep { }
        [Step(9)] class Migr9 : GenericStep { }
        [Step(10)] class Migr10 : GenericStep { }
        [Step(11)] class Migr11 : GenericStep { }
        [Step(12)] class Migr12 : GenericStep { }
        [Step(13)] class Migr13 : GenericStep { }
        [Step(14)] class Migr14 : GenericStep { }
        [Step(15)] class Migr15 : GenericStep { }
        [Step(16)] class Migr16 : GenericStep { }
        [Step(17)] class Migr17 : GenericStep { }
        [Step(18)] class Migr18 : GenericStep { }
        [Step(19)] class Migr19 : GenericStep { }
        [Step(20)] class Migr20 : GenericStep { }

        class Execution
        {
            public string Id { get; set; }
            public int Number { get; set; }
            public string BranchSpec { get; set; }
            public int Sequence { get; set; }
        }

        [Test]
        public void DoesNotMessUp()
        {

        }
    }
}
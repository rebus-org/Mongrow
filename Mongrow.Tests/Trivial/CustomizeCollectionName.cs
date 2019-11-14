using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongrow.Steps;
using NUnit.Framework;
using Testy;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Mongrow.Tests.Trivial
{
    [TestFixture]
    public class CustomizeCollectionName : FixtureBase
    {
        IMongoDatabase _database;

        protected override void SetUp()
        {
            _database = MongoTest.GetCleanTestDatabase();
        }

        [Test]
        public async Task CanCustomizeCollectionName()
        {
            var randomCollectionName = Guid.NewGuid().ToString("N");
            var randomLockCollectionName = Guid.NewGuid().ToString("N");
            var options = new Options(collectionName: randomCollectionName, lockCollectionName: randomLockCollectionName);
            var migrator = new Migrator(_database, new[] { new DummyStep() }, options);

            await migrator.ExecuteAsync();

            var collectionNames = await GetCollectionNames(_database);

            Console.WriteLine($@"Found the following collections in the current database:

{string.Join(Environment.NewLine, collectionNames.Select(name => name.PadLeft(4)))}");

            Assert.That(collectionNames.OrderBy(name => name), Is.EqualTo(new[] { randomCollectionName, randomLockCollectionName }.OrderBy(name => name)));
        }

        async Task<IReadOnlyCollection<string>> GetCollectionNames(IMongoDatabase database)
        {
            var names = new List<string>();

            using (var cursor = await database.ListCollectionNamesAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    names.AddRange(cursor.Current);
                }
            }

            return names;
        }

        [Step(1)]
        class DummyStep : IStep
        {
            public async Task Execute(IMongoDatabase database, ILog log, CancellationToken cancellationToken)
            {

            }
        }
    }
}
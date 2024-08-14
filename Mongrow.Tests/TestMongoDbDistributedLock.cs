using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongrow.Internals;
using NUnit.Framework;
using Testy;

namespace Mongrow.Tests;

[TestFixture]
public class TestMongoDbDistributedLock : FixtureBase
{
    IMongoDatabase _database;

    protected override void SetUp()
    {
        base.SetUp();

        _database = MongoTest.GetCleanTestDatabase();
    }

    [Test]
    public async Task CanAcquireAndRelease()
    {
        var lockId = Guid.NewGuid().ToString("n");

        using var @lock = new MongoDbDistributedLock(
            database: _database,
            lockId: lockId,
            description: "this is my lock",
            collectionName: "Locks"
        );

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await @lock.AcquireAsync();
        }
        finally
        {
            await @lock.ReleaseAsync();
        }

        try
        {
            await @lock.AcquireAsync();
        }
        finally
        {
            await @lock.ReleaseAsync();
        }

        var elapsed = stopwatch.Elapsed;

        Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(5)), 
            "Two acquire/release cycles should not take longer than 5 s to execute");
    }
}
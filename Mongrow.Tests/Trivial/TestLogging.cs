using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongrow.Steps;
using NUnit.Framework;

namespace Mongrow.Tests.Trivial
{
    [TestFixture]
    public class TestLogging
    {
        [Test]
        public void CanLogFromStep()
        {
            var writtenLogs = new ConcurrentQueue<string>();

            var options = new Options(
                logAction: text => writtenLogs.Enqueue($"INF: {text}"),
                verboseLogAction: text => writtenLogs.Enqueue($"VBS: {text}")
            );

            new Migrator(MongoTest.GetCleanTestDatabase(), new[] { new TestStep() }, options).Execute();

            Console.WriteLine(string.Join(Environment.NewLine, writtenLogs));

            Assert.That(writtenLogs, Contains.Item("INF: YAY THIS IS INFO"));
            Assert.That(writtenLogs, Contains.Item("VBS: YAY THIS IS VERBOSE"));
        }

        [Step(1)]
        class TestStep : IStep
        {
            public async Task Execute(IMongoDatabase database, ILog log)
            {
                log.Write("YAY THIS IS INFO");
                log.WriteVerbose("YAY THIS IS VERBOSE");
            }
        }
    }
}
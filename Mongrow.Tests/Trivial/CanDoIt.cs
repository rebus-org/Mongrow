using NUnit.Framework;
using Testy;

namespace Mongrow.Tests.Trivial
{
    [TestFixture]
    public class CanDoIt : FixtureBase
    {
        [Test]
        public void TryRunning()
        {
            var database = MongoTest.GetCleanTestDatabase();

            var migrator = new Migrator(database);

            migrator.Execute();
        }
    }
}

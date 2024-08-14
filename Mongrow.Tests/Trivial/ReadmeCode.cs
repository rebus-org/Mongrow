using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Mongrow.Steps;
using NUnit.Framework;

namespace Mongrow.Tests.Trivial;

[TestFixture]
public class ReadmeCode
{
    [Test]
    public void ExampleStep()
    {
        new Migrator(MongoTest.GetCleanTestDatabase(), new[] {new AddAdminUser()}).Execute();

        //var migrator = new Migrator(
        //    connectionString: "mongodb://localhost/mongrow2",
        //    steps: GetSteps.FromAssemblyOf<AddAdminUser>()
        //);

        //migrator.Execute();
    }

    [Step(1)]
    public class AddAdminUser : IStep
    {
        public async Task Execute(IMongoDatabase database, ILog log, CancellationToken cancellationToken)
        {
            var users = database.GetCollection<BsonDocument>("users");

            var adminUser = new
            {
                _id = Guid.NewGuid().ToString(),
                uid = "user1",
                claims = new[]
                {
                    new {type = ClaimTypes.Email, value = "admin@whatever.com"},
                    new {type = ClaimTypes.Role, value = "admin"},
                }
            };

            await users.InsertOneAsync(adminUser.ToBsonDocument());
        }
    }
}
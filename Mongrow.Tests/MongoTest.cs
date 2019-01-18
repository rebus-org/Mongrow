using MongoDB.Driver;

namespace Mongrow.Tests
{
    public static class MongoTest
    {
        public static IMongoDatabase GetCleanTestDatabase()
        {
            var mongoUrl = new MongoUrl("mongodb://localhost/mongrow");
            var mongoClient = new MongoClient(mongoUrl);
            var databaseName = mongoUrl.DatabaseName;
            
            mongoClient.DropDatabase(databaseName);
            
            return mongoClient.GetDatabase(databaseName);
        }
    }
}

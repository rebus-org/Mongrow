using System.Threading.Tasks;
using MongoDB.Driver;
using Mongrow.Internals;

namespace Mongrow
{
    public class Migrator
    {
        readonly IMongoDatabase _mongoDatabase;

        public Migrator(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public void Execute() => AsyncHelpers.RunSync(ExecuteAsync);

        public async Task ExecuteAsync()
        {
        }
    }
}

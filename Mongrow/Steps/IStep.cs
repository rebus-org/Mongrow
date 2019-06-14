using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Mongrow.Steps
{
    public interface IStep
    {
        Task Execute(IMongoDatabase database, ILog log, CancellationToken cancellationToken);
    }
}
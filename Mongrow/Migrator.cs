using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Mongrow.Internals;
using Mongrow.Steps;

namespace Mongrow
{
    public class Migrator
    {
        readonly IMongoDatabase _mongoDatabase;
        readonly Options _options;
        readonly List<IStep> _steps;

        public Migrator(IMongoDatabase mongoDatabase, IEnumerable<IStep> steps, Options options = null)
        {
            _mongoDatabase = mongoDatabase;
            _options = options ?? new Options();
            _steps = steps.ToList();

            InitialScreening();

            Log($"Mongrow initialized with {_steps.Count} migration steps", verbose: false);
            Log(_steps.ListedAs(step => $"{step.GetId()}: {step.GetType().FullName}"));
        }

        public void Execute() => AsyncHelpers.RunSync(ExecuteAsync);

        public async Task ExecuteAsync()
        {
            while (true)
            {
                var idsOfStepsAlreadyExecuted = await GetIdsOfStepsAlreadyExecuted();
                var stepToExecute = GetNextStepToExecute(idsOfStepsAlreadyExecuted);

                if (stepToExecute == null) return;

                await ExecuteStep(stepToExecute);
            }
        }

        async Task ExecuteStep(IStep step)
        {
            Log($"Executing migration step {step.GetId()}: {step.GetType().FullName}");

            var stopwatch = Stopwatch.StartNew();
            try
            {
                await step.Execute(_mongoDatabase);

                Log($"Recording execution of step {step.GetId()}");

                var elapsed = stopwatch.Elapsed;

                await RecordExecution(step, elapsed);

                Log($"Successfully executed migration step {step.GetId()}: {step.GetType().FullName} in {elapsed.TotalSeconds:0.0} s", verbose: false);
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not execute step with ID {step.GetId()}", exception);
            }
        }

        async Task RecordExecution(IStep step, TimeSpan elapsed)
        {
            var mongoCollection = _mongoDatabase.GetCollection<BsonDocument>(_options.CollectionName);

            var bsonDocument = new BsonDocument
            {
                {"_id", step.GetId().ToString()},
                {"time_utc", DateTime.UtcNow},
                {"time_local", DateTime.Now.ToString(CultureInfo.CurrentCulture)},
                {"elapsed_s", elapsed.TotalSeconds},
                {"type", step.GetType().FullName},
                {"user", Environment.UserName},
                {"domain", Environment.UserDomainName},
                {"machine", Environment.MachineName},
            };

            try
            {
                await mongoCollection.InsertOneAsync(bsonDocument);
            }
            catch (Exception exception)
            {
                throw new ApplicationException($@"An error occurred when attempting to register execution of step {step.GetId()} by inserting

{bsonDocument}

into {_options.CollectionName}", exception);
            }
        }

        IStep GetNextStepToExecute(HashSet<StepId> idsOfStepsAlreadyExecuted)
        {
            return _steps
                .Where(step => !idsOfStepsAlreadyExecuted.Contains(step.GetId()))
                .OrderBy(step => step.GetId())
                .FirstOrDefault();
        }

        async Task<HashSet<StepId>> GetIdsOfStepsAlreadyExecuted()
        {
            var mongoCollection = _mongoDatabase.GetCollection<BsonDocument>(_options.CollectionName);

            using (var cursor = await mongoCollection.FindAsync(s => true))
            {
                var documents = await cursor.ToListAsync();

                var stepIds = documents
                    .Select(doc => StepId.Parse(doc["_id"].AsString));

                return new HashSet<StepId>(stepIds);
            }
        }

        void InitialScreening()
        {
            Log("Checking migration steps for consistency");

            var steps = _steps
                .Select(step => new
                {
                    Step = step,
                    Attribute = step.GetType().GetCustomAttributes().OfType<StepAttribute>().FirstOrDefault()
                })
                .ToList();

            var stepsWithoutAttribute = steps.Where(s => s.Attribute == null).ToList();

            if (stepsWithoutAttribute.Any())
            {
                throw new ArgumentException($@"The following steps passed to the migrator do not carry the step attribute: 

{stepsWithoutAttribute.ListedAs(a => a.Step.GetType())}

Please remember to decorate each step class with [Step(...)]");
            }

            var duplicatedIds = steps.GroupBy(s => s.Attribute.GetId())
                .Where(g => g.Count() > 1).ToList();

            if (duplicatedIds.Any())
            {
                throw new ArgumentException($@"The following steps passed to the migrator have duplicated IDs:

{duplicatedIds.ListedAs(a => $@"{a.Key}: 
{a.ListedAs(g => g.Step.GetType(), level: 2)}")}

Please ensure that each step is provided with a unique ID. Remember that IDs can be further qualified, by adding a branch specification like this:  [Step(3, ""something"")]");
            }
        }

        void Log(string message, bool verbose = true)
        {
            if (verbose)
            {
                _options.VerboseLogAction(message);
            }
            else
            {
                _options.LogAction(message);
            }
        }
    }
}

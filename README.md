# Mongrow

It's a MongoDB migration helper.

With this, you can write classes that implement steps to migrate a MongoDB database.

## What do steps look like?

Like this:

```csharp
[Step(1)]
public class AddAdminUser : IStep
{
    public async Task Execute(IMongoDatabase database)
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
```

and then you execute it like this:

```csharp
var migrator = new Migrator(
    connectionString: "mongodb://mongohost01/MyDatabase",
    steps: GetSteps.FromAssemblyOf<AddAdminUser>()
);

migrator.Execute();
```

## How to make robust steps

While steps are just C# code, and you can do anything you want in there to the passed-in `IMongoDatabase`, you are
encouraged to write steps that do not change along with the rest of your code.

This means that you most likely want to use `BsonDocument`, magic strings, and anonymous types throughout.

Wouldn't want a rename of one of your C# classes to mess up how all of your existing migrations work.

## Parallel execution

A distributed lock is used to coordinate execution, so Mongrow will never execute migrations concurrently.



## How to number the steps

Steps are identified by a number and a "branch specification". The branch specification allows for
co-existence of steps with the same number, thus escaping a global lock on the number sequence when
working with multiple branches.

The branch specification defaults to `master`. You are encouraged to structure your steps like this:

```
| 1 - master |                 |
| 2 - master |                 |
| 3 - master | 3 - some-branch |
|            | 4 - some-branch |
| 5 - master |                 |
```

and so forth.

PLEASE NOTE: You are not allowed to INSERT a step into the sequence, so if step number `n` with any branch
specification has been executed, you will get an exception if you add a migration with number `< n` for
that branch specification.
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Mongrow.Internals;

static class StringExtensions
{
    public static string ListedAs<TItem>(this IEnumerable<TItem> items, Func<TItem, object> getValue, int level = 1)
    {
        const int factor = 4;

        var indentation = new string(' ', factor * level);

        return string.Join(Environment.NewLine,
            items.Select(item => string.Concat(indentation, $"- {getValue(item)}")));
    }

    public static IMongoDatabase GetMongoDatabase(this string connectionString)
    {
        var mongoUrl = new MongoUrl(connectionString);
        var databaseName = mongoUrl.DatabaseName;

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentException($"The connection string '{connectionString}' does not specify a database name!");
        }

        return new MongoClient(mongoUrl).GetDatabase(databaseName);
    }
}
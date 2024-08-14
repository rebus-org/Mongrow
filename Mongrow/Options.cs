using System;

namespace Mongrow;

public class Options
{
    static readonly Action<string> NoAction = _ => { };

    public Options(string collectionName = "_mongrow", string lockCollectionName = "_mongrow_lock", Action<string> logAction = null, Action<string> verboseLogAction = null)
    {
        LogAction = logAction ?? NoAction;
        VerboseLogAction = verboseLogAction ?? NoAction;
        CollectionName = collectionName;
        LockCollectionName = lockCollectionName;
    }

    internal string CollectionName { get; }
        
    internal string LockCollectionName { get; }

    internal Action<string> LogAction { get; }
        
    internal Action<string> VerboseLogAction { get; }
}
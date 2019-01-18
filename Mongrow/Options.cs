using System;

namespace Mongrow
{
    public class Options
    {
        static readonly Action<string> NoAction = _ => { };

        public Options(string collectionName = "_mongrow", Action<string> logAction = null, Action<string> verboseLogAction = null)
        {
            LogAction = logAction ?? NoAction;
            VerboseLogAction = verboseLogAction ?? NoAction;
            CollectionName = collectionName;
        }

        internal string CollectionName { get; }

        internal Action<string> LogAction { get; }
        
        internal Action<string> VerboseLogAction { get; }
    }
}
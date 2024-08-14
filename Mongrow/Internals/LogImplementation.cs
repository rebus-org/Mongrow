using System;
using Mongrow.Steps;

namespace Mongrow.Internals;

class LogImplementation : ILog
{
    readonly Action<string> _verboseLog;
    readonly Action<string> _log;

    public LogImplementation(Action<string> verboseLog, Action<string> log)
    {
        _verboseLog = verboseLog;
        _log = log;
    }

    public void WriteVerbose(string text) => _verboseLog(text);

    public void Write(string text) => _log(text);
}
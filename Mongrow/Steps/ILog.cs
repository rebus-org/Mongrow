namespace Mongrow.Steps
{
    public interface ILog
    {
        void WriteVerbose(string text);
        void Write(string text);
    }
}
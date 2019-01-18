namespace Mongrow
{
    public class Options
    {
        public Options(string collectionName = "_mongrow")
        {
            CollectionName = collectionName;
        }

        public string CollectionName { get; }
    }
}
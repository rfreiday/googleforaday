namespace Google.Common
{
    public interface IGlobalCachingProvider
    {
        void AddItem(string key, object value);
        T GetItem<T>(string key, bool createIfNull);
    } 
}

namespace Toggl.PrimeRadiant.Settings
{
    public interface IKeyValueStorage
    {
        bool GetBool(string key);

        string GetString(string key);

        void SetBool(string key, bool value);

        void SetString(string key, string value);

        void Remove(string key);

        void RemoveAllWithPrefix(string prefix);
    }
}

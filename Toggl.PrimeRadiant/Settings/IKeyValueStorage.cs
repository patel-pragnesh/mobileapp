using System;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IKeyValueStorage
    {
        bool GetBool(string key);

        string GetString(string key);

        int GetInt(string key, int defaultValue);
      
        DateTimeOffset? GetDateTimeOffset(string key);

        void SetBool(string key, bool value);

        void SetString(string key, string value);

        void SetInt(string key, int value);
      
        void SetDateTimeOffset(string key, DateTimeOffset value);

        void Remove(string key);

        void RemoveAllWithPrefix(string prefix);
    }
}

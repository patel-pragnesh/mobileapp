using System;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Realm.Models;

namespace Toggl.PrimeRadiant.Realm
{
    public sealed class SinceParameterStorage : ISinceParameterRepository
    {
        private readonly Func<Realms.Realm> getRealmInstance;

        private static readonly Dictionary<Type, string> keys = new Dictionary<Type, string>()
        {
            [typeof(IDatabaseUser)] = "user"
        };

        public SinceParameterStorage(Func<Realms.Realm> getRealmInstance)
        {
            Ensure.Argument.IsNotNull(getRealmInstance, nameof(getRealmInstance));

            this.getRealmInstance = getRealmInstance;
        }

        public DateTimeOffset? Get(Type entityType)
        {
            var key = getKeyByType(entityType);
            var realm = getRealmInstance();
            var record = realm.Find<RealmSinceParameter>(key);
            return record?.Since;
        }

        public void Set(Type entityType, DateTimeOffset? since)
        {
            var key = getKeyByType(entityType);
            var realm = getRealmInstance();
            using (var transaction = realm.BeginWrite())
            {
                var record = realm.Find<RealmSinceParameter>(key);
                if (record == null)
                {
                    record = new RealmSinceParameter
                    {
                        Key = key,
                        Since = since
                    };
                    realm.Add(record);
                }
                else
                {
                    record.Since = since;
                }

                transaction.Commit();
            }
        }

        private string getKeyByType(Type entityType)
        {
            if (keys.TryGetValue(entityType, out var key))
                return key;

            throw new ArgumentException($"Since parameters for the type {entityType.FullName} cannot be stored.");
        }
    }
}

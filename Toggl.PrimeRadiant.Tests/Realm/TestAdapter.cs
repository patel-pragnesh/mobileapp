﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Realm;

namespace Toggl.PrimeRadiant.Tests.Realm
{
    public class GenericTestAdapter<T> : IRealmAdapter<T, T>
        where T : class, IIdentifiable
    {
        private readonly List<T> list = new List<T>();
        private readonly Func<long, Predicate<T>> matchById;

        public GenericTestAdapter()
            : this(id => e => e.Id == id)
        {
        }

        public GenericTestAdapter(Func<long, Predicate<T>> matchById)
        {
            this.matchById = matchById;
        }

        public T Get(long id)
            => list.Single(entity => matchById(id)(entity));

        public T Create(T entity)
        {
            if (list.Find(matchById(entity.Id)) != null)
                throw new InvalidOperationException();

            list.Add(entity);

            return entity;
        }

        public T Update(long id, T entity)
        {
            var index = list.FindIndex(matchById(id));

            if (index == -1)
                throw new InvalidOperationException();

            list[index] = entity;

            return entity;
        }

        public IQueryable<T> GetAll()
            => list.AsQueryable();

        public void Delete(long id)
        {
            var entity = Get(id);
            var worked = list.Remove(entity);
            if (worked) return;

            throw new InvalidOperationException();
        }

        public IEnumerable<IConflictResolutionResult<T>> BatchUpdate(
            IEnumerable<(long Id, T Entity)> entities,
            Func<T, T, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<T, T> resolver)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class TestAdapter : GenericTestAdapter<TestModel>
    {
        public TestAdapter()
            : base()
        {
        }

        public TestAdapter(Func<long, Predicate<TestModel>> matchById)
            : base(matchById)
        {
        }
    }
}

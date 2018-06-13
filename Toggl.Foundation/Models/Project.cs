using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.Models
{
    internal class Project : IThreadSafeProject
    {
        public long Id { get; }
        public string Name { get; }
        public bool IsPrivate { get; }
        public bool Active { get; }
        public string Color { get; }
        public bool? Billable { get; }
        public bool? Template { get; }
        public bool? AutoEstimates { get; }
        public long? EstimatedHours { get; }
        public double? Rate { get; }
        public string Currency { get; }
        public int? ActualHours { get; }
        public long WorkspaceId { get; }
        public long? ClientId { get; }
        public SyncStatus SyncStatus { get; }
        public string LastSyncErrorMessage { get; }
        public bool IsDeleted { get; }
        public DateTimeOffset At { get; }
        public DateTimeOffset? ServerDeletedAt { get; }

        public IThreadSafeWorkspace Workspace { get; }
        IDatabaseWorkspace IDatabaseProject.Workspace => Workspace;
        public IThreadSafeClient Client { get; }
        IDatabaseClient IDatabaseProject.Client => Client;
        public IEnumerable<IThreadSafeTask> Tasks { get; }
        IEnumerable<IDatabaseTask> IDatabaseProject.Tasks => Tasks;

        private Project(IProject entity, SyncStatus syncStatus, string lastSyncErrorMessage = "", bool isDeleted = false,
            Client client = null, Workspace workspace = null, IEnumerable<Task> tasks = null)
            : this(entity.Id, entity.Name, entity.IsPrivate, entity.Active, entity.Color, entity.Billable, entity.Template, entity.AutoEstimates,
                entity.EstimatedHours, entity.Rate, entity.Currency, entity.ActualHours, entity.WorkspaceId, entity.ClientId, syncStatus,
                lastSyncErrorMessage, isDeleted, entity.At, entity.ServerDeletedAt, workspace, client, tasks)
        {

        }

        public Project(long id, string name, bool isPrivate, bool active, string color, bool? billable, bool? template,
            bool? autoEstimates, long? estimatedHours, double? rate, string currency, int? actualHours,
            long workspaceId, long? clientId, SyncStatus syncStatus, string lastSyncErrorMessage, bool isDeleted,
            DateTimeOffset at, DateTimeOffset? serverDeletedAt,
            IThreadSafeWorkspace workspace, IThreadSafeClient client, IEnumerable<IThreadSafeTask> tasks)
        {
           /* Ensure.Argument.IsNotNullOrEmpty(name, nameof(name));
            Ensure.Argument.IsNotNullOrEmpty(color, nameof(color));
            Ensure.Argument.IsNotZero(workspaceId, nameof(workspaceId));
            Ensure.Argument.IsNotNull(at, nameof(at));
            Ensure.Argument.IsNotTooLong(name, MaxClientNameLengthInBytes, nameof(name));
            */
            Id = id;
            Name = name;
            IsPrivate = isPrivate;
            Active = active;
            Color = color;
            Billable = billable;
            Template = template;
            AutoEstimates = autoEstimates;
            EstimatedHours = estimatedHours;
            Rate = rate;
            Currency = currency;
            ActualHours = actualHours;
            WorkspaceId = workspaceId;
            ClientId = clientId;
            At = at;
            ServerDeletedAt = serverDeletedAt;

            Workspace = workspace;
            Client = client;
            Tasks = tasks;

            SyncStatus = syncStatus;
            LastSyncErrorMessage = lastSyncErrorMessage;
            IsDeleted = isDeleted;
        }

        // Bad constructor, for creation
        public Project(long id, string name, DateTimeOffset at, SyncStatus syncStatus, string color, long workspaceId, long? clientId = null, bool active = false, bool? billable = null)
        {
            Id = id;
            Name = name;
            At = at;
            SyncStatus = syncStatus;
            Color = color;
            Active = active;
            Billable = billable;
            ClientId = clientId;
            WorkspaceId = workspaceId;
        }

        public static Project From(IDatabaseProject entity)
        {
            return new Project(
                entity,
                entity.SyncStatus,
                entity.LastSyncErrorMessage,
                entity.IsDeleted,
                entity.Client == null ? null : Models.Client.From(entity.Client),
                entity.Workspace == null ? null : Models.Workspace.From(entity.Workspace),
                entity.Tasks == null ? null : entity.Tasks.Select(Models.Task.From)
            );
        }

        public static Project Clean(IProject entity)
            => new Project(entity, SyncStatus.InSync, null);

        public static Project Unsyncable(IProject entity, string errorMessage)
            => new Project(entity, SyncStatus.SyncFailed, errorMessage);

        public static Project Dirty(IProject entity)
            => new Project(entity, SyncStatus.SyncNeeded, null);

        public class ProjectBuilder
        {
            internal IList<Action<Project>> actions = new List<Action<Project>>();

            internal ProjectBuilder With(Action<Project> with)
            {
                actions.Add(with);
                return this;
            }

            public Project build()
            {
                ensureValidity();
                return new Project(this);
            }

            private void ensureValidity()
            {

            }
        }

        private Project(ProjectBuilder builder)
        {
            foreach (var action in builder.actions)
            {
                action(this);
            }
        }
    }
}

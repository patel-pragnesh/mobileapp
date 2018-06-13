using System;
using System.Collections.Generic;
using System.Linq;
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
            Client client = null, Workspace workspace = null, IEnumerable<Task> taks = null)
        {
            Ensure.Argument.IsNotNullOrEmpty(entity.Name, nameof(entity.Name));
            Ensure.Argument.IsNotNullOrEmpty(entity.Color, nameof(entity.Color));
            Ensure.Argument.IsNotZero(entity.WorkspaceId, nameof(entity.WorkspaceId));
            Ensure.Argument.IsNotNull(entity.At, nameof(entity.At));
            Ensure.Argument.IsNotTooLong(entity.Name, MaxClientNameLengthInBytes, nameof(entity.Name));

            Id = entity.Id;
            Name = entity.Name;
            IsPrivate = entity.IsPrivate;
            Active = entity.Active;
            Color = entity.Color;
            Billable = entity.Billable;
            Template = entity.Template;
            AutoEstimates = entity.AutoEstimates;
            EstimatedHours = entity.EstimatedHours;
            Rate = entity.Rate;
            Currency = entity.Currency;
            ActualHours = entity.ActualHours;
            WorkspaceId = entity.WorkspaceId;
            ClientId = entity.ClientId;
            At = entity.At;
            ServerDeletedAt = entity.ServerDeletedAt;

            Workspace = workspace;
            Client = client;
            Tasks = taks;

            SyncStatus = syncStatus;
            LastSyncErrorMessage = lastSyncErrorMessage;
            IsDeleted = isDeleted;
        }

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
    }
}

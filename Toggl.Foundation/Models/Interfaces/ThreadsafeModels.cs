using System.Collections.Generic;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models.Interfaces
{
    public partial interface IThreadSafeClient
        : IThreadSafeModel, IDatabaseSyncable, IClient
    {
        IThreadSafeWorkspace Workspace { get; }
    }

    public partial interface IThreadSafePreferences
        : IThreadSafeModel, IDatabaseSyncable, IPreferences, IIdentifiable
    {
    }

    public partial interface IThreadSafeProject
        : IThreadSafeModel, IDatabaseSyncable, IProject
    {
        IThreadSafeClient Client { get; }

        IThreadSafeWorkspace Workspace { get; }

        IEnumerable<IThreadSafeTask> Tasks { get; }
    }

    public partial interface IThreadSafeTag
        : IThreadSafeModel, IDatabaseSyncable, ITag
    {
        IThreadSafeWorkspace Workspace { get; }
    }

    public partial interface IThreadSafeTask
        : IThreadSafeModel, IDatabaseSyncable, ITask
    {
        IThreadSafeUser User { get; }

        IThreadSafeProject Project { get; }

        IThreadSafeWorkspace Workspace { get; }
    }

    public partial interface IThreadSafeTimeEntry
        : IThreadSafeModel, IDatabaseSyncable, ITimeEntry
    {
        IThreadSafeTask Task { get; }

        IThreadSafeUser User { get; }

        IThreadSafeProject Project { get; }

        IThreadSafeWorkspace Workspace { get; }

        IEnumerable<IThreadSafeTag> Tags { get; }
    }

    public partial interface IThreadSafeUser
        : IThreadSafeModel, IDatabaseSyncable, IUser
    {
    }

    public partial interface IThreadSafeWorkspace
        : IThreadSafeModel, IDatabaseSyncable, IWorkspace
    {
    }

    public partial interface IThreadSafeWorkspaceFeature
        : IThreadSafeModel, IWorkspaceFeature
    {
    }

    public partial interface IThreadSafeWorkspaceFeatureCollection
        : IThreadSafeModel, IWorkspaceFeatureCollection
    {
        IThreadSafeWorkspace Workspace { get; }

        IEnumerable<IThreadSafeWorkspaceFeature> DatabaseFeatures { get; }
    }
}

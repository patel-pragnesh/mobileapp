using System.Collections.Generic;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models.Interfaces
{
    public partial interface IThreadSafeClient
        : IThreadSafeModel, IDatabaseClient
    {
        IThreadSafeWorkspace ThreadSafeWorkspace { get; }
    }

    public partial interface IThreadSafePreferences
        : IThreadSafeModel, IDatabasePreferences
    {
    }

    public partial interface IThreadSafeProject
        : IThreadSafeModel, IDatabaseProject
    {
        IThreadSafeClient ThreadSafeClient { get; }

        IThreadSafeWorkspace ThreadSafeWorkspace { get; }

        IEnumerable<IThreadSafeTask> ThreadSafeTasks { get; }
    }

    public partial interface IThreadSafeTag
        : IThreadSafeModel, IDatabaseTag
    {
        IThreadSafeWorkspace ThreadSafeWorkspace { get; }
    }

    public partial interface IThreadSafeTask
        : IThreadSafeModel, IDatabaseTask
    {
        IThreadSafeUser ThreadSafeUser { get; }

        IThreadSafeProject ThreadSafeProject { get; }

        IThreadSafeWorkspace ThreadSafeWorkspace { get; }
    }

    public partial interface IThreadSafeTimeEntry
        : IThreadSafeModel, IDatabaseTimeEntry
    {
        IThreadSafeTask ThreadSafeTask { get; }

        IThreadSafeUser ThreadSafeUser { get; }

        IThreadSafeProject ThreadSafeProject { get; }

        IThreadSafeWorkspace ThreadSafeWorkspace { get; }

        IEnumerable<IThreadSafeTag> ThreadSafeTags { get; }
    }

    public partial interface IThreadSafeUser
        : IThreadSafeModel, IDatabaseUser
    {
    }

    public partial interface IThreadSafeWorkspace
        : IThreadSafeModel, IDatabaseWorkspace
    {
    }

    public partial interface IThreadSafeWorkspaceFeature
        : IThreadSafeModel, IDatabaseWorkspaceFeature
    {
    }

    public partial interface IThreadSafeWorkspaceFeatureCollection
        : IThreadSafeModel, IDatabaseWorkspaceFeatureCollection
    {
        IThreadSafeWorkspace ThreadSafeWorkspace { get; }

        IEnumerable<IThreadSafeWorkspaceFeature> ThreadSafeDatabaseFeatures { get; }
    }
}

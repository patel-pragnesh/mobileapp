﻿using System;

namespace Toggl.Multivac.Models
{
    public interface IProject : IIdentifiable, ILastChangeDatable, IDeletable
    {
        long WorkspaceId { get; }

        long? ClientId { get; }

        string Name { get; }

        bool IsPrivate { get; }

        bool Active { get; }

        string Color { get; }

        bool? Billable { get; }

        bool? Template { get; }

        bool? AutoEstimates { get; }

        long? EstimatedHours { get; }

        double? Rate { get; }

        string Currency { get; }

        int? ActualHours { get; }
    }
}

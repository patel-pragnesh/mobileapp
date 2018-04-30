﻿using System;

namespace Toggl.PrimeRadiant.Models
{
    public interface ISinceParameter
    {
        string Key { get; }

        DateTimeOffset? Since { get; }
    }
}

using System;

namespace Loadshop.DomainServices.Utilities
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        DateTime Today { get; }

        DateTimeOffset OffsetNow { get; }
        DateTimeOffset OffsetUtcNow { get; }
    }
}

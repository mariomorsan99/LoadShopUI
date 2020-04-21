using System;

namespace Loadshop.DomainServices.Utilities
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Today => DateTime.Today;

        public DateTimeOffset OffsetNow => DateTimeOffset.Now;

        public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Models
{
    public struct SmallDateTime
    {
        private DateTime? baseDateTime;
        private DateTime BaseDateTime
        {
            get => (baseDateTime ?? new DateTime(631139040000000000L));
            set => baseDateTime = value;
        }

        public static readonly SmallDateTime MaxValue = new SmallDateTime(DateTime.MaxValue);
        public static readonly SmallDateTime MinValue = new SmallDateTime(0);

        public static SmallDateTime Now => new SmallDateTime(DateTime.Now);
        public static SmallDateTime Today => new SmallDateTime(DateTime.Today);
        public static SmallDateTime UtcNow => new SmallDateTime(DateTime.UtcNow);

        public SmallDateTime(DateTime baseDateTime)
        {
            this.baseDateTime = baseDateTime;
        }

        public SmallDateTime(long compressedDateTime)
        {
            this.baseDateTime = new DateTime(((long)compressedDateTime * 10000L) + 631139040000000000L);
        }

        public DateTime GetBaseDateTime()
        {
            return BaseDateTime;
        }

        public long GetCompressedTime()
        {
            return (BaseDateTime.Ticks - 631139040000000000L) / 10000L;
        }

        public override bool Equals(object obj)
        {
            if (obj is SmallDateTime unix)  return this == unix;
            return false;
        }

        public override int GetHashCode()
        {
            return BaseDateTime.GetHashCode();
        }

        public static SmallDateTime operator +(SmallDateTime d, TimeSpan t) => new SmallDateTime(d.BaseDateTime + t);
        public static TimeSpan operator -(SmallDateTime d1, SmallDateTime d2) => d1.BaseDateTime - d2.BaseDateTime;
        public static SmallDateTime operator -(SmallDateTime d, TimeSpan t) => new SmallDateTime(d.BaseDateTime - t);
        public static bool operator ==(SmallDateTime d1, SmallDateTime d2) => d1.BaseDateTime == d2.BaseDateTime;
        public static bool operator !=(SmallDateTime d1, SmallDateTime d2) => d1.BaseDateTime != d2.BaseDateTime;
        public static bool operator <(SmallDateTime t1, SmallDateTime t2) => t1.BaseDateTime < t2.BaseDateTime;
        public static bool operator >(SmallDateTime t1, SmallDateTime t2) => t1.BaseDateTime > t2.BaseDateTime;
        public static bool operator <=(SmallDateTime t1, SmallDateTime t2) => t1.BaseDateTime <= t2.BaseDateTime;
        public static bool operator >=(SmallDateTime t1, SmallDateTime t2) => t1.BaseDateTime >= t2.BaseDateTime;
    }
}

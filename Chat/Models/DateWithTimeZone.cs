using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    [Serializable]
    public struct DateWithTimeZone
    {
        private TimeZoneInfo tz;
        private DateTime dt;

        public DateWithTimeZone(DateTime dateValue, TimeZoneInfo timeZone)
        {
            dt = dateValue;
            if (timeZone == null)
                tz = TimeZoneInfo.Local;
            else
                tz = timeZone;
        }

        public TimeZoneInfo TimeZone
        {
            get { return (tz); }
            set { tz = value; }
        }

        public DateTime DateTime
        {
            get { return (dt); }
            set { dt = value; }
        }
    }
}
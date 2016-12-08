using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    public class ClientDateTimeCookie
    {
        public int UTCMinutesOffset { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
    }
}
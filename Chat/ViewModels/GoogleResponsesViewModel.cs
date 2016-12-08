using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.ViewModels
{
    public class GoogleResponse
    {
        public enum STATUS
        {
            INACTIVE,
            TENPAST,
            ACTIVENOW,
            FIFTEEN,
            THIRTY,
            HOUR,
            TWOHOUR,
            TWOHOURPLUS,
            UNKNOWN
        }

        public Hashtable StatusStrings { get
            {
                return new Hashtable()
                {
                    { 0, "Inactive" },
                    { 1, "10 minutes past!" },
                    { 2, "Active" },
                    { 3, "15 or fewer minutes" },
                    { 4, "30 or fewer minutes" },
                    { 5, "1 hour or less" },
                    { 6, "2 hours or less" },
                    { 7, "More than 2 hours" },
                    { 8, "Unknown time frame" }
                };
            } }

        public string Topic { get; set; }
        public DateTime TimeRequested { get; set; }
        public STATUS Status { get; set; }
    }

    public class GoogleResponsesViewModel
    {
        public List<GoogleResponse> Responses { get; set; }

        public GoogleResponsesViewModel()
        {
            Responses = new List<GoogleResponse>();
        }
    }
}
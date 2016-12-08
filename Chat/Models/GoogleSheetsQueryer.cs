using System;
using System.Web.Configuration;
using System.Collections.Generic;
using Chat.ViewModels;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections;
using System.Collections.ObjectModel;

namespace Chat.Models
{
    public static class GoogleSheetsQueryer
    {
        private static Hashtable TimeZones { get
            {
                return new Hashtable()
                {
                    { "AST", "Atlantic Standard Time" },
                    { "EST", "Eastern Standard Time" },
                    { "CST", "Central Standard Time" },
                    { "MST", "Mountain Standard Time" },
                    { "PST", "Pacific Standard Time" },
                    { "AKST", "Alaskan Standard Time" },
                    { "HAST", "Hawaiian Standard Time" },
                    { "WST", "Samoa Standard Time" }
                };
            } }

        private static ClientDateTimeCookie ParseCookie (string cookie)
        {
            var parts = cookie.Split(':');

            return new ClientDateTimeCookie()
            {
                UTCMinutesOffset = Convert.ToInt32(parts[0]),
                Year = Convert.ToInt32(parts[1]),
                Month = Convert.ToInt32(parts[2]),
                Day = Convert.ToInt32(parts[3]),
                Hour = Convert.ToInt32(parts[4]),
                Minute = Convert.ToInt32(parts[5]),
                Second = Convert.ToInt32(parts[6])
            };
        }

        public static GoogleResponsesViewModel QueryResponses(string cookie)
        {
            // Create service to query spreadsheet
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = "SafeTalk",
                ApiKey = WebConfigurationManager.AppSettings["GoogleSheetsAPIKey"]
            });

            // Define request parameters.
            string spreadsheetId = "1Jv1yjyFMHW0FU3nxjSnh4EZ1jy5pVSRMI5_XFHMtQew";
            string range = "Sheet1";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Retrieve values from spreadsheet            
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;

            return ExtractResponse(values, cookie);
        }

        private static GoogleResponsesViewModel ExtractResponse(IList<IList<Object>> values, string cookie)
        {
            var viewModel = new GoogleResponsesViewModel();
            var userCookie = ParseCookie(cookie);          

            // Find client's timezone
            TimeZoneInfo clientTimezone = null;
            ReadOnlyCollection<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones();
            foreach (TimeZoneInfo zone in zones)
            {
                if (zone.BaseUtcOffset.Hours * 60 + zone.BaseUtcOffset.Minutes == userCookie.UTCMinutesOffset)
                {
                    clientTimezone = zone;
                }
            }

            // Iterate over spreadsheet values
            var skipFirstRow = false;
            if (values != null && values.Count > 0)
            {
                foreach (var submission in values)
                {
                    // Skip first row as first row has names of columns;
                    // or if row is somehow empty in google doc
                    if (!skipFirstRow || submission.Count == 0)
                    {
                        skipFirstRow = true;
                        continue;
                    }



                    // Create datetime of response; messy because of
                    // the response from the google doc
                    var date = submission[3].ToString().Split('/');
                    var year = Convert.ToInt32(date[2]);
                    var month = Convert.ToInt32(date[0]);
                    var day = Convert.ToInt32(date[1]);

                    var time = submission[2].ToString();
                    var isAM = time.Split(' ')[1] == "AM";
                    var hour = Convert.ToInt32(time.Split(' ')[0].Split(':')[0]) - 1 + (isAM ? 0 : 12);
                    var min = Convert.ToInt32(time.Split(' ')[0].Split(':')[1]);
                    var sec = Convert.ToInt32(time.Split(' ')[0].Split(':')[2]);

                    var timeOfResponse = new DateTime(year, month, day, hour, min, sec);

                    
                    // Get timezone of response
                    var submittedTimezone = submission[4].ToString();
                    var parsedTimezone = submittedTimezone.Split(' ')[0].ToString();

                    // We can correctly estimate the time for this response
                    if (parsedTimezone != "OTHER" && clientTimezone != null)
                    {
                        var knownTimezone = TimeZones[parsedTimezone];
                        var timezoneOfResponse = TimeZoneInfo.FindSystemTimeZoneById(knownTimezone.ToString());
                        var timeOffsetOfResponse = timezoneOfResponse.BaseUtcOffset.Subtract(clientTimezone.BaseUtcOffset);
                        var clientUsersDatetime = new DateTime(userCookie.Year, userCookie.Month, userCookie.Day, userCookie.Hour, userCookie.Minute, userCookie.Second);
                        var datetimeOfResponseConvertedToUsersTime = timeOfResponse.Subtract(timeOffsetOfResponse);
                        var differenceInTimes = datetimeOfResponseConvertedToUsersTime.Subtract(clientUsersDatetime);
                        GoogleResponse.STATUS statusFlag = GoogleResponse.STATUS.INACTIVE;

                        if (differenceInTimes.Days > 0 || differenceInTimes.Hours >= 2)
                        {
                            statusFlag = GoogleResponse.STATUS.TWOHOURPLUS;
                        } else {
                            var totalMinutes = 0;

                            totalMinutes += differenceInTimes.Hours * 60;
                            totalMinutes += differenceInTimes.Minutes;

                            if (totalMinutes <= -15)
                            {
                                statusFlag = GoogleResponse.STATUS.INACTIVE;
                            } else if (totalMinutes <= -10)
                            {
                                statusFlag = GoogleResponse.STATUS.TENPAST;
                            } else if (totalMinutes <= 0)
                            {
                                statusFlag = GoogleResponse.STATUS.ACTIVENOW;
                            } else if (totalMinutes <= 15)
                            {
                                statusFlag = GoogleResponse.STATUS.FIFTEEN;
                            } else if (totalMinutes <= 30)
                            {
                                statusFlag = GoogleResponse.STATUS.THIRTY;
                            } else if (totalMinutes <= 60)
                            {
                                statusFlag = GoogleResponse.STATUS.HOUR;
                            } else if (totalMinutes <= 120)
                            {
                                statusFlag = GoogleResponse.STATUS.TWOHOUR;
                            }                            
                        }


                        // Add response
                        var formResponse = new GoogleResponse()
                        {
                            Topic = Convert.ToString(submission[1]),
                            TimeRequested = datetimeOfResponseConvertedToUsersTime,
                            Status = statusFlag
                        };

                        viewModel.Responses.Add(formResponse);
                    } else
                    {
                        // Add response
                        var formResponse = new GoogleResponse()
                        {
                            Topic = Convert.ToString(submission[1]),
                            TimeRequested = timeOfResponse,
                            Status = GoogleResponse.STATUS.UNKNOWN
                        };

                        viewModel.Responses.Add(formResponse);
                    }
                }
            }

            return viewModel;
        }
    }
}
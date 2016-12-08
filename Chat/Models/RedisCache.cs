using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Chat.Models
{
    //https://developers.google.com/sheets/quickstart/dotnet
    //http://stackoverflow.com/questions/725627/accessing-google-spreadsheets-with-c-sharp-using-google-data-api
    public class User
    {
        public string Guid { get; set; }
        public string ConnectionId { get; set; }
        public string Room { get; set; }
    }

    public class Room
    {
        public string Number { get; set; }
        public int UserCount { get; set; }
    }

    public class UsersOnline
    {
        public List<User> Users { get; set; }
    }


    public static class RedisQueryer
    {
        private static readonly string _MainKeyName = WebConfigurationManager.AppSettings["MainKeyName"].ToString();
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(WebConfigurationManager.ConnectionStrings["RedisCache"].ConnectionString);
        }); 

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static bool DoesMainKeyExist
        {
            get
            {
                IDatabase cache = Connection.GetDatabase();
                return !cache.StringGet(_MainKeyName).IsNullOrEmpty;
            }
        }

        private static IDatabase Cache
        {
            get
            {
                return Connection.GetDatabase();
            }
        }

        private static RedisValue GetMainKeyCache()
        {
            if (!DoesMainKeyExist)
            {
                // Create main key cache object
                var newCacheObject = new UsersOnline()
                {
                    Users = new List<User>()
                };

                Cache.StringSet(_MainKeyName, JsonConvert.SerializeObject(newCacheObject));
            }

            return Cache.StringGet(_MainKeyName);
        }


        public static User GetUser(string guid)
        {
            // Retrieve cache
            var cache = GetMainKeyCache();

            // Find user
            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);
            return userInformation.Users.Find(x => x.Guid == guid);
        }

        public static User RegisterUser(string guid, string connectionId)
        {
            // Retrieve cache
            var cache = GetMainKeyCache();

            // Attempt to find user
            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);
            var index = userInformation.Users.FindIndex(x => x.Guid == guid);


            // If user was not found, add record to cache
            if (index == -1)
            {
                var newUser = new User()
                {
                    Guid = guid,
                    ConnectionId = connectionId,
                    Room = "1"
                };
                userInformation.Users.Add(newUser);

                // Save updated cache information
                Cache.StringSet(_MainKeyName, JsonConvert.SerializeObject(userInformation));

                return newUser;
            }
            else
            {
                userInformation.Users[index].ConnectionId = connectionId;

                // Save updated cache information
                Cache.StringSet(_MainKeyName, JsonConvert.SerializeObject(userInformation));

                return userInformation.Users[index];
            }
        }

        public static void TransferUser(string guid, string room, bool joinRoom = false, bool leaveRoom = false)
        {
            // Retrieve cache
            var cache = GetMainKeyCache();

            // Find user
            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);
            var userIndex = userInformation.Users.FindIndex(x => x.Guid == guid);

            if (userIndex >= 0)
            {
                if (joinRoom)
                {
                    userInformation.Users[userIndex].Room = room;
                }
                else if (leaveRoom)
                {
                    userInformation.Users[userIndex].Room = "-1";
                }
            }

            // Save updated user information
            Cache.StringSet(_MainKeyName, JsonConvert.SerializeObject(userInformation));
        }

        public static string GetGuidForUser(string connectionId)
        {
            // Retrieve cache
            var cache = GetMainKeyCache();

            // Find user
            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);
            var userIndex = userInformation.Users.FindIndex(x => x.ConnectionId == connectionId);

            if (userIndex != -1)
            {
                return userInformation.Users[userIndex].Guid;
            }
            else
            {
                return "";
            }
        }

        public static int ConnectedUsersInChatRoom(string room)
        {
            var cache = GetMainKeyCache();

            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);
            return userInformation.Users.FindAll(x => x.Room == room).Count;
        }

        public static int TotalConnectedUsers()
        {
            var cache = GetMainKeyCache();

            UsersOnline userInformation = JsonConvert.DeserializeObject<UsersOnline>(cache);

            return userInformation.Users.Count;
        }

        // Clears out all information of the cache;
        // http://stackoverflow.com/questions/24531421/remove-delete-all-one-item-from-stackexchange-redis-cache
        public static void ClearCache()
        {
            var endpoints = Connection.GetEndPoints();
            var server = Connection.GetServer(endpoints.First());

            var keys = server.Keys();
            foreach (var key in keys)
            {
                // Only delete cache from environment we execute this command in
                if (key == WebConfigurationManager.AppSettings["MainKeyName"])
                {
                    Cache.KeyDelete(key);
                }                
            }
        }
    }
}
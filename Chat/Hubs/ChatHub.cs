using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Chat.Models;

namespace Chat.Hubs
{

    public class ChatHub : Hub
    {
        private readonly static int _UserLimitPerChatRoom = 20;

        // Returns the total number of chat rooms
        private string GetTotalNumberOfRooms()
        {
            var n = RedisQueryer.TotalConnectedUsers() / _UserLimitPerChatRoom + 1;

            return Convert.ToString(n);
        }


        // Called for every user on connection start
        public void RegisterUser(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
            var user = RedisQueryer.RegisterUser(guid, Context.ConnectionId);
            Clients.Caller.userRegistered(user);
        }

        // Assigns user to chat rooms they were already in;
        // or the newest created
        public Task JoinPreviousChatRoom(string guid)
        {
            var user = RedisQueryer.GetUser(guid);
            var totalRooms = GetTotalNumberOfRooms();

            // Join previous room; else join the most-recently created room
            if (Convert.ToInt32(user.Room) >= 1 &&
                Convert.ToInt32(user.Room) <= Convert.ToInt32(totalRooms))
            {
                RedisQueryer.TransferUser(guid, user.Room, joinRoom: true);
                Clients.Group(user.Room).userHasJoinedRoom(user.Room);

                return Groups.Add(Context.ConnectionId, user.Room);
            } else
            {
                RedisQueryer.TransferUser(guid, totalRooms, joinRoom: true);
                Clients.Group(totalRooms).userHasJoinedRoom(totalRooms);

                return Groups.Add(Context.ConnectionId, totalRooms);
            }
        }
        
        // Returns number of chat rooms available (to the client)
        public void HowManyChatRoomsAvailable()
        {
            var totalRooms = GetTotalNumberOfRooms();
            Clients.Caller.serverHasChatRoomsAvailable(totalRooms);
        }

        // Returns number of users in a particular chat room
        public void HowManyUsersInChatRoom(string room)
        {
            var count = RedisQueryer.ConnectedUsersInChatRoom(room);

            Clients.Caller.chatRoomHasUsers(count);
        }

        // Join chat room
        public Task JoinChatRoom(string guid, string room)
        {
            RedisQueryer.TransferUser(guid, room, joinRoom: true);
            Clients.Group(room).userHasJoinedRoom(room);

            return Groups.Add(Context.ConnectionId, room);
        }

        // Leave chat room
        public Task LeaveChatRoom(string guid, string room)
        {
            RedisQueryer.TransferUser(guid, room, leaveRoom: true);
            Clients.Group(room).userHasLeftRoom(room);

            return Groups.Remove(Context.ConnectionId, room);
        }

        // Sends messages
        public void SendMessage(string room, object data)
        {
            Clients.Group(room).receiveMessage(data);
        }




        public override Task OnConnected()
        {            
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {            
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var guid = RedisQueryer.GetGuidForUser(Context.ConnectionId);

            if (!string.IsNullOrEmpty(guid))
            {
                var user = RedisQueryer.GetUser(guid);
                Clients.Group(user.Room).userHasDisconnected(user.Room);
            
                RedisQueryer.TransferUser(guid, "-1", leaveRoom: true);
            }            
            return base.OnDisconnected(stopCalled);
        }
    }
}
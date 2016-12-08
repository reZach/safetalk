using Owin;
using Microsoft.Owin;
using System.Configuration;
using Microsoft.AspNet.SignalR;
using Chat.Models;

[assembly: OwinStartup(typeof(SignalRChat.Startup))]
namespace SignalRChat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();

            // https://greenfinch.ie/blog/redissingalr.html
            // get the connection string
            var redisConnection = ConfigurationManager.ConnectionStrings["RedisCache"].ConnectionString;
            // set up SignalR to use Redis, and specify an event key that will be used to identify the application in the cache
            GlobalHost.DependencyResolver.UseRedis(new RedisScaleoutConfiguration(redisConnection, "MyEventKey"));
        }
    }
}
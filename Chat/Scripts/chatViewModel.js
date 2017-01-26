// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

// Returns both the 'change' and 'beforeChange' events for a ko subscription
ko.subscribable.fn.subscribeChanged = function (callback) {
    var oldValue;
    this.subscribe(function (_oldValue) {
        oldValue = _oldValue;
    }, null, "beforeChange");
    this.subscribe(function (newValue) {
        callback(newValue, oldValue);
    });
};

// Load sounds from Ion.Sound; http://ionden.com/a/plugins/ion.sound/en.html
ion.sound({
    sounds: [
        {
            name: "button_tiny",
            volume: 0.3,
            preload: false
        }
    ],
    path: "Content/Sounds/",
    preload: true
});




var chatViewModel = function(){
    var _ = this;
    

    /* Properties */
    _.dummy = ko.observable();
    _.guid = ko.observable("");
    _.userCookie = ko.observable("");
    _.name = ko.observable("");
    _.digits = ko.observable("");
    _.chat = $.connection.chatHub;
    _.chatRooms = ko.observableArray();
    _.canSwitchRoom = ko.observable(true);
    _.canSendMessage = ko.observable(true);
    _.selectedRoom = ko.observable(0);
    _.setFocus = ko.observable(true);
    _.message = ko.observable("");
    _.messageLog = ko.observableArray();
    _.messageRelay = ko.observableArray();
    _.messageTitleUpdate = ko.observable(false);
    _.messageCountdownActive = ko.observable(false);
    _.bannedUsers = ko.observable("");
    _.bannedUsersArray = ko.observableArray();
    _.notificationsEnabled = ko.observable(false);
    _.previousUserCountInRoom = ko.observable(-1);
    _.previousUserCountTicker = ko.observable(0);
    _.previousUserCountLongTimer = 7; // represents minutes
    _.displayName = ko.computed(function () {
        return _.name() + ":" + _.digits();
    }, _);
    _.showWarning = ko.computed(function () {
        _.dummy();
        return docCookies.getItem("safetalk_guid") === null;
    }, _);
    



    /* Functions */
    _.setup = function () {

        // Start connection; get guid/user cookie from cache
        $.connection.hub.start().done(function () {

            // Register guid with the server
            var guid = (docCookies.getItem("safetalk_guid") != null) ? docCookies.getItem("safetalk_guid") : "";
            _.chat.server.registerUser(guid);


            // Hook up subscription when changing rooms
            _.selectedRoom.subscribeChanged(function (newValue, oldValue) {

                // oldValue is undefined when this first runs
                if (typeof oldValue !== "undefined" && !isNaN(oldValue) &&
                    typeof newValue !== "undefined" && !isNaN(newValue) &&
                    _.canSwitchRoom() && oldValue != newValue) {
                    _.chat.server.leaveChatRoom(_.guid(), oldValue);
                    _.chat.server.joinChatRoom(_.guid(), newValue);

                    // Only allow switching rooms per 5 seconds to prevent abuse
                    _.canSwitchRoom(false);
                    setTimeout(function () {
                        _.canSwitchRoom(true);
                        _.chat.server.howManyUsersInChatRoom(_.selectedRoom());
                    }, 5000);
                }
            });
        });        
    }

    _.reEvaluateGuidCookie = function () {
        _.dummy.notifySubscribers();
    }

    // This is the function that all messages need to be created through
    _.addToMessages = function (cookie, room, css, message) {
        // Add timestamp to the message
        var time = (new Date()).toLocaleTimeString();

        _.messageLog.push({
            room: room,
            cookie: cookie,
            timestamp: time,
            css: css,
            message: message
        });

        // Scroll to bottom of messages
        $("#js-messages").scrollTop($("#js-messages")[0].scrollHeight);
    }

    // Updates users whose messages should be blocked
    _.updateBannedUsers = function () {

        var users = _.bannedUsers().trim().split(",");

        for (var i = 0; i < users.length; i++) {
            if (users[i].split(":").length == 2) {
                _.bannedUsersArray.push(users[i]);
            }
        }
    }

    // Clear messages from the log from the chat room you are in
    _.clearMessages = function () {
        
        _.messageLog.remove(function (item) {
            return item.room == _.selectedRoom();
        });
        _.setFocus(false);
    }

    _.sendMessage = function () {

        // Can't send message if we are switching chat rooms
        // or sending messages is disabled (temporarily)
        if (_.canSwitchRoom() && _.canSendMessage()) {


            var dateNow = Date.now();
            var canSend = false;

            if (_.messageRelay().length <= 4) {
                _.messageRelay().push(dateNow);
                canSend = true;
            } else {
                var total = _.messageRelay()[_.messageRelay().length - 1] - _.messageRelay()[0];

                // can't post more than 5 messages every 10 seconds
                if (total <= 10000) {
                    _.addToMessages("SERVER", _.selectedRoom(), "warning", "Cannot post more than 5 messages every 10 seconds! 10 second timeout");

                    if (!_.messageCountdownActive()) {
                        _.messageCountdownActive(true);
                        _.canSendMessage(false);

                        setTimeout(function () {
                            _.messageRelay([]);
                            _.messageCountdownActive(false);
                            _.canSendMessage(true);

                            _.addToMessages("SERVER", _.selectedRoom(), "cleared", "You may post messages again");
                        }, 10000);
                    }

                } else {
                    _.messageRelay(_.messageRelay().slice(1, 4));
                    _.messageRelay().push(dateNow);
                    canSend = true;
                }
            }


            if (canSend) {

                // Do not allow empty messages to be sent
                if (_.message().trim() != "") {
                    _.chat.server.sendMessage(_.selectedRoom(), {
                        room: _.selectedRoom(),
                        cookie: _.userCookie(),
                        message: htmlEncode(_.message())
                    });
                    _.message("");
                    _.setFocus(true);
                }
            }
        }
    }

    // Is called after you accept the warning for the chat room
    _.enterChatRoom = function () {

        var name = htmlEncode(window.prompt("Enter your public chat name:", "").trim());
        var digits = /\d{4}/.exec(Math.random());
        var cookie = htmlEncode(name + ":" + digits);

        _.name(name);
        _.digits(digits);
        _.userCookie(cookie);
        docCookies.setItem("safetalk_uniquekey", cookie);

        _.setup();
    }
    




    /* Client-hub functions */
    _.chat.client.userRegistered = function (user) {
        
        // Set guid
        _.guid(user.Guid);
        docCookies.setItem("safetalk_guid", user.Guid);          

        // Force-evaluate knockout computed to refresh
        _.reEvaluateGuidCookie();

        // If user has already been in chat room before
        if (!_.showWarning()) {            

            // Re-set user's display name
            var cookie = docCookies.getItem("safetalk_uniquekey");
            _.userCookie(cookie);
            _.name(cookie.split(":")[0]);
            _.digits(cookie.split(":")[1]);

            // Set room
            _.selectedRoom(parseInt(user.Room, 10));

            // Place user in chat room that they were in last or most newly-created
            _.chat.server.joinPreviousChatRoom(_.guid());

            // Load chat rooms
            _.chat.server.howManyChatRoomsAvailable();

            // Query how many users are in this chat room
            _.chat.server.howManyUsersInChatRoom(parseInt(user.Room, 10));

            // Start interval for querying number of users in current chat room
            _.interval();
        }
    }

    _.chat.client.serverHasChatRoomsAvailable = function (rooms) {

        // Create these chat rooms client-side
        _.chatRooms.removeAll();
        for (var i = 0; i < rooms; i++) {
            _.chatRooms.push({
                name: "Room " + (i + 1),
                value: (i + 1)
            });
        }
    }

    _.chat.client.chatRoomHasUsers = function (number) {
        var message = "";

        if (number <= 0) {
            message = "No users in this room";
        } else if (number == 1) {
            message = "1 user in this room";
        } else if (number >= 2) {
            message = number + " users in this room";
        }



        // If this is the first time we are printing this message;
        // print information about the Facebook page/invitations
        if (_.previousUserCountInRoom() == -1) {

            _.addToMessages("SERVER", _.selectedRoom(), "server", message);
            _.addToMessages("SERVER", _.selectedRoom(), "server", "Please like us on <a href='https://www.facebook.com/safeplacetotalk' target='_blank'>Facebook</a>");

            if (number <= 1) {
                _.addToMessages("SERVER", _.selectedRoom(), "server", "No one else online? Submit and view chat invitations <a href='/invitation'>here</a> to see when and what others want to talk about");
            }
        } else {

            // If the number of users has changed since this last was called;
            // OR if we hit the long timer and need to display a message
            var usersHasChanged = number != _.previousUserCountInRoom();
            var timerIsHit = (_.previousUserCountTicker() * 5000) > (_.previousUserCountLongTimer * 60 * 1000)

            if (usersHasChanged) {
                _.addToMessages("SERVER", _.selectedRoom(), "server", message);
            } else if (timerIsHit) {
                _.addToMessages("SERVER", _.selectedRoom(), "server", message);

                // Reset
                _.previousUserCountTicker(0);
            }
        }

        _.previousUserCountInRoom(number);
    }

    _.chat.client.receiveMessage = function (data) {

        // Don't show messages of banned users
        for (var i = 0; i < _.bannedUsersArray().length; i++) {
            if (_.bannedUsersArray()[i].split(":")[0] == data.cookie.split(":")[0] && _.bannedUsersArray()[i].split(":")[1] == cookie.split(":")[1]) {
                return;
            }
        }

        // Update title of tab to notify the user we have a message
        if (!_.messageTitleUpdate()) {
            _.messageTitleUpdate(true);
            var oldTitle = document.title;
            document.title = "new message!";

            // Wait 3 seconds until swapping back to old title
            setTimeout(function () {
                document.title = oldTitle;
                _.messageTitleUpdate(false);
            }, 3000);
        }

        // Play sound if user has notifications enabled
        /*if (_.notificationsEnabled()) {
            ion.sound.play("button_tiny");
        }*/

        _.addToMessages(data.cookie, data.room, "client", data.message);        
    }

    _.chat.client.userHasDisconnected = function (room) {

        if (_.selectedRoom() == room) {
            _.addToMessages("SERVER", _.selectedRoom(), "server", "User has disconnected");

            // Query how many users are in this chat room
            _.chat.server.howManyUsersInChatRoom(_.selectedRoom());
        }
    }



    // Ask the user if they want notifications
    if ("Notification" in window) {
        Notification.requestPermission(function (permission) {

            // If the user accepts
            if (permission === "granted") {
                _.notificationsEnabled(true);
            }
        });
    }

    // Run this on a timer to check how many users are in our chat room
    _.interval = function () {
        setInterval(function () {
            _.chat.server.howManyUsersInChatRoom(_.selectedRoom());
            _.previousUserCountTicker(_.previousUserCountTicker() + 1);
        }, 5000);
    }    
};

var vm = new chatViewModel();
if (!vm.showWarning()) {
    vm.setup();
}
ko.applyBindings(vm, $(".js-knockoutbindingtarget")[0]);



// Check for ad block;
/*var cssStyleDeclaration = window.getComputedStyle(document.getElementsByClassName("google-ad")[0], ":content");
var adDisplay = cssStyleDeclaration.getPropertyValue("display");
if (adDisplay == "none"){
    $(".google-ad").removeClass("google-ad").addClass("alert alert-warning").text("The ads we serve are meant to support the features we provide for free to you on the site. Please consider whitelisting our site and support us");
}*/
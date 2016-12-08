$(function () {

    var hookupChat = function () {

        // Reference the auto-generated proxy for the hub.
        window.chat = $.connection.chatHub;

        // Create a function that the hub can call back to display messages.
        window.chat.client.addNewMessageToPage = function (name, message, cookie) {

            var dateNow = new Date();

            // don't display message of banned user
            for (var i = 0; i < window.bannedUsers.length; i++) {
                if (window.bannedUsers[i].split(":")[0] == name && window.bannedUsers[i].split(":")[1] == cookie.split(":")[1]) {
                    return;
                }
            }

            // Add the message to the page.
            $("#discussion").append("<li>" + dateNow.toLocaleTimeString() + " <strong>" + htmlEncode(name) + ":" + cookie.split(":")[1] + "</strong> " + htmlEncode(message) + "</li>");
            $("#discussion").scrollTop($("#discussion")[0].scrollHeight);
        };

        // Logs to chat how many users are online
        window.chat.client.online = function (online) {

            if (online == 1) {
                $("#discussion").append("<li class='server-message'>(Server) There is " + online + " user connected</li>");
            } else if (online >= 2) {
                $("#discussion").append("<li class='server-message'>(Server) There are " + online + " users connected</li>");
            }
        }

        // Sends updates on connects/disconnects
        window.chat.client.updateOnlineCount = function (string) {
            $("#discussion").append("<li class='server-message'>(Server) User " + string + "</li>");
        }

        // Start the connection.
        $.connection.hub.start().done(function () {
            $('#sendmessage').click(function () {
                sendMessage();
            });

            window.chat.server.online();
        });
    }

    var getPublicChatName = function () {

        if (docCookies.getItem("uniquekey") === null) {

            // Get the user name and store it to prepend to messages.
            $('#displayname').val(prompt('Enter your public chat name:', '').trim());

            var random = Math.random();
            var name = $("#displayname").val() + ":" + /\d{4}/.exec(random)

            $("#user-name").html(htmlEncode(name));
            docCookies.setItem("uniquekey", name);

        } else {
            var name = docCookies.getItem("uniquekey").split(":")[0];

            $("#displayname").val(name);
            $("#user-name").html(htmlEncode(docCookies.getItem("uniquekey")));
        }
    }


    var sendMessage = function () {

        if ($("#message").val().length > 0) {

            var dateNow = Date.now();
            var canSend = false;

            if (window.messageRelay.length <= 4) {
                window.messageRelay.push(dateNow);
                canSend = true;
            } else {
                var total = window.messageRelay[window.messageRelay.length - 1] - window.messageRelay[0];

                // can't post more than 5 messages every 10 seconds
                if (total <= 10000) {
                    $("#frequent-poster-cleared").hide();
                    $("#frequent-poster").show();

                    if (!window.timerActive) {
                        window.timerActive = true;

                        setTimeout(function () {
                            window.messageRelay = [];
                            window.timerActive = false;
                            $("#frequent-poster").hide();

                            $("#frequent-poster-cleared").show();
                            setTimeout(function () {
                                $("#frequent-poster-cleared").hide();
                            }, 3000);
                        }, 10000);
                    }

                } else {
                    window.messageRelay = window.messageRelay.slice(1, 4);
                    window.messageRelay.push(dateNow);
                    canSend = true;
                }
            }

            if (canSend) {

                // Call the Send method on the hub.
                window.chat.server.send($('#displayname').val(), $('#message').val(), docCookies.getItem("uniquekey"));
                // Clear text box and reset focus for next comment.
                $('#message').val('').focus();
            }
        }
    }

    var init = function () {

        window.bannedUsers = [];
        window.messageRelay = [];
        window.timerActive = false;

        getPublicChatName();
        hookupChat();

        // Set initial focus to message input box.
        $('#message').focus();
    }

    $("#message").keypress(function (e) {
        if (e.which == 13) {
            sendMessage();
        }
    })

    $("#clear").on("click", function () {
        $("#discussion").empty();
        $('#message').focus();
    });

    $("#enter").on("click", function () {
        $(".main").show();
        $(".warning").hide();

        init();
    });

    $("#banned-users").blur(function () {

        var users = $("#banned-users").val().trim().split(",");

        window.bannedUsers = [];
        for (var i = 0; i < users.length; i++) {
            if (users[i].split(":").length == 2) {
                window.bannedUsers.push(users[i]);
            }
        }
    });

    
    // auto-load chat if we already accepted the warning
    if (docCookies.getItem("uniquekey") !== null) {
        $(".main").show();
        $(".warning").hide();

        init();
    } else {
        $(".warning").show();
    }
});


var app = angular.module("chat", ["ui.router"])
    .config(["$stateProvider", "$urlRouterProvider", function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state({
            name: "warning",
            url: "/",
            template: "warning.html"
        });

        $stateProvider.state({
            name: "browse",
            url: "/browse",
            template: "browse.html",
            controller: "RoomBrowserController"
        });

        $stateProvider.state({
            name: "chat",
            url: "/chat",
            template: "chat.html",
            controller: "ChatRoomController"
        });

        $urlRouterProvider.otherwise("warning");
    }])
    .controller("RoomBrowserController", function RoomBrowserController($scope) {

    })
    .controller("ChatRoomController", function ChatRoomController($scope) {

    });
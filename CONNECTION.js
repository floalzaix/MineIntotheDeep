import * as signalR from "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js";

const connection = new signalR.HubConnectionBuilder()
        .withUrl("/playershub")
        .build();

connection.start()

connection.on("MESSAGE", function (message) {

})


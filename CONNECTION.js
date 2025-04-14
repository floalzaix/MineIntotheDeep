import * as signalR from "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js";

// Here the Microsoft signalR lib is used but check for the languages available

const connection = new signalR.HubConnectionBuilder()
        .withUrl("/playershub")
        .build();

connection.start();

connection.on("MESSAGE", function (message) {
    console.log("De serveur : ", message);
})

let query = "DEPLACER|0|1|2"; // String

connection.invoke("QueryAsync", query);


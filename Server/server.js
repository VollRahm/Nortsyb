const password = require("./password");

(async() => {
    const express = require("express");
    const constants = require("./constants");
    const bodyParser = require("body-parser");
    const RequestHandler = require("./RequestHandler");
    const http = require("http");

    var app = express();
    app.use(bodyParser.json());
    app.use(bodyParser.urlencoded({extended: true}));

    app.post("/api/auth", RequestHandler.auth);
    app.post("/api/listUsers", RequestHandler.listUsers);
    app.post("/api/send", RequestHandler.send);

    var server = http.createServer(app);
    
    server.listen(constants.API_PORT, ()=> console.log("REST API started listening on port " + constants.API_PORT));
})();


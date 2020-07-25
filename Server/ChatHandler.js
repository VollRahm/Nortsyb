const ws = require("ws");
const dbManager = require("./DBManager");
const wss = new ws.Server({port: 3001});

/**
 * @type {WebSocket[]} connectedClients
 */
var connectedClients = [];

wss.on('connection', async(socket, req)=>{
    socket.on("message", async(msg)=>{
        if(msg.startsWith("auth:")){
            var token = msg.slice(5);
            var auth = await dbManager.checkToken(token);
    
            if(auth == null){
                console.log(req.socket.remoteAddress + " failed to authenticate.");
                socket.close();
                return;
            }
            socket.send("auth:success");
        connectedClients[auth.id] = socket;
        }
    });

    socket.on('close', (code, reason)=>{
        connectedClients[connectedClients.indexOf(socket)] = null;
    });
});

async function sendMessage(sender, id, message){
    if(connectedClients[id] == null) return false;
    if(connectedClients[sender.id] == null) return true;

    connectedClients[id].send("sender:"+sender.id+"msg:" + message);
    return true;
}

module.exports={
    sendMessage
}
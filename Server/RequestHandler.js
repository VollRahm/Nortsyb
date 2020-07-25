const dbManager = require("./DBManager");
const chatHandler = require("./ChatHandler");

function checkContentType(req){
    if(req.headers["content-type"] != "application/json"){
        return false;
    }else return true;
}

/**
@param {import("express").Request} req
@param {import("express").Response} res
 */
async function auth(req, res){
    if(!checkContentType(req)){
        res.status(400).end();
    }

    try{
        var login = await dbManager.authenticate(req.body.username, req.body.password);
        if(login == "error") res.status(500).end();
        else if(login == "not found" || login == "pw") res.status(403).end();
        else{
            res.status(200).json({token: login.token});
        }
    }catch{
        res.status(500).end();
    }
}

/**
@param {import("express").Request} req
@param {import("express").Response} res
 */
async function listUsers(req, res){
    if(!checkContentType(req)){
        res.status(400).end();
    }

    try{
        var auth = await dbManager.checkToken(req.body.token);
        if(auth == null){
            res.status(403).end();
            return;
        }
        var users = await dbManager.getUsers();
        res.status(200).json(users);
    }catch{
        res.status(500).end();
    }
}

/**
@param {import("express").Request} req
@param {import("express").Response} res
 */
async function send(req, res){
    if(!checkContentType(req)){
        res.status(400).end();
    }

    try{
        var auth = await dbManager.checkToken(req.body.token);
        if(auth == null){
            res.status(403).end();
            return;
        }
        chatHandler.sendMessage(auth, req.body.id, req.body.message);
        res.status(200).end();
    }catch{
        res.status(500).end();
    }
}

module.exports={
    auth, listUsers, send
}
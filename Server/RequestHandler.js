const bodyParser = require("body-parser")

const dbManager = require("./DBManager");

/**
@param {import("express").Request<ParamsDictionary, any, any, qs.ParsedQs>} req
@param {import("express").Response} res
 */

async function auth(req, res){
    if(req.headers["content-type"] != "application/json"){
        res.status(400).end();
        return;
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

module.exports={
    auth
}
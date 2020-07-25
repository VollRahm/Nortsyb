let db = require("./database");
let passwordCheck = require("./password");

function authenticate(username, password){
    var cmd = "SELECT id, password, token FROM users where username=?;"
    
    return new Promise((res, rej)=>{
        db.get(cmd, [username], async(err, dbRes)=>{
            if(err) res("error");
            if(dbRes == null) res("not found");
            
            var pwSuccess = await passwordCheck.checkPassword(password, dbRes.password);
            if(pwSuccess) res(dbRes);
            else res("pw");
        });
    })
}

function getUsers(){
    var cmd = "SELECT id, username FROM users;";
    return new Promise((res, rej)=>{
        db.all(cmd, (err, dbRes)=>{
            if(err) res(null);
            res(dbRes);
        });
    })
}

async function checkToken(token){
    var cmd = "SELECT id, username FROM users WHERE token=?";
    return new Promise((res, rej)=>{
        db.get(cmd, [token], (err, dbRes)=>{
            if(err) res(null);
            res(dbRes);
        });
    });
}

module.exports={
    authenticate, getUsers, checkToken
}

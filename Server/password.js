const bcrypt = require("bcrypt");


async function hash(password){
    return await bcrypt.hash(password, require("./constants").PASSWORD_SALT);
}

async function checkPassword(password, hash){
    return await bcrypt.compare(password, hash);
}

function getRandomToken(){
    return new Promise((res, rej)=>{
        require('crypto').randomBytes(32, function(err, buffer) {
            var token = buffer.toString('hex');
            res(token);
          });
    });
}

module.exports={
    hash, getRandomToken, checkPassword
}
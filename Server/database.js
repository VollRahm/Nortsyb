const sqlite = require("sqlite3");
const constants = require("./constants");
var db = new sqlite.Database(constants.DB_PATH, sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE);
module.exports = db;
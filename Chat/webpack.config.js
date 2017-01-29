var path = require("path");

module.exports = {
    context: path.join(__dirname, "Scripts"),
    entry: "./main.ts",
    output: {
        path: path.join(__dirname, "Scripts-Build"),
        filename: "[name].bundle.js"
    }
}
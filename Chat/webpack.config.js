var path = require("path");

module.exports = {
    context: path.join(__dirname, "Scripts"),
    entry: "./main.ts",
    output: {
        path: path.join(__dirname, "Scripts-Build"),
        filename: "./[name].bundle.js"
    },
    resolve: {
        // Add ".ts" as a resolvable extension
        extensions: ["", ".ts", ".js"]
    },
    module: {
        // This will be renamed to "rules" in the future
        loaders: [
            // all files with a ".ts" extension will be handled by "ts-loader"
            {
                test: /\.ts$/,
                loader: "ts-loader",
                options: {
                    visualStudioErrorFormat: true
                }
            }
        ]
    }
}
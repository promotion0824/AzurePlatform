const path = require("path");
const CopyWebpackPlugin = require("copy-webpack-plugin");

module.exports = {
  mode: 'development',
  devtool: 'inline-cheap-source-map',
  entry: "./src/main.ts",
  target: ['web', 'es5'],
  output: {
    hashFunction: "xxhash64",
    path: path.resolve("./out"),
    filename: "dnsconfig.js",
  },
  resolve: {
    extensions: [".ts", ".js"],
    modules: ["src", "node_modules"].map((x) => path.resolve(x)),
  },
  module: {
    rules: [
      {
        test: /\.ts$/,
        use: "ts-loader",
      },
    ],
  },
  plugins: [
    new CopyWebpackPlugin({
      patterns: [{ from: "./creds-template.json", to: "./creds.json" }],
    }),
  ],
  optimization: {
    minimize: false,
  },
};

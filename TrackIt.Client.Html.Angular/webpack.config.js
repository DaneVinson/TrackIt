var webpack = require('webpack');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var path = require('path');

module.exports = {
    entry: {
        'index.app': './app/main.ts',
        'index.polyfills': [
          'core-js/es6',
          'core-js/es7/reflect',
          'zone.js/dist/zone'
        ]
    },
    context: __dirname,
    output: {
        path: path.join(__dirname, "deploy"),
        filename: '[name].[hash].js'
    },
    resolve: { extensions: ['.js', '.ts'] },
    module: {
        rules: [
            { test: /\.ts$/, use: ['awesome-typescript-loader', 'angular2-template-loader'] },
            { test: /\.html$/, use: ['html-loader'] },
            { test: /\.css$/, use: ['style-loader', 'css-loader'] },
            { test: /\.json$/, use: ['json-loader'] },
            { test: /\.(jpg|png|svg)$/, use: ['file-loader'] }
        ],
    },
    plugins: [
        new webpack.optimize.CommonsChunkPlugin({
            name: 'index.polyfills'
        }),
        new webpack.optimize.UglifyJsPlugin(),
        new HtmlWebpackPlugin({
            template: './index.html'
        })
    ]
};

var fs = require('fs');
var express = require('express'),
    app = express();

app.use(express.bodyParser());

app.post('/', function(request, response){
    console.log(request.body);
    response.send(request.body);
    exec("git pull", null);
    fs.writeFile("./test.log", request.body, function(err) {
        if(err){
            console.log(err);
        } else {
            console.log("Save'd");
        }
    });
});

app.listen(7001);
/*
var sys = require('sys')
var exec = require('child_process').exec;
function gibe(error, stdout, stderr) { 
    console.log(stdout);
    fs.writeFile("./test.log", stdout, function(err) {
        if(err){
            console.log(err);
        } else {
            console.log("Save'd");
        }
    });
}
exec("testink", gibe);
*/



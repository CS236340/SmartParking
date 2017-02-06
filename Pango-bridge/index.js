// Usage:
// from the PangoBridge diractory, node index.js -p 9002
//if you want to change the port number, use node index.js -r -u "http://localhost:portNumber", after that you need to restart the server


var config=require('../pangoConfiguration.json');
var ROOM_ID=config.roomId;
var http = require("http");
var qs = require("querystring"); 
var requestLib = require("request"); 
var bridge;
var URL=config.pangoUrl; // our Mock Url, not Pango real Url. any change should be made in the configuration file

/* IMPORTANT!!!
Because we don't have Pango actual API, we used our own Mock, based on assumption we made regarding how thay expect
to got information from us and how thay send their response
Keep in mind that the actual API may be diffrent, and the JSON object we send may change
any such change should be done in the 'body' variable
*/
http.createServer(function(request, response) {
	console.log(request.method + " " + request.url);
	var body = "";
	request.on("data", function(chunk) {
		body += chunk;
	});
	request.on("end", function() {
	console.log(body);
	var params=JSON.parse(body);
	params.subscription=1;
	var intent = bridge.getIntent("@Pango_pango:my.domain.name")
	intent.sendText(ROOM_ID, JSON.stringify(params));
        response.writeHead(200, {"Content-Type": "application/json"});
        response.write(body);
        response.end();
    });
}).listen(config.pangoBridgePort,config.IP);

var Cli = require("matrix-appservice-bridge").Cli;
var Bridge = require("matrix-appservice-bridge").Bridge;
var AppServiceRegistration = require("matrix-appservice-bridge").AppServiceRegistration;
var path=config.pangoRegistrationPath;
var domain=config.matrixDomain;
var serverUrl=config.matrixUrl;
new Cli({
    registrationPath: config.pangoRegistrationPath,
    generateRegistration: function(reg, callback) {
        reg.setId(AppServiceRegistration.generateToken());
        reg.setHomeserverToken(AppServiceRegistration.generateToken());
        reg.setAppServiceToken(AppServiceRegistration.generateToken());
        reg.setSenderLocalpart("pangobot");
        reg.addRegexPattern("users", "@Pango_.*", true);
        callback(reg);
    },
    run: function(port, cfg) {
		bridge = new Bridge({
			homeserverUrl:"http://localhost:8008",
			domain: "localhost",
			registration: "pango-registration.yaml",
			controller: {
				onUserQuery: function(queriedUser) {
					return {}; // auto-provision users with no additonal data
				},
				onEvent: function(request, context) {
					var event = request.getData();
					if (event.type !== "m.room.message" || !event.content || event.room_id !== ROOM_ID || event.sender!=="@SPM_spm:my.domain.name") {;
				return;
				}
				var param={};
				try{	
					param=JSON.parse(event.content.body);
					if (param.subscription!=1){
						return;
					}
				}
				catch(e){ return;}
/* IMPORTANT!!!
Because we don't have Pango actual API, we used our own Mock, based on assumption we made regarding how thay expect
to got information from us and how thay send their response
Keep in mind that the actual API may be diffrent, and the JSON object we send may change
any such change should be done in the 'MyBody' variable
*/
				var myBody = {};
				myBody.carId = param.carId;
				myBody.refId = param.refId;
				myBody.time = param.time;
				myBody.action=param.action;
				requestLib({
					method: "POST",
					json: true,
					uri: URL,
					body: myBody
				}, function(err, res) {
					if (err || res.statusCode!==200) {
						console.log("HTTP Error: %s", err);
						var myResp={carId:param.carId,refId:param.refId,responseCode:7,info:{info:res.statusCode,responseCode:7},subscription:1}
						var intent = bridge.getIntent	(config.matrixUsernameAndDomain)
						intent.sendText(ROOM_ID, JSON.stringify(myResp));
					}
					else {}
					});
				}
			}
		});
	    
		console.log("Matrix-side listening on port %s", port);
		bridge.run(port, cfg);
	}
}).run();

// Usage:
//from the CpBridge diractory, node index.js -p 9001
//if you want to change the port number, use node index.js -r -u "http://localhost:portNumber", after that you need to restart the server

var config=require('../celloParkConfiguration.json'); // JSON configuration file, if you move it from the .synapse directory, make sure to update the route
var ROOM_ID = config.roomId;
var http = require("http");
var qs = require("querystring");
var requestLib = require("request"); 
var bridge;
var URL=config.cpUrl; // our Mock Url, not CelloPark real Url. any change should be made in the configuration file

/* IMPORTANT!!!
because we don't have CellopPark actual API, we used our own Mock, based on information we got from CelloPark regarding how thay expect
to got information from us and how thay send their response
keep in mind that the actual API may be diffrent, and the JSON object we send may change
any such change should be done in the 'body' variable
*/
http.createServer(function(request, response) {
	console.log(request.method + " " + request.url);
	var body = "";
	request.on("data", function(chunk) {
		body += chunk;
	});
	request.on("end", function() {
	var params=JSON.parse(body);
	params.subscription=0;
	var intent = bridge.getIntent(config.matrixUsernameAndDomain)
	intent.sendText(ROOM_ID, JSON.stringify(params));
		response.writeHead(200, {"Content-Type": "application/json"});
		response.write(body);
		response.end();
	});
}).listen(config.cpBridgePort,config.IP);

var Cli = require("matrix-appservice-bridge").Cli;
var Bridge = require("matrix-appservice-bridge").Bridge;
var AppServiceRegistration = require("matrix-appservice-bridge").AppServiceRegistration;
var path=config.cpRegistrationPath;
var domain=config.matrixDomain;
var serverUrl=config.matrixUrl;
new Cli({
	registrationPath: path,
	generateRegistration: function(reg, callback) {
		reg.setId(AppServiceRegistration.generateToken());
		reg.setHomeserverToken(AppServiceRegistration.generateToken());
		reg.setAppServiceToken(AppServiceRegistration.generateToken());
		reg.setSenderLocalpart("celloparkbot");
		reg.addRegexPattern("users", config.matrixRegexUsersDomain, true);
		callback(reg);
	},
	run: function(port, cfg) {
		bridge = new Bridge({
			homeserverUrl: serverUrl,
			domain: domain,
			registration: path,
			controller: {
				onUserQuery: function(queriedUser) {
					return {}; // auto-provision users with no additonal data
				},
				onEvent: function(request, context) {
					var event = request.getData();
					if (event.type !== "m.room.message" || !event.content || event.room_id !== ROOM_ID || event.sender!=="@SPM_spm:my.domain.name") {
						return;
					}
				var param={};
				try{	
					param=JSON.parse(event.content.body);
					if (param.subscription!=0){
						return;
					}
				}
				catch(e){ return;}
/* IMPORTANT!!!
because we don't have CellopPark actual API, we used our own Mock, based on information we got from CelloPark regarding how thay expect
to got information from us and how thay send their response
keep in mind that the actual API may be diffrent, and the JSON object we send may change
any such change should be done in the 'myBody' variable
*/
				var myBody = {};
				myBody.carId = param.carId;
				myBody.refId = param.refId;
				myBody.time = param.time;
				myBody.action = param.action;
				requestLib({
				method: "POST",
				json: true,
				uri: URL,
				body: myBody
				}, function(err, res) {
					if (err || res.statusCode!==200) {
						console.log("HTTP Error: %s", err);
						var myResp={carId:param.carId,refId:param.refId,responseCode:7,info:{info:res.statusCode,responseCode:7},subscription:0}
						var intent = bridge.getIntent(config.matrixUsernameAndDomain);
						intent.sendText(ROOM_ID, JSON.stringify(myResp));
					}
					else {}
					}
				);
				}
			}
		});
		console.log("Matrix-side listening on port %s", port);
		bridge.run(port, cfg);
	}
}).run();

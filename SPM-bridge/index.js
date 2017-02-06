// Usage:
// from the SpmBridge diractory, node index.js -p 9000
//if you want to change the port number, use node index.js -r -u "http://localhost:portNumber", after that you need to restart the server


var config=require('../spmConfiguration.json');
var http = require("http");
var qs = require("querystring");
var requestLib = require("request");
var bridge;
var URL = config.SPMUrl;  // our Mock Url, not the SPM real Url. any change should be made in the configuration file
var ROOM_ID=config.roomId;
var cpTimer,pangoTimer;

/*IMPORTNAT!!!
Based on the technion Smart Parking Manager API. 
Because that project wasn't complete at the time, we used our own Mock, based on the specifitcaion from the Parking Manager project
*/
http.createServer(function(request, response) {
	console.log(request.method + " " + request.url);
	var body = "";
	request.on("data", function(chunk) {
		body += chunk;
	});
    request.on("end", function() {
		var params=JSON.parse(body);
		var intent = bridge.getIntent(config.matrixUsernameAndDomain)
		intent.sendText(config.roomId, JSON.stringify(params));
		//we set a timer for every subscription, if the provider bridge is down we need to inform the client(the spm)
		if(params.subscription==0){
			cpTimer=setTimeout(function(){timeoutFunction		(params)},config.providerBridgeTimeout);
		}
		if(params.subscription==1){
			pangoTimer=setTimeout(function(){timeoutFunction(params)},config.providerBridgeTimeout);
		}
		response.writeHead(200, {"Content-Type": "application/json"});
		response.write(JSON.stringify(params.refId));
		response.end();
	});
}).listen(config.spmBridgePort,config.IP);

var Cli = require("matrix-appservice-bridge").Cli;
var Bridge = require("matrix-appservice-bridge").Bridge;
var AppServiceRegistration = require("matrix-appservice-bridge").AppServiceRegistration;
var path=config.spmRegistrationPath;
var domain=config.matrixDomain;
var serverUrl=config.matrixUrl;
new Cli({
    registrationPath: config.spmRegistrationPath,
    generateRegistration: function(reg, callback) {
		reg.setId(AppServiceRegistration.generateToken());
		reg.setHomeserverToken(AppServiceRegistration.generateToken());
		reg.setAppServiceToken(AppServiceRegistration.generateToken());
		reg.setSenderLocalpart("kbot");
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
						if (event.type !== "m.room.message" || !event.content || event.room_id !== ROOM_ID || event.sender=="@SPM_spm:my.domain.name") {
							return;
						}
						var param={};
						try{	
							param=JSON.parse(event.content.body);
						}
						catch(e){return; }
					//when the provider bridge send his response, we turn off the timer
					if(param.subscription==0){			
						clearTimeout(cpTimer);
					}
					if(param.subscription==1){
						clearTimeout(pangoTimer);
					}
					delete param.subscription;
					requestLib({
						method: "POST",
						json: true,
						uri:URL,
						body: param
						}, function(err, res) {
							if (err || res.statusCode!==200) {
									resendFunction(param);
							}else {}
						});
					}
				}
			});
			console.log("Matrix-side listening on port %s", port);
			bridge.run(port, cfg);
		}
}).run();

//if the spm is down, we make another attempt to send the massege
var resendFunction = function(param){
requestLib({
	method: "POST",
	json: true,
	uri: config.SPMUrl,
	body: param
	}, function(err, res) {
		if (err) {
			console.log("HTTP Error: %s", err);
			}
})};
//if no response came from the provider Bridge, we let the spm know, with response code 8
var timeoutFunction = function(params){
	var myResp={carId:params.carId,refId:params.refId,responseCode:8,info:{info:"Provider "+ params.subscription + " Bridge Not Avilable",responseCode:8}};
	f(myResp);

}





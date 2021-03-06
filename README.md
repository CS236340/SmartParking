# SmartParking
Finding a parking slot in the Technion has become a cumbersome thing. Employees and many other students are having trouble finding a suitable parking slot for their vehicles.
The Smart Parking project came to solve those problems - ‘Connecting’ the parking slots to the network.

This Project was created to provide a mean of communication between the technion's Smart Parking Manager(SPM) and various Billing providers such as CelloPark and Pango

We used matrix.org chat in order to relay masseges from the SPM to the providers, using Bridges in the matrix system.

We currently have support for 2 billing providers - CelloPark and Pango, each provider have his own bridge

***IMPORTANT***
Because we don't have CellopPark's actual API, we used our own Mock, based on information we got from CelloPark regarding how thay expect to got information from us and how thay send their response.

Because we don't have Pango's actual API, we used our own Mock, based on assumption we made regarding how thay expect to got information from us and how thay send their response.

Keep in mind that the actual API may be diffrent, and the JSON object we send may change. see comments in the code itslef


***SetUp Insructions***

You need to run a homeserver. we used synapse server, from https://github.com/matrix-org/synapse
just replace the `homeserver.yaml` file that was created with the file we provided.

Download the 3 configuration files into the synapse directory. update the server IP in each of the files, and the providers URL.

We used our Mock's URL's, not CelloPark's/Pango's real URL

Create the Bridge diretory for every provider and the SPM. we gave them the names spmBridge, celloParkBridge & PangoBridge.
if you change the names, you need to change the `app_services_config_files` field in the `homeserver.yaml` file.

Create each bridge, foloowing the instruction on https://github.com/matrix-org/matrix-appservice-bridge/blob/master/HOWTO.md

Replace the `index.js` file with the one we provided, and download the correct `registration.yaml` file to the directory(`spm-registration.yaml` to the `spmBridge` directory etc.)

***Running Instructions***

1. first you need to start the matrix server, in the synapse directory

    `cd ~/.synapse
    
    source ./bin/activate
    
    synctl start`
    
2. you need to start each of the bridges, each one in his own dircetory
the spm bridge:

  `cd ~/.synapse/SpmBridge
  
  node index.js -p 9000`
  
Cello Park bridge:

  `cd ~/.synapse/CpBridge
  
  node index.js -p 9001`
  
Pango Bridge:

  `cd ~/.synapse/PangoBridge
  
  node index.js -p 9002`
  
All this is true as long as you don't change the location of the Bridge's Directorys or the port number
if you wish to change the port number, you need to stop the homeserver using

   `synctl stop`
   
and then run, in the bridge directory:

   ` node index.js -r -u "http://localhost:portNumber"`
   
where portNumber is the port you want to set.

after that, restart the homeserver using

     `synctl start`
 


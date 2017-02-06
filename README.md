# SmartParking
Finding a parking slot in the Technion has become a cumbersome thing. Employees and many other students are having trouble finding a suitable parking slot for their vehicles.
The Smart Parking project came to solve those problems - ‘Connecting’ the parking slots to the network
This Project was created to provide a mean of communication between the technion's Smart Parking Manager(SPM) and various Billing providers such as CelloPark and Pango

We used matrix.org chat in order to relay masseges from the SPM to the providers, using Bridges in the matrix system.

We currently have support for 2 billing providers - CelloPark and Pango, each provider have his own bridge

in order to run the systam:
1. first you need to start the matrix server, in the synapse directory
    cd ~/.synapse
    source ./bin/activate
    synctl start
    
2. you need to start each of the bridges, each one in his own dircetory
the spm bridge:
  cd ~/.synapse/SpmBridge
  node index.js -p 9000
Cello Park bridge:
  cd ~/.synapse/CpBridge
  node index.js -p 9001
Pango Bridge
  cd ~/.synapse/PangoBridge
  node index.js -p 9002
 


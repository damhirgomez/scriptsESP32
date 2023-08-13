/*
ProxSIMity BleScanner multiconnection
@By: Damhir Gomez
*/

#include <ArduinoBLE.h>
#include <algorithm>
#include <vector>
String savedDevices[] = {"ProxSIMityEyeDrop001", "ProxSIMityGlove001"};
BLEDevice peripherals[sizeof(savedDevices)/sizeof(savedDevices[0])];
std::vector<BLECharacteristic> characteristicPointers;
int numDevices = 0;
int count = 0;
void setup() {
  Serial.begin(9600);
  while (!Serial);

  // begin initialization
  if (!BLE.begin()) {
    Serial.println("starting Bluetooth® Low Energy module failed!");
    
    while (1);
  }

  Serial.println("Bluetooth® Low Energy Central - SensorTag button");
  Serial.println("Make sure to turn on the device.");
  Serial.println("Tamano ble devies"+sizeof(savedDevices));
  // start scanning for peripheral
  BLE.scan();
}

void loop() {
  // check if a peripheral has been discovered
  BLEDevice peripheral = BLE.available();
  
  if (peripheral) {
    // discovered a peripheral, print out address, local name, and advertised service
    Serial.print("Found ");
    Serial.print(peripheral.address());
    Serial.print(" '");
    Serial.print(peripheral.localName());
    Serial.print("' ");
    Serial.print(peripheral.advertisedServiceUuid());
    Serial.print("size devices");
    Serial.print(sizeof(savedDevices)/sizeof(savedDevices[0]));
    Serial.println();

    // Check if the peripheral is a SensorTag, the local name will be:
    // "CC2650 SensorTag"
    if (std::find_if(std::begin(savedDevices), std::end(savedDevices), [&](const String& device) { return device == peripheral.localName(); }) != std::end(savedDevices)) {
     if (numDevices < sizeof(savedDevices)/sizeof(savedDevices[0])) {
        Serial.print("Add: ");
        Serial.print(peripheral.localName());
        Serial.println();
        peripherals[numDevices] = peripheral;
        numDevices++;
     }else{
        connectSensors(peripherals);
     }
    }
  }
}

void connectSensors(BLEDevice peripherals[]) {
    BLE.stopScan();
    for (int i = 0; i < numDevices; i++) {
      Serial.print("numDevices: ");
      Serial.print(numDevices);
      Serial.print(i);
      Serial.println();
      if (peripherals[i].connect()) {
        Serial.print(peripherals[i].localName());
        Serial.println(" Connected");
      } else {
        Serial.println("Failed to connect!");
      }
      // discover peripheral attributes
      Serial.println("Discovering attributes ...");
      while(!peripherals[i].discoverAttributes()) {
        Serial.println("Attribute discovery failed.");
        //peripherals[i].disconnect();
      }
  
      // loop through all services
      int numServices = peripherals[i].serviceCount();
      for (int k = 0; k < numServices; k++) {
        BLEService service =peripherals[i].service(k);
        Serial.print("services:");
        Serial.print(numServices);
        Serial.print(k);
        Serial.println();
        int caracCount = service.characteristicCount();
        // loop through all characteristics of the service
        for (int j = 0; j < caracCount; j++) {
          BLECharacteristic characteristic = service.characteristic(j);
          Serial.print("caracterisitca:");
          Serial.print(service.characteristicCount());
          Serial.print(service.characteristic(j));
          Serial.println();
          if (characteristic.canSubscribe()) {
            characteristicPointers.push_back(characteristic);
            Serial.print("Subscribing to characteristic ");
            Serial.println(characteristic.uuid());
            characteristic.subscribe();  
          }
        }
      }
    }
    while(characteristicPointers.size() >2 ){
      ReadData();
    }
  }
 void ReadData (){
  for (int j = 0; j < characteristicPointers.size(); j++) {
    BLECharacteristic characteristic2 = characteristicPointers[j];  
    // subscribe to the characteristic
    if (characteristic2.canRead()) {
      characteristic2.read();
      if (characteristic2.valueLength() > 0) {
        printData(characteristic2.value(), characteristic2.valueLength());
      }
    }
  }
}

void printData(const unsigned char data[], int length) {
  
  Serial.print("data: ");
  for (int i = 0; i < length; i++) {
    unsigned char b = data[i];
    Serial.write(b);
  }
  Serial.println("");
  
}

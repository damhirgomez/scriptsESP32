#include <BLEScan.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEAdvertisedDevice.h>

BLEScan* pBLEScan;
BLEAdvertisedDevice myDevice[50];
int deviceCount = 0;

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
  void onResult(BLEAdvertisedDevice advertisedDevice) {
    Serial.print("BLE Advertised Device found: ");
    Serial.println(advertisedDevice.getName().c_str());
    myDevice[deviceCount] = advertisedDevice;
    deviceCount++;
  }
};

void setup() {
  Serial.begin(115200);
  Serial.println("Scanning for BLE devices...");
  BLEDevice::init("");
  pBLEScan = BLEDevice::getScan();
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true);
  pBLEScan->start(5);
}

void loop() {
  while (true) {
    if (deviceCount > 0) {
      Serial.println("Select a device to connect to:");
      for (int i = 0; i < deviceCount; i++) {
        Serial.print(i);
        Serial.print(": ");
        Serial.println(myDevice[i].getName().c_str());
      }
      while (!Serial.available()) {
        delay(100);
      }
      int selection = Serial.parseInt();
      if (selection >= 0 && selection < deviceCount) {
        BLEAddress addr = myDevice[selection].getAddress();
        Serial.print("Connecting to ");
        Serial.println(myDevice[selection].getName().c_str());
        BLEClient* pClient = BLEDevice::createClient();
        pClient->connect(addr);
        Serial.println("Connected!");
        deviceCount = 0;
        pBLEScan->start(5); // Start scanning for BLE devices again
        Serial.println("Do you want to add another device? (y/n)");
        while (!Serial.available()) {
          delay(100);
        }
        char response = Serial.read();
        if (response == 'n' || response == 'N') {
          break; // Exit the loop if the user does not want to add another device
        }
      } else {
        Serial.println("Invalid selection.");
        deviceCount = 0;
        pBLEScan->start(5); // Start scanning for BLE devices again
      }
    }
  }
}

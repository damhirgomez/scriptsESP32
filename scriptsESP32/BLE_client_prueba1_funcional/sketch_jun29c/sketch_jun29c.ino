#include <BLEScan.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEAdvertisedDevice.h>

BLEAdvertisedDevice devices[50];
int deviceCount = 0;

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
  public:
    void onResult(BLEAdvertisedDevice advertisedDevice) {
      Serial.printf("%d: Device found: %s\n", deviceCount, advertisedDevice.toString().c_str());
      devices[deviceCount] = advertisedDevice;
      deviceCount++;
    }
};



void setup() {
  Serial.begin(115200);
  BLEDevice::init("");
  BLEScan* pBLEScan = BLEDevice::getScan();
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true);
  pBLEScan->start(30);
}

void loop() {
  // Wait for user input
  Serial.println("Enter device numbers to connect to (separated by commas):");
  while (!Serial.available()) {
    delay(100);
  }
  String input = Serial.readStringUntil('\n');
  Serial.println(input);
  // Parse input and connect to selected devices
  int selectedDevices[50];
  int selectedCount = 0;
  char* token = strtok((char*)input.c_str(), ",");
  while (token != NULL) {
    int deviceNumber = atoi(token);
    Serial.printf("device numer %d:",deviceNumber);
    Serial.println(deviceNumber);
    if (deviceNumber >= 0 && deviceNumber < deviceCount) {
      selectedDevices[selectedCount] = deviceNumber;
      selectedCount++;
      Serial.print(devices[deviceNumber].toString().c_str());
      Serial.print(", ");
    }
    token = strtok(NULL, ",");
  }
  
  // Connect to selected devices and read data
  for (int i = 0; i < selectedCount; i++) {
    BLEAdvertisedDevice device = devices[selectedDevices[i]];
    BLEClient* pClient = BLEDevice::createClient();
    BLEAddress address = device.getAddress();
    pClient->connect(address);
    if (pClient->isConnected()) {
      Serial.println("Connected to device");
    } else {
      Serial.println("Failed to connect to device");
    }
    BLERemoteService* pRemoteService = pClient->getService(device.getServiceUUID());
    if (pRemoteService == nullptr) {
      Serial.print("Failed to find our service UUID: ");
      Serial.println(device.getServiceUUID().toString().c_str());
      pClient->disconnect();
    }
    BLERemoteCharacteristic* pCharacteristic = pRemoteService->getCharacteristic("ABF0E002-B597-4BE0-B869-6054B7ED0CE3");
    if (pCharacteristic == nullptr) {
      Serial.print("Failed to find our characteristic UUID: ");
      Serial.println(pRemoteService->getUUID().toString().c_str());
      pClient->disconnect();
    }
    std::string value = pCharacteristic->readValue();
    Serial.printf("Data from device %d:\n", value);
    pClient->disconnect();
    delete pClient;
  }
}

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
    Serial.println(deviceNumber);
    if (deviceNumber >= 0 && deviceNumber <= deviceCount) {
      selectedDevices[selectedCount] = deviceNumber;
        Serial.print(devices[deviceNumber].toString().c_str());
      Serial.print(", ");
      selectedCount++;
      
    }
    token = strtok(NULL, ",");
  }
  
  // Connect to selected devices and read data
  for (int i = 0; i < selectedCount; i++) {
    Serial.println(selectedDevices[i]);
    Serial.println(selectedCount);
    BLEAdvertisedDevice device = devices[selectedDevices[i]];
  
    BLEClient* pClient = BLEDevice::createClient();
    BLEAddress address = device.getAddress();
    Serial.println(selectedDevices[i]);
    pClient->connect(address);
    Serial.println("Connected to device");
    BLERemoteService* pRemoteService = pClient->getService(device.getServiceUUID());
    BLERemoteCharacteristic* pCharacteristic = pRemoteService->getCharacteristic(pRemoteService->getUUID());
    std::string value = pCharacteristic->readValue();
    Serial.printf("Data from device %d: %s\n", selectedDevices[i]+1, value.c_str());
    pClient->disconnect();
    delete pClient;
  }
}

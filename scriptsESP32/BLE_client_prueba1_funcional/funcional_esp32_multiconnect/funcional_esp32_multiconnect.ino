#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>

// Vector para almacenar los dispositivos encontrados durante el escaneo
std::vector<BLEAdvertisedDevice> devices;

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
  void onResult(BLEAdvertisedDevice advertisedDevice) { 
    Serial.printf("Device found: %s\n", advertisedDevice.toString().c_str());
    if (advertisedDevice.getName().find("ProxSIMity") != std::string::npos) {
      // Agregar el dispositivo al vector de dispositivos encontrados
      devices.push_back(advertisedDevice);
    }
  }
};
void notifyCallback(BLERemoteCharacteristic* pRemoteCharacteristic, uint8_t* pData, size_t length, bool isNotify) {
  std::string value = std::string((char*)pData, length);
  Serial.print("Valor notificado: ");
  Serial.println(value.c_str());
}
void setup() {
  Serial.begin(115200);
  // Inicializar BLE
  BLEDevice::init("MyDevice");
  
  // Escanear BLE
  BLEScan* pBLEScan = BLEDevice::getScan();
  Serial.println("Scanning for BLE devices...");
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true);
  pBLEScan->start(30);
}

void loop() {
  // Iterar sobre los dispositivos encontrados durante el escaneo
  for (int i = 0; i < devices.size(); i++) {
    BLEAdvertisedDevice device = devices[i];
      BLEAddress address = device.getAddress();
      // Conectar al dispositivo
      BLEClient* pClient = BLEDevice::createClient();
      pClient->connect(address);
      Serial.print("Connecting to ");
      Serial.println( device.getName().c_str());
      // Descubrir los servicios del dispositivo
      std::map<std::string, BLERemoteService*>* pServices = pClient->getServices();
      for (auto const& service : *pServices) {
        // Obtener las características del servicio
        std::map<std::string, BLERemoteCharacteristic*>* pCharacteristics = service.second->getCharacteristics();
  
        // Convertir las características a un vector
        std::vector<BLERemoteCharacteristic*> characteristicPointers;
        for (auto const& characteristic : *pCharacteristics) {
          characteristicPointers.push_back(characteristic.second);
        }
  
        // Iterar sobre las características del servicio
        for (int j = 0; j < characteristicPointers.size(); j++) {
          BLERemoteCharacteristic* pCharacteristic = characteristicPointers[j];          
          // Leer los datos de la característica
          std::string value = pCharacteristic->readValue();
  
          // Imprimir los datos leídos
          Serial.print("Dispositivo ");
          Serial.print(i);
          Serial.print(", Servicio ");
          Serial.print(service.first.c_str());
          Serial.print(", Característica ");
          Serial.print(j);
          Serial.print(": ");
          Serial.println(value.c_str());
          if(pCharacteristic->canNotify()){
            pCharacteristic->registerForNotify(notifyCallback);
          }
        }
      }
    // Liberar recursos
    //pClient->disconnect();
    //delete pClient;
  }

  // Limpiar el vector de dispositivos encontrados
  devices.clear();

  // Esperar antes de volver a escanear
  delay(1000);
}

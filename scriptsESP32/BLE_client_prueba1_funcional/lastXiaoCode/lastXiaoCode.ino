
// based on https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/examples/HighLevelExample/HighLevelExample.ino
// BLE   UUID: https://btprodspecificationrefs.blob.core.windows.net/assigned-values/16-bit%20UUID%20Numbers%20Document.pdf
#include <Arduino.h>
#include <ArduinoBLE.h>
#include <LSM6DS3.h>
#include "Wire.h"

// Create a instance of class LSM6DS3
LSM6DS3 my_imu(I2C_MODE, 0x6A); // I2C device address 0x6A
//push button variable
// const int pushButton = 1;// asignar el pin al que se conecta el boton
int buttonState = 1;         // variable for reading the pushbutton status
//pines sensor presion
int fsrPin = 3;     // the FSR and 10K pulldown are connected to a0
int fsrPin2 = 4;
int fsrPin3 = 5;
int fsrReading;     // the analog reading from the FSR resistor divider
int fsrReading2;
int fsrReading3;
String nombreBle;

// BLE Variables
// Physical Activity Monitor: 0x183E
BLEService imu_service("ABF0E000-B597-4BE0-B869-6054B7ED0CE1");
// ACC
// acceleration unit = m/s**2
BLEByteCharacteristic acc_unit_characteristic("2713", BLERead);
// xiaoble is 32bit chip: 64bit,4 byte

BLEStringCharacteristic acc_x_characteristic("ABF0E001-B597-4BE0-B869-6054B7ED0CE1", BLERead | BLENotify,60);
BLEDescriptor acc_x_descriptor("2901", "accX");



void setup()
{
  // put your setup code here, to run once:
  Serial.begin(9600);
  //pinMode(pushButton, INPUT_PULLUP);
  // build-in leds
  pinMode(LEDR, OUTPUT);
  pinMode(LEDG, OUTPUT);
  pinMode(LEDB, OUTPUT);
  // turn off the all build-in leds
  digitalWrite(LEDR, HIGH);
  digitalWrite(LEDG, HIGH);
  digitalWrite(LEDB, HIGH);



  // Call .begin() to configure the IMUs
  if (my_imu.begin() != 0)
  {
    Serial.println("Device error");
    digitalWrite(LEDR, LOW);
  }
  else
  {
    Serial.println("Device OK!");
  }

  // ble
  // begin initialization
  if (!BLE.begin())
  {
    Serial.println("starting BLE failed!");

    while (1)
    {
      digitalWrite(LEDR, !digitalRead(LEDR));
      delay(500);
    };
  }
  nombreBle ="PG001";
  BLE.setDeviceName("ProxSIMityGlove001");
  BLE.setLocalName("ProxSIMityGlove001");
  BLE.setAdvertisedService(imu_service);


  // acc
  imu_service.addCharacteristic(acc_x_characteristic);

  BLE.addService(imu_service);


  acc_x_characteristic.writeValue("");


  BLE.advertise();
}

int incomingByte;

void loop()
{
  //buttonState = digitalRead(pushButton);
  //if(buttonState == 0){
    
        BLEDevice central = BLE.central();
  
    if (central)
    {
      while (central.connected())
      {
       if (Serial.available() > 0) {
            incomingByte = Serial.read();
            if (incomingByte == '1') {
            buttonState =1;
          }

        }  
        digitalWrite(LEDR,HIGH);
       
        //buttonState = digitalRead(pushButton);
        Serial.println(buttonState);
//        if (buttonState == 1)
//        {
//          buttonState = 0;
//          digitalWrite(LEDB,HIGH);
//          digitalWrite(LEDG, !digitalRead(LEDG)); // ble heartbeat
//        }
        //else
        //{
         // buttonState = 1;
          //digitalWrite(LEDG,HIGH);
          //digitalWrite(LEDB, !digitalRead(LEDB)); // ble heartbeat
        //}

        
        fsrReading = analogRead(fsrPin);
        fsrReading2 = analogRead(fsrPin2);
        fsrReading3 = analogRead(fsrPin3);

        float x = my_imu.readFloatAccelX();
        float y = my_imu.readFloatAccelY();
        float z = my_imu.readFloatAccelZ();
        float gx = my_imu.readFloatGyroX();
        float gy = my_imu.readFloatGyroY();
        float gz = my_imu.readFloatGyroZ();
        //int presion1 = digitalRead(PresSensor1);
        //int presion2 = digitalRead(PresSensor2);
        //int presion3 = digitalRead(PresSensor3);
        String ValueFinal = ""; 
        String Coma = ",";
        ValueFinal.concat(x);
        ValueFinal.concat(Coma);
        ValueFinal.concat(y);
        ValueFinal.concat(Coma);
        ValueFinal.concat(z);
        ValueFinal.concat(Coma);
        ValueFinal.concat(gx);
        ValueFinal.concat(Coma);
        ValueFinal.concat(gy);
        ValueFinal.concat(Coma);
        ValueFinal.concat(gz);
        ValueFinal.concat(Coma);
        ValueFinal.concat(fsrReading);
        ValueFinal.concat(Coma);
        ValueFinal.concat(fsrReading2);
        ValueFinal.concat(Coma);
        ValueFinal.concat(fsrReading3);
        ValueFinal.concat(Coma);
        ValueFinal.concat(buttonState);
        ValueFinal.concat(Coma);
        ValueFinal.concat("1");
        ValueFinal.concat(Coma);
        ValueFinal.concat("2");
        ValueFinal.concat(Coma);
        ValueFinal.concat("3");
        ValueFinal.concat(Coma);
        ValueFinal.concat(nombreBle);
        
        
        //const unsigned char dataValue = stringToBytes(ValueFinal);
        acc_x_characteristic.writeValue(ValueFinal);

        //Serial.println("String");
        Serial.println(ValueFinal);

        buttonState =0;
        delay(50);
        
        
        
      }
    }
  //}
  Serial.println("Waiting BLE connection");
  digitalWrite(LEDR, !digitalRead(LEDR)); // waiting ble connection
  digitalWrite(LEDB,HIGH);
  digitalWrite(LEDG,HIGH);
  delay(200);
}

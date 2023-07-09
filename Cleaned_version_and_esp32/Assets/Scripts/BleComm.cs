using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.EventSystems;


//NOTE: Upon building for windows standalone, make sure architecture is x86_64 (64 bits) for BLE functionality to work
public class BleComm : MonoBehaviour
{
    public Text MessageText;

    // Change this to match your device.
    public string targetDeviceName = "ProxSIMityGlove";
    public string serviceUuid = "{ABF0E000-B597-4BE0-B869-6054B7ED0CE3}";
    public string[] characteristicUuids = {
         "{ABF0E002-B597-4BE0-B869-6054B7ED0CE3}"
    };

    
    public float[] sensors;

    public string datos, record, connectionOn, identifier;// damhir
    public string[] valores = new string[9];
    public char delimitador;
    BLE ble;
    BLE.BLEScan scan;
    public bool isScanning = false, _connected = false;
    string deviceId = null;
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();
    int devicesCount = 0;
    private bool readData = false;
    // BLE Threads 
    public Thread scanningThread, connectionThread, readingThread;

    // GUI elements 

    private bool failedconnection;


    void Start()
    {
        Application.targetFrameRate = 60;
        ble = new BLE();
        readingThread = new Thread(ReadBleData);
        readingThread.Start();        
        failedconnection = false;


    }

    void Update()
    {
        //Scan BLE devices 
        if (isScanning)
        {
            
            if (discoveredDevices.Count > devicesCount)
            {
                foreach (KeyValuePair<string, string> entry in discoveredDevices)
                {
                    Debug.Log("Added device: " + entry.Key);
                }
                devicesCount = discoveredDevices.Count;
            }
        }


        // The target device was found.
        if (deviceId != null && deviceId != "-1")
        {
            // Target device is connected and GUI knows. 
            if (BLE.isConnected && _connected)
            {
                if (!readingThread.IsAlive)
                { 
                    
                    readingThread = new Thread(ReadBleData);
                    readingThread.Start();


                } 


            }
            // Target device is connected, but GUI hasn't updated yet.
            else if (BLE.isConnected && !_connected)
            {
                _connected = true;
                Debug.Log("Connected to target device!\n");
            }
            else if (!_connected)
            {
                Debug.Log("Found target device but not connected.\n");
            }
        }
        if(failedconnection)
        {

            failedconnection = false;

        }

    }


    /* Functions to handle BLE */

    public void setScan()
    {
        StartScanHandler("");
    }

    //Start BLE Scan
    public void StartScanHandler(string identifier)
    {
        targetDeviceName = targetDeviceName + identifier;
        devicesCount = 0;
        _connected = false;
        isScanning = true;
        discoveredDevices.Clear();
        deviceId = null;
        Debug.Log("Scanning...");
        scanningThread = new Thread(ScanBleDevices);
        scanningThread.Start();

    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    // Scan BLE devices
    private void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        scan.Found = (_deviceId, deviceName) =>
        {

            discoveredDevices.Add(_deviceId, deviceName);

            if(BLE.errormsg != null)
            {
                //screentext = BLE.errormsg;
                failedconnection = true;
            }

            //if found the target device, immediately stop scan and attempt to connect
            //if (deviceId == null && deviceName.Contains(MTPL_PCB.targetDevice()))
            if (deviceId == null && deviceName == targetDeviceName)
            {
                Debug.Log("Found device!");
                //screentext = "Found device!";
                deviceId = _deviceId;
                StartConHandler();
            }
        };

        scan.Finished = () =>
        {
            isScanning = false;
            //screentext = "scan finished";
            Debug.Log("Scan finished!");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
            Thread.Sleep(500);
        scan.Cancel();
        scanningThread = null;
        isScanning = false;

        if (deviceId == "-1")
        {

            //screentext = "No device found! \nPlease close the app and start again";
            Debug.Log("no device found!");
            return;
        }
    }

    // Connect BLE device
    private void ConnectBleDevice()
    {
        if (deviceId != null)
        {
            try
            {
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids);
            }
            catch (Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
            }
        }
        if (BLE.isConnected)
            Debug.Log("Connected to Device! " );
            //screentext = "Connected to Device! " ;
    }

    // Read BLE Data
    public void ReadBleData(object obj)// get data from ble devices

    {


        byte[] bytes;
        bytes = BLE.ReadBytes();

        datos = Encoding.UTF8.GetString(bytes);
        delimitador = ',';
        valores = datos.Split(delimitador);
        
        if (valores.Length > 9)
        {
            record = valores[9];
        }
        
    }



    // Reset BLE handler
    public void ResetHandler()
    {
        // Reset previous discovered devices
        isScanning = false;
        discoveredDevices.Clear();
        deviceId = null;
        CleanUp();
        Application.Quit();

    }
    


    // Handle GameObject destroy
    private void OnDestroy()
    {
        StopAllCoroutines();
        ResetHandler();
        Application.Quit();

    }

    // Handle Quit Game
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        ResetHandler();
        Application.Quit();

    }

    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    public void CleanUp()
    {

        try
        {
            BLE.Close();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("ble never initialized.\n" + e);
        }

        try
        {
            scanningThread.Abort();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Scan thread never initialized.\n" + e);
        }

        try
        {
            connectionThread.Abort();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Connection thread never initialized.\n" + e);
        }

        try
        {
            readingThread.Abort();
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Reading thread never initialized.\n" + e);
        }
    }


}
using System;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Win32;


public class SerialPortManager
{
    private SerialPort serialPort;
    public void OpenSerialPort()
    {
        string portName= GetPortNames();
        Debug.Log(portName +" puerto");
        serialPort = new SerialPort(portName, 115200); // Ajusta el baudRate según tu configuración
        try
        {
            serialPort.Open();
            Debug.Log("Puerto serie abierto en: " + portName);
            // Resto de tu código para leer y procesar los datos
        }
        catch (IOException)
        {
            Debug.Log("No se pudo abrir el puerto serie en: " + portName);
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DataReceived += OnDataReceived;
        }
    }

    private string GetPortNames()
    {
        string portnameMAC = "";
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            Debug.Log("test2 ");
            // Busca automáticamente el puerto serie en el que está conectado el Arduino en una Mac
            string[] ports = SerialPort.GetPortNames();
            foreach (string p in ports)
            {
                if (p.Contains("usb"))
                {
                    portnameMAC = p;

                    Debug.Log("I read it  " + portnameMAC);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            // Establece el nombre del puerto serie en el que está conectado el Arduino en Windows
            // List<string> names = ComPortNames("Arduino");


            String pattern = String.Format("Silicon");
            Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            List<string> comports = new List<string>();
            string portname = " ";
            string portnumber = " ";
            RegistryKey rk1 = Registry.LocalMachine;
            RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\USB");
            foreach (String s3 in rk2.GetSubKeyNames())
            {
                RegistryKey rk3 = rk2.OpenSubKey(s3);
                foreach (String s in rk3.GetSubKeyNames())
                {
                    RegistryKey rk4 = rk3.OpenSubKey(s);
                    portname = (string)rk4.GetValue("FriendlyName");
                    if (portname != null)
                    {
                        if (_rx.Match(portname).Success)
                        {
                            RegistryKey rk7 = rk4.OpenSubKey("Device Parameters");
                            portnumber = (string)rk7.GetValue("PortName");
                            Debug.Log(portnumber + "numero puerto");
                            comports.Add(portnumber);
                        }
                    }
                }
            }

            List<string> names = comports;
            Debug.Log("comport"+comports);

            if (names.Count > 0)
            {
                foreach (String s in SerialPort.GetPortNames())
                {
                    if (names.Contains(s)) portnameMAC = s;
                    Debug.Log("My Arduino port is " + s);
                        


                }
            }
            else
                Debug.Log("No COM ports found");
        }
        Debug.Log(portnameMAC+"puerto en la funcion get");
        return portnameMAC;
    }
    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = serialPort.ReadLine();
        // Haz algo con los datos recibidos, como procesarlos o mostrarlos en la consola
        Debug.Log("Datos recibidos: " + data);
    }
    public void CloseSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.DataReceived -= OnDataReceived; // Desvincula el evento DataReceived
            serialPort.Close(); // Cierra el puerto serie
            Debug.Log("Puerto serie cerrado");
        }
    }

}
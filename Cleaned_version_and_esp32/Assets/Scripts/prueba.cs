using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prueba : MonoBehaviour
{
    private SerialPortManager serialPortManager;

    private void Start()
    {
        serialPortManager = new SerialPortManager();
        serialPortManager.OpenSerialPort();
    }

    private void Update()
    {
        // Aqu� puedes realizar otras operaciones o l�gica de tu juego
    }

    private void OnDestroy()
    {
        if (serialPortManager != null)
        {
            serialPortManager.CloseSerialPort();
        }
    }
}


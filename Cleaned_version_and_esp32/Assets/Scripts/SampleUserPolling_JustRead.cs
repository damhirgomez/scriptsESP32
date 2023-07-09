/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/**
 * Sample for reading using polling by yourself. In case you are fond of that.
 */
public class SampleUserPolling_JustRead : MonoBehaviour
{
    public SerialController serialController;

    public Text TextSerial1, TextSerial2, TextSerial3, TextSerial4, TextSerial5, TextSerial6, TextSerial7; 
    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
	}

    // Executed each frame
    void Update()
    {
        string message = serialController.ReadSerialMessage();

        if (message == null)
            return;

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
            Debug.Log("Connection established");
        else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
            Debug.Log("Connection attempt failed or disconnection detected");
        else
            Debug.Log("Message arrived: " + message);
        
        if (message.Contains("data:"))
        {
            switch (message)
            {
                case string messageText when message.Contains("PG001"):
                    TextSerial1.text = "Data from PG001: " + message;
                    break;
                case string messageText when message.Contains("PG002"):
                    TextSerial2.text = "Data from PG002: " + message;
                    break;
                case string messageText when message.Contains("PG003"):
                    TextSerial3.text = "Data from PG003: " + message;
                    break;
                default:

                    break;
            }
        }else
        {
            TextSerial1.text = "";
            TextSerial2.text = "";
            TextSerial3.text = "";
            TextSerial4.text = "";
            TextSerial5.text = "";
            TextSerial6.text = "";
            TextSerial7.text = "";
        }

    }
}

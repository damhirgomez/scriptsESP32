using System;
using Object = System.Object;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Glove
{
    public class GloveHandler : MonoBehaviour
    {


        public BleComm BleInput;
        public string pathFile;
        public List<string> dataList, options;
        public List<string[]> lines;
        private bool isRecording, initialsetup;
        private int filecount = 0;
        private Boolean triggerRecordingDAMHIR = false; // DAmhir
        private Boolean ceroEncontrado = false;// Damhir
        private string[] valores;
        private string identifier, filename; //damhir
        public Button sendIdentifier, Savedata, DeleteData;
        public InputField bleIdentifier;
        private string lastValue = "";
        public bool paused = false; 
        public Text MessageText;
        public Text informationText;



        void Start()
        {

            //handler to save data
            initialsetup = true; 
            isRecording = false;
            dataList = new List<string>();
            options = new List<string>();
            lines = new List<string[]>();
            Savedata.gameObject.SetActive(false);
            informationText.gameObject.SetActive(false);


        }

        void Update()
        {
            
            if (BleInput._connected && initialsetup)
            {
                initialsetup = false;
            }

            if (BleInput.record == "0" && !ceroEncontrado) //function to know the button of wristband has been pushed
            {
                informationText.gameObject.SetActive(true);
                StartCoroutine(WaitForFunction());
                triggerRecordingDAMHIR = !triggerRecordingDAMHIR;
                if (triggerRecordingDAMHIR)
                {
                    StartCannulate_Click();
                    informationText.text = "Recording data from ble...\n to save data press the wristband button ";
                }
                else
                {
                    
                    StopCannulate_Click();
                    informationText.text = "Press the wristband button to start recording data";
                }
                
                ceroEncontrado = true;


            }
            else if (BleInput.record == "1") 
            {
                ceroEncontrado = false;
            }

            if (isRecording) // Recording data to csv file 
            {
                MessageText.text = "";
                if (BleInput.valores != valores)
                {
                    //call the funcion to save data in a datalist to put later in a csv file
                    string dateNow = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                    saveData(BleInput.valores[0], BleInput.valores[1], BleInput.valores[2], BleInput.valores[3], BleInput.valores[4], BleInput.valores[5], BleInput.valores[6], BleInput.valores[7], BleInput.valores[8], dateNow);
                    MessageText.text = "data from ble" + ", "+ BleInput.valores[0] + ", " + BleInput.valores[1] + ", " + BleInput.valores[2] + ", " + BleInput.valores[3] + ", " + BleInput.valores[4] + ", " + BleInput.valores[5] + ", " + BleInput.valores[6]
                    + ", " + BleInput.valores[7] + ", " + BleInput.valores[8];
                }

            }

            
        }


        private void OnDestroy()
        {
            StopAllCoroutines();
            BleInput.ResetHandler();

            Application.Quit();
        }

        private void OnApplicationQuit()
        {

            StopAllCoroutines();
            BleInput.ResetHandler();
            Application.Quit();
        }


        public void bleNumber()//Start Scan ble devices
        {

            identifier = bleIdentifier.text;
            ButtonConnect_Click();


        }


        IEnumerator WaitForFunction()// wait function to prevent errors
        {
            yield return new WaitForSeconds(5);
        }


        //public void OnCannulaConnect(string arg) // Just need a wrapper
        //{

        //    sendIdentifier.gameObject.SetActive(true);//Damhir
        //    bleIdentifier.gameObject.SetActive(true);//Damhir


        //}


        public void ButtonConnect_Click()   //Start connection to ble devices
        {
            sendIdentifier.gameObject.SetActive(false);//Damhir
            bleIdentifier.gameObject.SetActive(false);//Damhir
            
            if (BleInput._connected)
            {
                Debug.Log("Paired successfully!");
                MessageText.text = "Paired successfully!";

            }
            else
                StartCoroutine(BLEscan());//call the scan function from bleComm to scan devices with the number in the input
        } 

        private IEnumerator BLEscan()//function from bleComm to scan devices with the number in the input
        {

            Debug.Log("Attempting Connection");
            //BLE.Cancel();
            //BLE.Close();

            BleInput.StartScanHandler(identifier);
            

            yield return new WaitForSeconds(10f);

            if (BleInput._connected)
            {
                
                Debug.Log("Paired successfully!");
                //BleInput.ButtonReconnect.gameObject.SetActive(false);

            }
            else
            {
                MessageText.text = "Cannot find any glove!, restart the application ";
                Debug.Log("Cannot find any glove!");
                //BleInput.ButtonReconnect.gameObject.SetActive(true); 

            }
        }


       

        public void saveData(string ACX, string ACY, string ACZ, string GX, string GY, string GZ, string PRESS1, string PRESS2, string PRESS3,string Datenow)// function to save data in data list to save later in a csv file
        {   
            //float currTime = recordTimer;
            //string timeCurr = currTime.ToString();
            //timeCurr = timeCurr.Replace(",", "");
            string dataString = ACX.ToString() +
                    "," + ACY.ToString() +
                    "," + ACZ.ToString() +
                    "," + GX.ToString() +
                    "," + GY.ToString() +
                    "," + GZ.ToString() +
                    "," + PRESS1.ToString() +
                    "," + PRESS2.ToString() +
                    "," + PRESS3.ToString() +
                    "," + Datenow +
                    "," + "" +
                    "," + "";

            dataList.Add(dataString);
        }

        private  void recordData(List<string> datalist, string method, string material)//function to save data in a csv file
        {
            string pattern = "[^a-zA-Z0-9_ ]";
            Regex rgx = new Regex(pattern);
            method = rgx.Replace(method, "_");
            method = method.Replace(" ", "_");
            material = rgx.Replace(material, "_");
            material = material.Replace(" ", "_");
            string dateNow2 = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            string dateNow2WithDashes = dateNow2.Replace('/', '_');
            dateNow2WithDashes = dateNow2WithDashes.Replace(':', '_');
            dateNow2WithDashes = dateNow2WithDashes.Replace(' ', '_');
            int randomInt = UnityEngine.Random.Range(0, 100);
            string randomIntAsString = randomInt.ToString();
            pathFile = Application.dataPath + "/" +  method + "_" + material + "_savedData_" + filecount + "_" + dateNow2WithDashes + "_"+ randomIntAsString + ".csv"; //directory creation for data entries
            while (File.Exists(pathFile))
            {
                filecount++;
                pathFile = Application.dataPath + "/" + method + "_" + material + "_savedData_" + filecount + "_" + dateNow2WithDashes + "_" + randomIntAsString + ".csv"; //directory creation for data entries
                
            }

            filename = method + "_" + material + "_savedData_" + filecount + "_" + dateNow2WithDashes + "_" + randomIntAsString + ".csv";
            filename = filename.Replace(" ", "_");
            
            string CouponName = material;
            string cannulation_method = method;
            StreamWriter sw = new StreamWriter(pathFile, true); 
            //sw.WriteLine();
            sw.WriteLine("ACX" + ","
                + "ACY" + ","
                + "ACZ" + ","
                + "GX" + ","
                + "GY" + ","
                + "GZ" + ","
                + "PRESS1" + ","
                + "PRESS2" + ","
                + "PRESS3" + ","
                + "Time" + ","
                + CouponName + ","
                + cannulation_method
                ); ;
            foreach (string s in datalist)
            {
                sw.WriteLine(s);
            }
            sw.Close();
        }

        public void StartCannulate_Click()//Start recording data 
        { 
            isRecording = true;
            paused = false;

            //StartCannulate.gameObject.SetActive(false);
            //StopCannulate.gameObject.SetActive(true);
            //UpdateStateAndFire();
            //StartCoroutine(CUpdate());

        }


        public void StopCannulate_Click()//stop recording data
        {
            //startcannulate = false;
            isRecording = false;
            paused = true;
            Savedata.gameObject.SetActive(true);
            DeleteData.gameObject.SetActive(true);
        }    

        public void Submit_Click()// call the funcion to save data in a csv file
        {
            recordData(dataList, "data", "test");
            dataList = new List<string>();
            Savedata.gameObject.SetActive(false);
            DeleteData.gameObject.SetActive(false);



        }

        public void Delete_Click()// Delete data saved in a data list
        {

            dataList = new List<string>();
            Savedata.gameObject.SetActive(false);
            DeleteData.gameObject.SetActive(false);
        }

    }   
}

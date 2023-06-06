using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
public class SerialWithArduino : MonoBehaviour
{
    SerialPort _SerialPort;
    // Start is called before the first frame update
    void Start()
    {
        string serialPortName = null;

//#if UNITY_ANDROID
//        foreach (var portName in SerialPort.GetPortNames())
//        {
//            Debug.Log($"portName: {portName}");
//            if (portName.Contains("ttyACM"))
//            {
//                serialPortName = portName;
//                break;
//            }
//        }
//        HDOBD2MainUI.DebugLabel($"find port:{serialPortName}");

//#elif UNITY_EDITOR
//            serialPortName = "COM3";

//#else
//            serialPortName = "COM3";
//#endif

        foreach (var portName in SerialPort.GetPortNames())
        {
            Debug.Log($"portName: {portName}");
#if UNITY_EDITOR
            if (portName.Contains("COM3"))
#else
            if (portName.Contains("COM3"))
#endif
            {
                serialPortName = portName;
                break;
            }
        }
        HDOBD2MainUI.PrintlnDebugLabel($"find port:{serialPortName}");

        //_SerialPort = new SerialPort(SerialPort.GetPortNames().Last(), 9600);
        if (serialPortName != null)
        {
            //var su = Runtime.getRuntime().exec("/system/bin/su");
#if UNITY_ANDROID
            HDOBD2MainUI.DebugLabel($"Android permission request");
            var permissionList = new List<string>() { Permission.ExternalStorageWrite, Permission.ExternalStorageRead, Permission.FineLocation };
            foreach(string permission in permissionList)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermission(permission);
                }
            }
#endif
            _SerialPort = new SerialPort($"{serialPortName}", 115200);
            HDOBD2MainUI.PrintlnDebugLabel($"selectedport:{_SerialPort.PortName}");
            try
            {
                //_SerialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
                //_SerialPort.Handshake = Handshake.None;
                //_SerialPort.ReadTimeout = 5000;    // 0.5 sec
                //_SerialPort.WriteTimeout = 5000;
                //_SerialPort.DtrEnable = true;
                //_SerialPort.RtsEnable = true;
                _SerialPort.Open();
                HDOBD2MainUI.PrintlnDebugLabel($"port open");

            }
            catch(System.Exception e)
            {
                HDOBD2MainUI.PrintlnDebugLabel($"exeption while open port:{e}");
            }

        }
        else
        {
            foreach (var portName in SerialPort.GetPortNames())
            {
                HDOBD2MainUI.PrintlnDebugLabel($"portName: {portName}");
            }
        }

        System.Threading.Thread serialCommunicationThread = new System.Threading.Thread(SerialCommunicationThread);
        serialCommunicationThread.Start();
    }
    int idx = 0;
    void SerialCommunicationThread()
    {
        while (true)
        {
            HDOBD2MainUI.UpdatePIDStatus("test", $"{idx++}");
            if (_SerialPort != null)
            {
                var res = _SerialPort.ReadLine();
                if (res.Length >= 8)
                {

                    string header = res[0..8];
                    if (header == "OBD2____")
                    {
                        var resSplit = res.Split(",");
                        string category = resSplit[1];
                        string code = resSplit[2];
                        string value = resSplit[3];
                        switch (category)
                        {
                            case "STATUS":
                                {
                                    HDOBD2MainUI.UpdatePIDStatus(code, value);
                                    //switch (pidName)
                                    //{
                                    //    case "RPM":
                                    //        {

                                    //            break;
                                    //        }
                                    //    case "COOLANT_TEMP":
                                    //        {

                                    //            break;
                                    //        }
                                    //}
                                    break;
                                }
                            case "DTC":
                                {
                                    HDOBD2MainUI.UpdateDTC(code);

                                    break;
                                }
                        }



                    }
                    else if (header == "packet__")
                    {

                    }
                    else if (header == "order___")
                    {
                        var resSplit = res.Split(",");
                        float angle = float.Parse(resSplit[1]);
                        transform.rotation = Quaternion.Euler(0, angle * Time.deltaTime, 0) * transform.rotation;
                    }
                    else
                    {
                        HDOBD2MainUI.PrintlnDebugLabel($"{_SerialPort.PortName}: {res}");
                    }
                }
            }
            else
            {
                //foreach (var portName in SerialPort.GetPortNames())
                //{
                //    HDOBD2MainUI.DebugLabel($"portName: {portName}");
                //}
            }
            System.Threading.Thread.Sleep(100);
        }


    }
}

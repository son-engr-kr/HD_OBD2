using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class SerialTest : MonoBehaviour
{

    SerialPort _SerialPort;
    void Start()
    {
        foreach(var port in SerialPort.GetPortNames())
        {
            Debug.Log($"portName: {port}");
        }
        _SerialPort = new SerialPort("COM3", 9600);
        Debug.Log($"{_SerialPort}");
        //_SerialPort.Parity = Parity.None;
        //_SerialPort.StopBits = StopBits.One;
        //_SerialPort.Handshake = Handshake.None;
        //_SerialPort.DtrEnable = true;
        //_SerialPort.RtsEnable = true;
        //_SerialPort.ReadTimeout = 5000;
        //_SerialPort.WriteTimeout = 500;
        //_SerialPort.Close();
        Debug.Log($"{_SerialPort.PortName}");
        _SerialPort.Open();
        //_SerialPort.DiscardInBuffer();
        //InvokeRepeating
        //if (!_SerialPort.IsOpen)
        //{
        //    _SerialPort.Open();

        //}
        //var awd = new System.IO.Port
    }

    // Update is called once per frame
    void Update()
    {
        
        var res = _SerialPort.ReadLine();
        //Debug.Log($"{res}");
        try
        {
            var awd = float.Parse(res);
            transform.rotation = Quaternion.Euler(0, awd, 0) * transform.rotation;
        }
        catch (System.Exception e)
        {

        }
    }
}

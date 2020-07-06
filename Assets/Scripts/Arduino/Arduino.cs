using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class Arduino : MonoBehaviour
{
    public string portName;
    public int baudRate;

    SerialPort stream;

    public static Arduino self;

    void Awake()
    {
        if (self != null)
        {
            Destroy(this);
        } else
        {
            self = this;
        }

        stream = new SerialPort(portName, baudRate);
        stream.Open();
    }

    public void Update()
    {
        //WriteToArduino("2");
    }

    public void WriteToArduino(string message)
    {
        stream.Write(message);
        //stream.BaseStream.Flush();
    }

    public void WriteToArduino(int message)
    {
        stream.Write(message.ToString());
    }

    string ReadFromArduino()
    {
        string returnVal = stream.ReadLine();
        stream.BaseStream.Flush();
        return returnVal;
    }

    private void OnApplicationQuit()
    {
        stream.Close();
    }
}

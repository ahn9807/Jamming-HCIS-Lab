using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using UnityEditor.VersionControl;

public class Arduino : MonoBehaviour
{
    public string portName;
    public int baudRate;
    public int timeoutMilliseconds;

    SerialPort stream;

    public static Arduino self;

    public string lastMessage;

    bool messageQueueEmpty = true;

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
        stream.WriteTimeout = timeoutMilliseconds;
        stream.ReadTimeout = timeoutMilliseconds;
        stream.Open();
    }

    private void Start()
    {
        //StartCoroutine(ReadFromArduino("r", (stringa)=>{ Debug.Log(stringa); }, () => { Debug.Log("failed"); }, 100f));
    }

    public void WriteToArduino(string message)
    {
        if(checkLastMessage(message))
        {
            stream.Write(message);
            //stream.BaseStream.Flush();
        }
    }

    public void WriteToArduino(int message)
    {
        if(checkLastMessage(message.ToString()))
        {
            stream.Write(message.ToString());
        }
    }

    public IEnumerator ReadFromArduino(string syncCode, Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        WriteToArduino(syncCode);

        do
        {
            try
            {
                dataString = stream.ReadLine();
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield break;
            }
            else
            {
                yield return null;
            }

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;
        } while (diff.Milliseconds < timeout);

        fail?.Invoke();

        yield return null;
    }

    private void OnApplicationQuit()
    {
        stream.Close();
    }

    private bool checkLastMessage(string currentMessage)
    {
        if(currentMessage == lastMessage)
        {
            return false;
        } else
        {
            lastMessage = currentMessage;
            return true;
        }
    }
}

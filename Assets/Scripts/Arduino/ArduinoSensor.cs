using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ArduinoSensor : MonoBehaviour
{
    public float sensoreReadInterval;
    public float timeoutInterval = 100f;
    float sensorValue;

    void Start()
    {
        
    }

    public void SetPressure(float pressure)
    {
        Arduino.self.WriteToArduino("r" + Mathf.RoundToInt(pressure) + " ");
    }

    void ReadSensorValue(string val)
    {
        sensorValue = float.TryParse(val, out float num) ? num : -1;
    }

    public void UpdateSensorValue()
    {
        StartCoroutine(Arduino.self.ReadFromArduino("r", ReadSensorValue, () => { Debug.Log("failed to read"); }, timeoutInterval));
    }

    public float GetSensorValue()
    {
        return sensorValue;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoSensor : MonoBehaviour
{
    public float timeoutInterval = 100f;
    float sensorValue;
    bool updateSensorValue;

    void Start()
    {
        StartCoroutine(Arduino.self.ReadFromArduino("r", ReadSensorValue, () => { Debug.Log("failed to read"); }, timeoutInterval));
    }

    private void Update()
    {
        if(updateSensorValue)
        {
            StartCoroutine(Arduino.self.ReadFromArduino("r", ReadSensorValue, () => { Debug.Log("failed to read"); }, timeoutInterval));
            updateSensorValue = false;
        }
    }

    void ReadSensorValue(string val)
    {
        sensorValue = float.Parse(val);
    }

    public void UpdateSensorValue()
    {
        updateSensorValue = true;
    }

    public float GetSensorValue()
    {
        updateSensorValue = true;
        return sensorValue;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoInteraction : MonoBehaviour
{
    float stiffness;

    private void Start()
    {
        //ArduinoPump.ActivatePressurePump();
        //ArduinoPump.ActivatePressureSol();
        //ArduinoPump.ActivateVaccumPump();
        //ArduinoPump.ActivateVaccumSol();
    }

    private void Update()
    {
        //Debug.Log(sensor.GetSensorValue());
    }   
         
    public void SetStiffness(float stiffness)
    {
        this.stiffness = stiffness;
        if(stiffness != 0)
        {
            ArduinoPump.ActivateVaccumPump();
            ArduinoPump.ActivateVaccumSol();
        } else
        {
            ArduinoPump.ReleaseVaccumPump();
            ArduinoPump.ReleaseVaccumSol();
        }
    }
}

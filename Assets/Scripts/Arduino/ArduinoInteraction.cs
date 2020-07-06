using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoInteraction : MonoBehaviour
{
    float stiffness;

    Arduino arduino;

    // Start is called before the first frame update
    void Start()
    {
        arduino = Arduino.self; 
    }

    // Update is called once per frame
    void Update()
    {
        
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

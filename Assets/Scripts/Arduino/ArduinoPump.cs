using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class ArduinoPump {
    static bool PressurePumpOn;
    static bool VaccumPumpOn;
    static bool PressureSolOn;
    static bool VaccumSolOn;

    public static void ActivatePressurePump()
    {
        if(!PressurePumpOn)
        {
            PressurePumpOn = true;
            Arduino.self.WriteToArduino("2");
        }

    }

    public static void ReleasePressurePump()
    {
        if(PressurePumpOn)
        {
            Arduino.self.WriteToArduino("6");
            PressurePumpOn = false;
        }

    }

    public static void ActivateVaccumPump()
    {
        if(!VaccumPumpOn)
        {
            Arduino.self.WriteToArduino("1");
            VaccumPumpOn = true;
        }

    }

    public static void ReleaseVaccumPump()
    {
        if(VaccumPumpOn)
        {
            Arduino.self.WriteToArduino("5");
            VaccumPumpOn = false;
        }

    }

    public static void ActivateWaterSuck()
    {
        Arduino.self.WriteToArduino("3");
    }

    public static void ActivateWaterPull()
    {
        Arduino.self.WriteToArduino("4");
    }

    public static void ReleaseWater()
    {
        Arduino.self.WriteToArduino("7");
    }

    public static void ActivateVaccumSol()
    {
        if(!VaccumSolOn)
        {
            Arduino.self.WriteToArduino("8");
            VaccumSolOn = true;
        }

    }

    public static void ReleaseVaccumSol()
    {
        if(VaccumSolOn)
        {
            Arduino.self.WriteToArduino("a");
            VaccumSolOn = false;
        }

    }

    public static void ActivatePressureSol()
    {
        if(!PressureSolOn)
        {
            Arduino.self.WriteToArduino("9");
            PressurePumpOn = true;
        }

    }

    public static void ReleasePressureSol()
    {
        if(PressureSolOn)
        {
            Arduino.self.WriteToArduino("b");
            PressurePumpOn = false;
        }
    }
}

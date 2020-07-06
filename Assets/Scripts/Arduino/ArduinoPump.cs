using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArduinoPump {
    public static void ActivatePressurePump()
    {
        Arduino.self.WriteToArduino("2");
    }

    public static void ReleasePressurePump()
    {
        Arduino.self.WriteToArduino("6");
    }

    public static void ActivateVaccumPump()
    {
        Arduino.self.WriteToArduino("1");
    }

    public static void ReleaseVaccumPump()
    {
        Arduino.self.WriteToArduino("5");
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
        Arduino.self.WriteToArduino("8");
    }

    public static void ReleaseVaccumSol()
    {
        Arduino.self.WriteToArduino("a");
    }

    public static void ActivatePressureSol()
    {
        Arduino.self.WriteToArduino("9");
    }

    public static void ReleasePressureSol()
    {
        Arduino.self.WriteToArduino("b");
    }
}

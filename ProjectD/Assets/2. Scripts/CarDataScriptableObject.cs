using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Car Data", menuName = "Scriptable Object/Car Data", order = int.MaxValue)]
public class CarDataScriptableObject : ScriptableObject
{
    public string name;
    public HandlingData Handling;
    public EngineData Engine;
    public GearData Gear;
    public EnvironmentData Environment;

}
[SerializeField]
public class CarData
{
    public string name;
    public HandlingData Handling;
    public EngineData Engine;
    public GearData Gear;
    public EnvironmentData Environment;
}


[System.Serializable]
public class HandlingData
{
    public float MaxSteerAngle;
}

[System.Serializable]
public class EngineData
{
    public string Transmission;
    public string WheelWork;
    public int MinRPM;
    public int MaxRPM;
    public float MaxMotorTorque;
    public float MaxBrakeTorque;
    public int RPMSmoothness;
}

[System.Serializable]
public class GearData
{
    public float FinalGearRatio;
    public float[] GearRatios;
    public float ReverseRatio;
    public float ShiftUp;
    public float ShiftDown;
    public int ShiftDelay;
}

[System.Serializable]
public class EnvironmentData
{
    public float DownForceValue;
    public float DownForce;
    public float AirDragCoeff;
}

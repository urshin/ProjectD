using UnityEngine;
using System.IO;

public class JsonRead : MonoBehaviour
{
    // JSON ���� ���
    public string carName;
    [SerializeField] public CarData carData;

    void Start()
    {
        carName = GameManager.Instance.carName;
        // JSON ���� �б�
        string jsonText = File.ReadAllText("Assets\\Resources\\CarDataScriptableObject\\CarInformation\\" + carName + ".txt");

        // JSON ���ڿ��� ��ü�� ��ȯ
        carData = JsonUtility.FromJson<CarData>(jsonText);

        Invoke("SpawnPlayer", 0.5f);
    }

    void SpawnPlayer()
    {
       // gameObject.GetComponent<CarSpawner_june>().SpawnCar();
    }



[System.Serializable]
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
}

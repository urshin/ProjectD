using UnityEngine;
using System.IO;

public class JsonRead : MonoBehaviour
{
    // JSON 파일 경로
    public string carName;
    [SerializeField] public CarData carData;

    void Start()
    {
        carName = GameManager.Instance.carName;
        // JSON 파일 읽기
        string jsonText = File.ReadAllText("Assets\\Resources\\CarDataScriptableObject\\CarInformation\\" + carName + ".txt");

        // JSON 문자열을 객체로 변환
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

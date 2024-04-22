using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using UnityEditor;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public void Awake()
    {
        if (Instance == null) //정적으로 자신을 체크함, null인진
        {
            Instance = this; //이후 자기 자신을 저장함.
        }
        DontDestroyOnLoad(gameObject);
    }

    public List<GameObject> garageCarPrefab = new List<GameObject>();
    public List<GameObject> InGameCarPrefab = new List<GameObject>();
    public List<GameObject> cardataObject = new List<GameObject>();
    public List<CarData> carDatas = new List<CarData>();


    private void Start()
    {
        ParsingGarageCar();
        ParsingInGameCar();
        ParsingcarDatas();
    }


    public void ParsingGarageCar()
    {
        GameObject[] carPrefabs = Resources.LoadAll<GameObject>("CarData\\GarageCar\\");

        // 로드된 프리팹을 garageCarPrefab 리스트에 추가
        foreach (GameObject carPrefab in carPrefabs)
        {
            garageCarPrefab.Add(carPrefab);
        }
    }
    public void ParsingInGameCar()
    {
        GameObject[] carPrefabs = Resources.LoadAll<GameObject>("CarData\\CarPrefab\\");

        // 로드된 프리팹을 garageCarPrefab 리스트에 추가
        foreach (GameObject carPrefab in carPrefabs)
        {
            InGameCarPrefab.Add(carPrefab);
        }
    }


    public string jsonFolderPath = "Resources/CarInformation";
    [SerializeField] public CarData carData;
    public TextAsset[] textAsset;
    public void ParsingcarDatas()
    {
        textAsset = Resources.LoadAll<TextAsset>("CarData\\CarInformation");
       
    }

    public void UpdateCarData(TextAsset text)
    {
        carData = JsonUtility.FromJson<CarData>(text.ToString());
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

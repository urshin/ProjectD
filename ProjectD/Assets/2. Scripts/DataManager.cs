using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

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
    public List<Sprite> mapImage = new List<Sprite>();
    public List<CarData> carDatas = new List<CarData>();
    public List<Material> skyBoxs = new List<Material>();

    Dictionary<string, CarData> datas;

    private void Start()
    {
        ParsingGarageCar();
        ParsingInGameCar();
        ParsingcarDatas();
        ParsingMapData();
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
    public void ParsingMapData()
    {
        Sprite[] mapImages = Resources.LoadAll<Sprite>("MapData\\Mapimage\\");

        // 로드된 프리팹을 garageCarPrefab 리스트에 추가
        foreach (Sprite mapSprite in mapImages)
        {
            mapImage.Add(mapSprite);
        }
    }


    public string jsonFolderPath = "Resources/CarInformation";
    [SerializeField] public CarData carData;
    public CarData enemyData;
    public TextAsset[] textAsset;
    public void ParsingcarDatas()
    {
        textAsset = Resources.LoadAll<TextAsset>("CarData\\CarInformation");
       
    }

    public void UpdateCarData(TextAsset text)
    {
        carData = JsonUtility.FromJson<CarData>(text.ToString());
        Debug.Log(text.ToString());
    }

    /// <summary>
    /// AI 차량의 데이터를 바꿀 때 사용할 예정
    /// </summary>
    /// <param name="carName">차량 프리팹의 이름, 차량 프리팹과 Json파일의 이름이 같아야 한다</param>
    public void UpdateCarData(string carName)
    {
        // 생성된 차량은 뒤에 (Clone) 이 붙어 나오기 때문에 7글자를 지운 것으로 검색해야 한다
        carName = carName.Substring(0, carName.Length - 7);
        foreach(TextAsset text in textAsset)
        {
            if(text.name == carName)
            {
                enemyData = JsonUtility.FromJson<CarData>(text.ToString());
            }
        }
    }


    IEnumerator RestartIngame()
    {
        SceneManager.LoadScene("InGame");

        // 차량 정보는 이미 저장돼있으나 리스타트 플래그 세워야함

        yield return new WaitForEndOfFrame();
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

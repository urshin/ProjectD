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

    public int playerCar;
    public int enemyCar;

    public List<GameObject> garageCarPrefab = new List<GameObject>();
    public List<GameObject> InGameCarPrefab = new List<GameObject>();
    public List<Sprite> mapImage = new List<Sprite>();
    public List<CarData> carDatas = new List<CarData>();
    public List<Material> skyBoxs = new List<Material>();

    Dictionary<string, CarData> datas;

    private void Start()
    {
        CardataInitialize();
        SoundDataInitialize();
        ParsingcarDatas();
        ParsingGarageCar();
        ParsingInGameCar();
        ParsingMapData();

        SoundManager.instance.InitSoundData();
        SceneControlManager.Instance.StartLobbyScene();
    }


    public void ParsingGarageCar()
    {
        //GameObject[] carPrefabs = Resources.LoadAll<GameObject>("CarData\\GarageCar\\");

        //// 로드된 프리팹을 garageCarPrefab 리스트에 추가
        //foreach (GameObject carPrefab in carPrefabs)
        //{
        //    garageCarPrefab.Add(carPrefab);
        //}

        foreach(CarData car in carDictionary.Values)
        {
            // 순서를 맞추기 위함
            garageCarPrefab.Add(Resources.Load<GameObject>("CarData\\GarageCar\\" + car.name));
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
    //[SerializeField] public CarData carData;
    public CarData enemyData;
    public TextAsset[] textAsset;
    public void ParsingcarDatas()
    {
        textAsset = Resources.LoadAll<TextAsset>("CarData\\CarInformation");
       
    }

    public void UpdateCarData(TextAsset text)
    {
        //carData = JsonUtility.FromJson<CarData>(text.ToString());
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

    public List<CarData> cardataList = new List<CarData>();  
    CarData carsingle = new CarData();  
    [SerializeField] TextAsset textAsset2;
    public Dictionary<int, CarData> carDictionary = new Dictionary<int, CarData>();
    public void CardataInitialize()
    {
        string filePath = "CarData/CarInformation/carDatas";
        textAsset2 = Resources.Load<TextAsset>(filePath);
        string tempCarJson = "";
        if (textAsset2 != null)
        {
            string[] lines = textAsset2.text.Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line)) //공백x
                {
                    tempCarJson += line;
                }
                else                
                {
                    carsingle = JsonUtility.FromJson<CarData>(tempCarJson);
                    cardataList.Add(carsingle);
                    carDictionary.Add(carsingle.index, carsingle);
                    tempCarJson = ""; //초기화
                }
            }
        }
    }

    // 현재는 bgm 테이블이 인덱스와 곡명만 저장되므로 밸류 값이 string이지만 테이블에 내용이 추가된다면 클래스를 선언하여 저장해야 한다
    public Dictionary<int, string> bgmDictionary = new Dictionary<int, string>();
    public void SoundDataInitialize()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Sound/BGM/BGMTable");
        string tempSoundJson = "";
        if (textAsset != null)
        {
            string[] lines = textAsset.text.Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line)) //공백x
                {
                    tempSoundJson += line;
                }
                else
                {
                    BgmData bgmData = JsonUtility.FromJson<BgmData>(tempSoundJson);
                    bgmDictionary.Add(bgmData.index, bgmData.name);
                    tempSoundJson = ""; //초기화
                }
            }
        }
    }

    public string carname;
    private void Update()
    {
        //디버깅용
        if(Input.GetKeyDown(KeyCode.P))
        {
            //print(carDictionary[carname].name + "\n" + carDictionary[carname].Engine.Transmission);
        }
    }
    [System.Serializable]
    public class BgmData
    {
        public int index;
        public string name;
    }


    [System.Serializable]
    public class CarData
    {
        public int index;
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

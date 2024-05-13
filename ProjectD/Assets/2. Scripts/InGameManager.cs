using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class InGameManager : MonoBehaviour
{
    static InGameManager uniqueInstance;
    public static InGameManager instance
    {
        get { return uniqueInstance; }
    }

    [SerializeField] GameObject playerCar;
    [SerializeField] GameObject enemyCar;
    [SerializeField] Camera cam;
    Transform playerSpawnPos;
    Transform enemySpawnPos;
    [SerializeField] List<GameObject> Maps = new List<GameObject>();

    ResultUIHandler gameEndUI;

    private void Awake()
    {
        uniqueInstance = this;
    }

    private void Start()
    {
        if(Camera.main == null)
        {
            cam =Instantiate(Camera.main);
        }
        
        //InitGame();
    }

    public void InitGame(int playerCarIndex, int enemyCarIndex)
    {
        if (Camera.main == null)
        {
            cam = Instantiate(Camera.main);
        }
        gameEndUI = GameObject.FindGameObjectWithTag("UI").GetComponent<ResultUIHandler>();
        gameEndUI.DisableUI();
        MapSelection();

        // 아래 코드 동작을 위해 맵 프리팹의 첫 자식으로 GameObjects, 그 첫 자식으로 SpawnPoints, 그 아래 자식에
        // 순서대로 정방향 플레이어, 정방향 적, 역방향 플레이어, 역방향 적 스폰 포인트 지점을 만들어 둬야 한다

        GameObject map = GameObject.FindGameObjectWithTag("Map");
        if (GameManager.Instance.isReverse)
        {
            playerSpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(2);
            enemySpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(3);
        }
        else
        {
            playerSpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(0);
            enemySpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(1);
        }


        // 임시 스폰
        // 플레이어 오브젝트를 리소스에서 가져와 생성하고 그 자식으로 차량이 스폰되어야 한다, AI도 마찬가지
        //foreach (var a in DataManager.Instance.InGameCarPrefab)
        //{
        //    if (a.name == GameManager.Instance.carName)
        //    {
        //        playerCar = Instantiate(a, playerSpawnPos.position, playerSpawnPos.rotation);
        //        playerCar.tag = "Player";
        //    }
        //}



        // +AI 스폰 필요
        //int carCount = DataManager.Instance.garageCarPrefab.Count;
        enemyCar = Instantiate(Resources.Load("CarData\\Temps\\Enemy") as GameObject, enemySpawnPos.position, enemySpawnPos.rotation);
        //Instantiate(DataManager.Instance.garageCarPrefab[UnityEngine.Random.Range(0, carCount)], enemySpawnPos.position, enemySpawnPos.rotation);
        //GameObject go = Instantiate(Resources.Load("CarData\\Temps\\" + GameManager.Instance.carName) as GameObject, enemyCar.transform);
        GameObject go = Instantiate(DataManager.Instance.garageCarPrefab[enemyCarIndex], enemyCar.transform);
        
        //go.GetComponent<CinemachinController>().enabled = false;

        playerCar = Instantiate(Resources.Load("CarData\\Temps\\Player") as GameObject, playerSpawnPos.position, playerSpawnPos.rotation);
        //Instantiate(Resources.Load("CarData\\GarageCar\\" + GameManager.Instance.carName) as GameObject, playerCar.transform);
        Instantiate(DataManager.Instance.garageCarPrefab[playerCarIndex], playerCar.transform);
        //Instantiate(Resources.Load("CarData\\Temps\\Car1") as GameObject, playerCar.transform);

        SpawnCar(playerCar.GetComponentInChildren<CarController>(), playerCarIndex, false);
        SpawnCar(enemyCar.GetComponentInChildren<CarController>(), enemyCarIndex, true);

        IngameCanvasHandler.Instance.InitUI();
        go = Resources.Load("MapData\\Minimap") as GameObject;
        Instantiate(go).GetComponent<Minimap>().InitialzeMiniMap(GameManager.Instance.Map - 4, playerCar.transform.GetChild(0).gameObject, enemyCar.transform.GetChild(0).gameObject);

    }
    
    public void SpawnCar(CarController controller, int index, bool isAI)
    {
        controller.InitializeSetting(index, isAI);


        //var controller = playerCar.GetComponentInChildren<FinalCarController_June>();
        //var controller = playerCar.GetComponentInChildren<CarController>();
        //controller.maxSteerAngle = DataManager.Instance.carData.Handling.MaxSteerAngle;


        //switch (DataManager.Instance.carData.Engine.Transmission)
        //{
        //    case "Manual":
        //        controller.transmission = Transmission.Manual_Transmission;
        //        break;
        //    case "Auto":
        //        controller.transmission = Transmission.Auto_Transmission;
        //        break;
        //}
        //if (isAI)
        //{
        //    controller.transmission = Transmission.Auto_Transmission;
        //    DataManager.Instance.UpdateCarData(controller.gameObject.name);
        //}

        //if(isAI)
        //{
        //    switch (DataManager.Instance.enemyData.Engine.WheelWork)
        //    {
        //        case "FRONT":
        //            controller.wheelWork = WheelWork.FRONT;
        //            break;
        //        case "REAR":
        //            controller.wheelWork = WheelWork.REAR;
        //            break;
        //        case "AWD":
        //            controller.wheelWork = WheelWork.AWD;
        //            break;
        //    }
        //    controller.minRPM = DataManager.Instance.enemyData.Engine.MinRPM;
        //    controller.maxRPM = DataManager.Instance.enemyData.Engine.MaxRPM;
        //    controller.maxMotorTorque = DataManager.Instance.enemyData.Engine.MaxMotorTorque;
        //    controller.maxBrakeTorque = DataManager.Instance.enemyData.Engine.MaxBrakeTorque;
        //    controller.RPMSmoothness = DataManager.Instance.enemyData.Engine.RPMSmoothness;
        //    controller.finalDriveRatio = DataManager.Instance.enemyData.Gear.FinalGearRatio;
        //    controller.gearRatiosArray = DataManager.Instance.enemyData.Gear.GearRatios;
        //    controller.reverseRatio = DataManager.Instance.enemyData.Gear.ReverseRatio;
        //    controller.shiftUp = DataManager.Instance.enemyData.Gear.ShiftUp;
        //    controller.shiftDown = DataManager.Instance.enemyData.Gear.ShiftDown;
        //    controller.shiftDelay = DataManager.Instance.enemyData.Gear.ShiftDelay;
        //    controller.DownForceValue = DataManager.Instance.enemyData.Environment.DownForceValue;
        //    controller.downforce = DataManager.Instance.enemyData.Environment.DownForce;
        //    controller.airDragCoeff = DataManager.Instance.enemyData.Environment.AirDragCoeff;
        //}
        //else
        //{
        //    switch (DataManager.Instance.carData.Engine.WheelWork)
        //    {
        //        case "FRONT":
        //            controller.wheelWork = WheelWork.FRONT;
        //            break;
        //        case "REAR":
        //            controller.wheelWork = WheelWork.REAR;
        //            break;
        //        case "AWD":
        //            controller.wheelWork = WheelWork.AWD;
        //            break;
        //    }
        //    controller.minRPM = DataManager.Instance.carData.Engine.MinRPM;
        //    controller.maxRPM = DataManager.Instance.carData.Engine.MaxRPM;
        //    controller.maxMotorTorque = DataManager.Instance.carData.Engine.MaxMotorTorque;
        //    controller.maxBrakeTorque = DataManager.Instance.carData.Engine.MaxBrakeTorque;
        //    controller.RPMSmoothness = DataManager.Instance.carData.Engine.RPMSmoothness;
        //    controller.finalDriveRatio = DataManager.Instance.carData.Gear.FinalGearRatio;
        //    controller.gearRatiosArray = DataManager.Instance.carData.Gear.GearRatios;
        //    controller.reverseRatio = DataManager.Instance.carData.Gear.ReverseRatio;
        //    controller.shiftUp = DataManager.Instance.carData.Gear.ShiftUp;
        //    controller.shiftDown = DataManager.Instance.carData.Gear.ShiftDown;
        //    controller.shiftDelay = DataManager.Instance.carData.Gear.ShiftDelay;
        //    controller.DownForceValue = DataManager.Instance.carData.Environment.DownForceValue;
        //    controller.downforce = DataManager.Instance.carData.Environment.DownForce;
        //    controller.airDragCoeff = DataManager.Instance.carData.Environment.AirDragCoeff;
        //}
        
        controller.InitializeIngame();
    }

    public void MapSelection()
    {
        //맵 false로 초기화
        foreach(var map in Maps)
        {
            for(int i = 0; i<3 ; i++)
            {
                map.transform.GetChild(i).gameObject.SetActive(false);
            }
        }



        Material timeMaterial = null;
        // skybox 변수에 GameManager의 mapTimeState를 문자열로 저장
        string skybox = GameManager.Instance.mapTimeState.ToString().Replace("_","");
        foreach (Material sky in DataManager.Instance.skyBoxs)
        {
            if (sky.name.Contains(skybox, StringComparison.OrdinalIgnoreCase))
            {
                timeMaterial = sky;
            }
        }
        RenderSettings.skybox = timeMaterial;

        //선택 된 게임 맵 true
        int weather = 0;
        switch (GameManager.Instance.mapWeatherState)
        {
            case MapWeatherState.Autumn: weather =0; break;
            case MapWeatherState.Summer: weather = 1; break;
            case MapWeatherState.Winter: weather = 2; break;

        }
        Maps[GameManager.Instance.Map-4].transform.GetChild(weather).gameObject.SetActive(true);
    }

    public void OnGameEnd(bool isPlayerWin)
    {
        gameEndUI.ActiveUI(isPlayerWin, IngameCanvasHandler.Instance.lapTime);
    }
}

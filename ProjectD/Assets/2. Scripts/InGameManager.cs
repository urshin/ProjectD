using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{

    [SerializeField] GameObject playerCar;
    [SerializeField] Camera cam;
    Transform playerSpawnPos;
    Transform enemySpawnPos;

    private void Start()
    {
        if(Camera.main == null)
        {
            cam =Instantiate(Camera.main);
        }
        
        InitGame(GameManager.Instance.isReverse);
    }

    public void InitGame(bool isReverse)
    {
        // �Ʒ� �ڵ� ������ ���� �� �������� ù �ڽ����� GameObjects, �� ù �ڽ����� SpawnPoints, �� �Ʒ� �ڽĿ�
        // ������� ������ �÷��̾�, ������ ��, ������ �÷��̾�, ������ �� ���� ����Ʈ ������ ����� �־� �Ѵ�
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        if (isReverse)
        {
            playerSpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(2);
            enemySpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(3);
        }
        else
        {
            playerSpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(0);
            enemySpawnPos = map.transform.GetChild(0).GetChild(0).GetChild(1);
        }


        // �ӽ� ����
        // �÷��̾� ������Ʈ�� ���ҽ����� ������ �����ϰ� �� �ڽ����� ������ �����Ǿ�� �Ѵ�, AI�� ��������
        //foreach (var a in DataManager.Instance.InGameCarPrefab)
        //{
        //    if (a.name == GameManager.Instance.carName)
        //    {
        //        playerCar = Instantiate(a, playerSpawnPos.position, playerSpawnPos.rotation);
        //        playerCar.tag = "Player";
        //    }
        //}

        playerCar = Instantiate(Resources.Load("CarData\\Temps\\Player") as GameObject, playerSpawnPos.position, playerSpawnPos.rotation);
        Instantiate(Resources.Load("CarData\\Temps\\Car1"/* + GameManager.Instance.carName*/) as GameObject, playerCar.transform);



        // +AI ���� �ʿ�
        int carCount = DataManager.Instance.garageCarPrefab.Count;
        Instantiate(DataManager.Instance.garageCarPrefab[Random.Range(0, carCount)], enemySpawnPos.position, enemySpawnPos.rotation);

        SpawnCar();
    }
    
    public void SpawnCar()
    {
        //var controller = playerCar.GetComponentInChildren<FinalCarController_June>();
        var controller = playerCar.GetComponentInChildren<CarController>();
        controller.maxSteerAngle = DataManager.Instance.carData.Handling.MaxSteerAngle;

        switch (DataManager.Instance.carData.Engine.Transmission)
        {
            case "Manual":
                controller.transmission = Transmission.Manual_Transmission;
                break;
            case "Auto":
                controller.transmission = Transmission.Auto_Transmission;
                break;
        }

        switch (DataManager.Instance.carData.Engine.WheelWork)
        {
            case "FRONT":
                controller.wheelWork = WheelWork.FRONT;
                break;
            case "REAR":
                controller.wheelWork = WheelWork.REAR;
                break;
            case "AWD":
                controller.wheelWork = WheelWork.AWD;
                break;
        }
        controller.minRPM = DataManager.Instance.carData.Engine.MinRPM;
        controller.maxRPM = DataManager.Instance.carData.Engine.MaxRPM;
        controller.maxMotorTorque = DataManager.Instance.carData.Engine.MaxMotorTorque;
        controller.maxBrakeTorque = DataManager.Instance.carData.Engine.MaxBrakeTorque;
        controller.RPMSmoothness = DataManager.Instance.carData.Engine.RPMSmoothness;
        controller.finalDriveRatio = DataManager.Instance.carData.Gear.FinalGearRatio;
        controller.gearRatiosArray = DataManager.Instance.carData.Gear.GearRatios;
        controller.reverseRatio = DataManager.Instance.carData.Gear.ReverseRatio;
        controller.shiftUp = DataManager.Instance.carData.Gear.ShiftUp;
        controller.shiftDown = DataManager.Instance.carData.Gear.ShiftDown;
        controller.shiftDelay = DataManager.Instance.carData.Gear.ShiftDelay;
        controller.DownForceValue = DataManager.Instance.carData.Environment.DownForceValue;
        controller.downforce = DataManager.Instance.carData.Environment.DownForce;
        controller.airDragCoeff = DataManager.Instance.carData.Environment.AirDragCoeff;




    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : MonoBehaviour
{

    [SerializeField] GameObject playerCar;
    [SerializeField] Camera cam;

    private void Start()
    {
        if(Camera.main == null)
        {

        cam =Instantiate(Camera.main);
        }
        

        
        foreach(var a in DataManager.Instance.InGameCarPrefab)
        {
            if(a.name == GameManager.Instance.carName)
            {
                playerCar = Instantiate(a,gameObject.transform);
            }
        }
        
        SpawnCar();
    }


    
    public void SpawnCar()
    {
        var controller = playerCar.GetComponent<FinalCarController_June>();
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

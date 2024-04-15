using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner_june: MonoBehaviour
{
    [SerializeField] GameObject CarPrefab;
    [SerializeField] string CarName;
 
    public CarData CarData;

    void Start()
    {

        CarName = GameManager.Instance.Carname;
        CarPrefab = Resources.Load<GameObject>("CarData\\CarPrefab\\"+CarName);
        
    }

    public void SpawnCar()
    {
        CarData =gameObject.GetComponent<JsonRead>().carData;
        GameObject car = Instantiate(CarPrefab);
        var controller =  car.GetComponent<FinalCarController_June>();
        controller.maxSteerAngle = CarData.Handling.MaxSteerAngle;

        switch (CarData.Engine.Transmission)
        {
            case "Manual":
                controller.transmission = Transmission.Manual_Transmission;
                break;
            case "Auto":
                controller.transmission = Transmission.Auto_Transmission;
                break;
        }

        switch (CarData.Engine.WheelWork)
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
        controller.minRPM = CarData.Engine.MinRPM;
        controller.maxRPM = CarData.Engine.MaxRPM;
        controller.maxMotorTorque = CarData.Engine.MaxMotorTorque;
        controller.maxBrakeTorque = CarData.Engine.MaxBrakeTorque;
        controller.RPMSmoothness = CarData.Engine.RPMSmoothness;
        controller.finalDriveRatio = CarData.Gear.FinalGearRatio;
        controller.gearRatiosArray = CarData.Gear.GearRatios;
        controller.reverseRatio = CarData.Gear.ReverseRatio;
        controller.shiftUp = CarData.Gear.ShiftUp;
        controller.shiftDown = CarData.Gear.ShiftDown;
        controller.shiftDelay = CarData.Gear.ShiftDelay;
        controller.DownForceValue = CarData.Environment.DownForceValue;
        controller.downforce = CarData.Environment.DownForce;
        controller.airDragCoeff = CarData.Environment.AirDragCoeff;




    }


}

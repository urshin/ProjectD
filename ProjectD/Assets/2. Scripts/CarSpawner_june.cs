//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CarSpawner_june : MonoBehaviour
//{
//    [SerializeField] GameObject CarPrefab;
//    [SerializeField] string CarName;

//    public CarDataScriptableObject CarDataScriptableObject;

//    void Start()
//    {

//        CarName = GameManager.Instance.carName;
//        CarPrefab = Resources.Load<GameObject>("CarDataScriptableObject\\CarPrefab\\" + CarName);

//    }

//    public void SpawnCar()
//    {
//        CarDataScriptableObject = gameObject.GetComponent<JsonRead>().carData;
//        GameObject car = Instantiate(CarPrefab);
//        var controller = car.GetComponent<FinalCarController_June>();
//        controller.maxSteerAngle = CarDataScriptableObject.Handling.MaxSteerAngle;

//        switch (CarDataScriptableObject.Engine.Transmission)
//        {
//            case "Manual":
//                controller.transmission = Transmission.Manual_Transmission;
//                break;
//            case "Auto":
//                controller.transmission = Transmission.Auto_Transmission;
//                break;
//        }

//        switch (CarDataScriptableObject.Engine.WheelWork)
//        {
//            case "FRONT":
//                controller.wheelWork = WheelWork.FRONT;
//                break;
//            case "REAR":
//                controller.wheelWork = WheelWork.REAR;
//                break;
//            case "AWD":
//                controller.wheelWork = WheelWork.AWD;
//                break;
//        }
//        controller.minRPM = CarDataScriptableObject.Engine.MinRPM;
//        controller.maxRPM = CarDataScriptableObject.Engine.MaxRPM;
//        controller.maxMotorTorque = CarDataScriptableObject.Engine.MaxMotorTorque;
//        controller.maxBrakeTorque = CarDataScriptableObject.Engine.MaxBrakeTorque;
//        controller.RPMSmoothness = CarDataScriptableObject.Engine.RPMSmoothness;
//        controller.finalDriveRatio = CarDataScriptableObject.Gear.FinalGearRatio;
//        controller.gearRatiosArray = CarDataScriptableObject.Gear.GearRatios;
//        controller.reverseRatio = CarDataScriptableObject.Gear.ReverseRatio;
//        controller.shiftUp = CarDataScriptableObject.Gear.ShiftUp;
//        controller.shiftDown = CarDataScriptableObject.Gear.ShiftDown;
//        controller.shiftDelay = CarDataScriptableObject.Gear.ShiftDelay;
//        controller.DownForceValue = CarDataScriptableObject.Environment.DownForceValue;
//        controller.downforce = CarDataScriptableObject.Environment.DownForce;
//        controller.airDragCoeff = CarDataScriptableObject.Environment.AirDragCoeff;




//    }


//}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CarControllerv2_June;





public class Test : MonoBehaviour
{    // WheelColliders 클래스 선언
    [System.Serializable]
    public class WheelColliders
    {
        public WheelCollider FRWheel;
        public WheelCollider FLWheel;
        public WheelCollider RRWheel;
        public WheelCollider RLWheel;
    }
    public enum GearState
    {
        Neutral,        // 중립 상태일 때
        Running,        // 엔진이 작동 중일 때
        CheckingChange, // 기어 변경을 확인 중일 때
        Changing        // 기어를 변경 중일 때
    };

    public enum Transmission
    {
        Auto_Transmission,
        Manual_Transmission,
    }
    // 플레이어의 Rigidbody 구성 요소
    private Rigidbody playerRB;
    public Vector3 _centerOfMass;

    // 자동차의 바퀴에 대한 WheelColliders
    public WheelColliders colliders;

    // 스티어링 슬라이더
    public Slider steerSlider;

    // 입력 변수
    private float gasInput;

    // 자동차 이동을 위한 매개변수

    //핸들링
    public float maxSteerAngle;
    public float sensitivity;

    //엔진
    public Transmission transmission;
    public float minRPM; //최소RPM
    public float motorRPM; //모터 RPM
    public float wheelRPM; //바퀴 RPM
    public float finalDriveRatio; //최종 구동 비율 3~4 사이
    public float totalMotorTorque; //최종 모터 토크
    public AnimationCurve gearRatios;//기어 비율
    public AnimationCurve torqueCurve;//토크 커브
    public float gearindex;


    //info
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public float speed;
    public float wheelDiameter;




    private void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
        wheelDiameter = colliders.RLWheel.radius;
    }


    private void Update()
    {
        //wheelRPM = colliders.RLWheel.rpm;
        wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f);

        //차속 (km/h) = 2pi * 타이어반지름 * 엔진RPM/(번속기 기어비 x 종감속 기어비)x60/1000
        float tireCircumference = 2 * Mathf.PI * (wheelDiameter / 2);
        speed = tireCircumference * (motorRPM / (gearRatios.Evaluate(gearindex) * finalDriveRatio)) * (60f / 1000f);




        GetInput();
        gearText.text = gearindex.ToString();
        //speedText.text = speed.ToString() ;
        speedText.text = playerRB.velocity.magnitude.ToString();

        if (!colliders.RRWheel.isGrounded)
        {
            Debug.Log(Time.fixedTime);
        }

        ApplyGear();

    }
    private void FixedUpdate()
    {
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
    }


    void GetInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        if (mouseX != 0)
        {
            //steerSlider.value += mouseX;
        }
        gasInput = Input.GetAxis("Vertical");
        print(gasInput);



    }


    public void ApplyMotor()
    {
        motorRPM = minRPM + (wheelRPM * finalDriveRatio * gearRatios.Evaluate(gearindex));
        totalMotorTorque = torqueCurve.Evaluate(motorRPM) * gearRatios.Evaluate(gearindex) * finalDriveRatio * gasInput;
        colliders.RLWheel.motorTorque = totalMotorTorque / 2; //뒷바퀴만
        colliders.RRWheel.motorTorque = totalMotorTorque / 2;
    }

    public void ApplySteering()
    {
        float steeringAngle;
        steeringAngle = steerSlider.value * maxSteerAngle / (1080.0f / 2);
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }
    public void ApplyBrake()
    {

    }
    public void ApplyGear()
    {
        if (transmission == Transmission.Manual_Transmission)
        {
            if (Input.GetKeyDown(KeyCode.E) && gearindex < finalDriveRatio)
            {
                gearindex++;
            }
            else if (Input.GetKeyDown(KeyCode.Q) && gearindex > 0)
            {
                gearindex--;
            }
        }
    }

}

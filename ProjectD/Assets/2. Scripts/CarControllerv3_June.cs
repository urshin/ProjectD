using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CarControllerv2_June;





public class CarControllerv3_June : MonoBehaviour
{
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
    private float brakeInput;

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

    //
    private bool handrbake = false; //핸드 브레이크
    public int isEngineRunning;// 엔진이 작동 중인지를 나타내는 변수
    public GearState gearState;    // 현재 기어 상태를 나타내는 변수 (중립, 작동 중, 기어 변경 확인 중, 기어 변경 중)
    public float slipAngle;
    public float clutch; // 클러치 상태를 나타내는 변수 (0에서 1 사이의 값)
    public float brakePower;
    public float increaseGearRPM;    // RPM이 증가할 때 기어 변경을 위해 필요한 추가 RPM
    public float decreaseGearRPM;    // RPM이 감소할 때 기어 변경을 위해 필요한 추가 RPM
    public float redLine;
    public TextMeshProUGUI gearratiostext;
    public TextMeshProUGUI torqueCurvetext;



    public AnimationCurve test_gearRatios;
    public AnimationCurve test_torqueCurve;
    public float[] test_gearRatiosArray; 
    public float[] test_torqueCurveArray; 




    private void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
        wheelDiameter = colliders.RLWheel.radius;
        MakingGearCurve();


    }
    //기어비 만들기
    void MakingGearCurve()
    {
        for (int i = -1; i < test_gearRatiosArray.Length; i++)
        {
            test_gearRatios.AddKey(i, test_gearRatiosArray[i + 1]);
        }
    }


    private void Update()
    {
        //wheelRPM = colliders.RLWheel.rpm;
        wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f);

        //차속 (km/h) = 2pi * 타이어반지름 * 엔진RPM/(번속기 기어비 x 종감속 기어비)x60/1000
        float tireCircumference = 2 * Mathf.PI * (wheelDiameter / 2);
        speed =  tireCircumference * (motorRPM / (gearRatios.Evaluate(gearindex) * finalDriveRatio)) * (60f / 1000f);
        gearratiostext.text = "gearRatios :"+gearRatios.Evaluate(gearindex).ToString();
        torqueCurvetext.text = "torqueCurve : "+ torqueCurve.Evaluate(motorRPM).ToString();



        GetInput();
        gearText.text = "Current Gear : "+gearindex.ToString();
        //speedText.text = speed.ToString() ;
        speedText.text = "Speed : " + playerRB.velocity.magnitude.ToString();

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
        //print(gasInput);
        handrbake = (Input.GetKey(KeyCode.Space));
        if (gasInput > 0)
        {
            gasInput += Mathf.Clamp01(sensitivity * Time.deltaTime);
        }
        if (gasInput < 0)
        {
            gasInput -= Mathf.Clamp01(sensitivity * Time.deltaTime);
        }
        // 엔진이 꺼져 있고 가스 입력이 있을 때 엔진 시작
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            if (transmission == Transmission.Auto_Transmission)
                gearindex++;
            gearState = GearState.Running;
            isEngineRunning = 1;
        }
        // 속도 방향에 따라 브레이크 입력 조절
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs(gasInput) > 0) gearState = GearState.Running;
            }
            else
            {
                clutch = Input.GetKey(KeyCode.LeftShift) ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0;
        }
        if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else
        {
            brakeInput = 0;
        }

    }


    public void ApplyMotor()
    {
        float currentTorque = CalculateTorque();

        colliders.RLWheel.motorTorque = currentTorque / 2; //뒷바퀴만
        colliders.RRWheel.motorTorque = currentTorque / 2;
    }
    float CalculateTorque()
    {
        float torque = 0;
        if (motorRPM < minRPM + 200 && gasInput == 0 && gearindex == 0)
        {
            gearState = GearState.Neutral;
        }
        if (gearState == GearState.Running && clutch > 0)
        {
            if (transmission == Transmission.Auto_Transmission)
            {

                if (motorRPM > increaseGearRPM)
                {
                    StartCoroutine(ChangeGear(1));
                }
                else if (motorRPM < decreaseGearRPM && gearindex > 1)
                {
                    StartCoroutine(ChangeGear(-1));
                }
            }

            if (transmission == Transmission.Manual_Transmission)
            {

                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(ChangeGear(1));
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    StartCoroutine(ChangeGear(-1));
                }
            }



        }
        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                totalMotorTorque = Mathf.Lerp(motorRPM, Mathf.Max(minRPM, redLine * gasInput) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                motorRPM = minRPM + (wheelRPM * finalDriveRatio * gearRatios.Evaluate(gearindex));
                totalMotorTorque = torqueCurve.Evaluate(motorRPM) * gearRatios.Evaluate(gearindex) * finalDriveRatio * gasInput;
                torque = totalMotorTorque;
            }
        }
        return torque;
    }


    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (gearindex + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                // 기어 올리기
                yield return new WaitForSeconds(0.7f);
                if (motorRPM < increaseGearRPM || gearindex >= finalDriveRatio)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                // 기어 내리기
                yield return new WaitForSeconds(0.1f);
                if (motorRPM > decreaseGearRPM || gearindex <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(0.2f);
            gearindex += gearChange;
        }

        if (gearState != GearState.Neutral)
            gearState = GearState.Running;
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
        colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        if (handrbake)
        {
            clutch = 0;
            colliders.RRWheel.brakeTorque = brakePower * 1000f;
            colliders.RLWheel.brakeTorque = brakePower * 1000f;
        }
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

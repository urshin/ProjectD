using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CarControllerv2_June;

// 자동차의 기어 상태를 나타내는 열거형
public enum GearState
{
    Neutral,        // 중립 상태일 때
    Running,        // 엔진이 작동 중일 때
    CheckingChange, // 기어 변경을 확인 중일 때
    Changing        // 기어를 변경 중일 때
};

// CarController_June 클래스 선언
public class CarController_June : MonoBehaviour
{
    // 플레이어의 Rigidbody 구성 요소
    private Rigidbody playerRB;
    public Vector3 _centerOfMass;

    // 자동차의 바퀴에 대한 WheelColliders
    public WheelColliders colliders;

    // 입력 변수
    private float gasInput;
    private float brakeInput;

    // 자동차 이동을 위한 매개변수
    public float motorPower;
    public float brakePower;
    public float slipAngle;
    public float speed;
    private float speedClamped;
    public float maxSpeed;
    public AnimationCurve steeringCurve;
    public float sensitivity;

    //엔진 관련
    public int isEngineRunning;// 엔진이 작동 중인지를 나타내는 변수
    public float RPM;// 현재 RPM(Revolutions Per Minute, 분당 회전수)을 저장하는 변수
    public float redLine;// 엔진의 최대 회전 속도를 나타내는 변수
    public float idleRPM;// 엔진이 대기 상태일 때의 RPM
    public TMP_Text rpmText;
    public int currentGear;// 현재 기어를 나타내는 변수
    public float[] gearRatios; // 엔진 출력(RPM에 따라 변하는 엔진 출력)에 따른 기어 비율을 저장하는 배열
    public float differentialRatio; // 변속기(differential) 비율
    private float currentTorque; // 현재 토크 값을 저장하는 변수
    private float clutch; // 클러치 상태를 나타내는 변수 (0에서 1 사이의 값)
    private float wheelRPM;  // 바퀴 RPM을 저장하는 변수
    public AnimationCurve hpToRPMCurve;    // 엔진 출력과 RPM 간의 관계를 정의하는 커브
    private GearState gearState;    // 현재 기어 상태를 나타내는 변수 (중립, 작동 중, 기어 변경 확인 중, 기어 변경 중)
    public float increaseGearRPM;    // RPM이 증가할 때 기어 변경을 위해 필요한 추가 RPM
    public float decreaseGearRPM;    // RPM이 감소할 때 기어 변경을 위해 필요한 추가 RPM
    public float changeGearTime = 0.5f;    // 기어를 변경하는 데 걸리는 시간
    private bool handrbake = false; //핸드 브레이크


    // Start is called before the first frame update
    void Start()
    {
        // Rigidbody 구성 요소 가져오기
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
    }

    // Update is called once per frame
    void Update()
    {
        // RPM 및 속도 갱신
        rpmText.text = RPM.ToString("0,000") + "rpm";
        speed = colliders.RRWheel.rpm * colliders.RRWheel.radius * 2f * Mathf.PI / 10f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);

        // 입력 확인
        CheckInput();
    }

    // 물리 업데이트마다 호출되는 함수
    private void FixedUpdate()
    {
        // 자동차 제어 함수 호출
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
       
    }

    // 스티어링 슬라이더
    public Slider steerSlider;

    // 입력 확인 함수
    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(isEngineRunning ==0)
            {
                isEngineRunning++;
            }
            else if (isEngineRunning == 1)
            {
                isEngineRunning--;
            }
        }
        handrbake = (Input.GetKey(KeyCode.Space));

        // 가스 및 브레이크 입력 확인
        gasInput = Input.GetAxis("Vertical");
        if (gasInput > 0)
        {
            gasInput += sensitivity * Time.deltaTime;
        }
        if (gasInput < 0)
        {
            gasInput -= sensitivity * Time.deltaTime;
        }

        // 엔진이 꺼져 있고 가스 입력이 있을 때 엔진 시작
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            gearState = GearState.Running;
        }

        // 마우스 X 입력에 따라 스티어링 조절
        float mouseX = Input.GetAxis("Mouse X");
        if (mouseX != 0)
        {
            steerSlider.value += mouseX;
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



    public float originFriction = 1f;
    public float handbreakFriction = 0.8f;

    WheelFrictionCurve fFriction;
    WheelFrictionCurve sFriction;

    // 브레이크 적용 함수
    void ApplyBrake()
    {
        colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        if (handrbake)
        {
            //fFriction.stiffness = handbreakFriction;
            //sFriction.stiffness = handbreakFriction;
            //colliders.RRWheel.forwardFriction = fFriction;
            //colliders.RRWheel.sidewaysFriction = sFriction;
            //colliders.RLWheel.forwardFriction = fFriction;
            //colliders.RLWheel.sidewaysFriction = sFriction;

            clutch = 0;
            colliders.RRWheel.brakeTorque = brakePower * 1000f;
            colliders.RLWheel.brakeTorque = brakePower * 1000f;
        }
        
    }

 


    // 모터 적용 함수
    void ApplyMotor()
    {
        currentTorque = CalculateTorque();
        colliders.RRWheel.motorTorque = currentTorque * gasInput;
        colliders.RLWheel.motorTorque = currentTorque * gasInput;
    }

    // 토크 계산 함수
    float CalculateTorque()
    {
        float torque = 0;
        if (RPM < idleRPM + 200 && gasInput == 0 && currentGear == 0)
        {
            gearState = GearState.Neutral;
        }
        if (gearState == GearState.Running && clutch > 0)
        {
            if (RPM > increaseGearRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < decreaseGearRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, redLine * gasInput) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f) * gearRatios[currentGear] * differentialRatio;
                RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM - 100, wheelRPM), Time.deltaTime * 3f);
                torque = (hpToRPMCurve.Evaluate(RPM / redLine) * motorPower / RPM) * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
            }
        }
        return torque;
    }

    // 스티어링 적용 함수
    void ApplySteering()
    {
        float steeringAngle;
        steeringAngle = steeringCurve.Evaluate(speed) *steerSlider.value * 30 / (1080.0f / 2);
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    // 기어 변경 코루틴
    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                // 기어 올리기
                yield return new WaitForSeconds(0.7f);
                if (RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                // 기어 내리기
                yield return new WaitForSeconds(0.1f);
                if (RPM > decreaseGearRPM || currentGear <= 0)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(changeGearTime);
            currentGear += gearChange;
        }

        if (gearState != GearState.Neutral)
            gearState = GearState.Running;
    }
}

// WheelColliders 클래스 선언
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}
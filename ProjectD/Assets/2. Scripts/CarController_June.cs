using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CarControllerv2_June;


public class CarController_June : MonoBehaviour
{
    // WheelColliders 클래스 선언
    [System.Serializable]
    public class Pair
    {
        public WheelCollider left; // 왼쪽 바퀴 콜라이더
        public WheelCollider right; // 오른쪽 바퀴 콜라이더
        public bool IsMoterPower; // 모터 힘 받는지
        public bool IsSteering; //스티어 힘 받는지
        public float brakeBias = 0.5f; //브레이크 전달 크기
        public float handbrakeBias = 5f; //핸드브레이크 전달 크기
        [System.NonSerialized] //public이라도 인스펙터 창에서 가리기 위함
        public WheelHit hitLeft; // 왼쪽 휠 히트
        [System.NonSerialized]
        public WheelHit hitRight; // 오른쪽 휠 히트
        //[System.NonSerialized]
        public bool isGroundedLeft = false; // 왼쪽이 땅에 붙어있는지 여부
        //[System.NonSerialized]
        public bool isGroundedRight = false; // 오른쪽이 땅에 붙어있는지 여부
    }

    public List<Pair> pair;





    public enum WheelWork //전,후,4륜
    {
        FRONT,
        REAR,
        AWD,
    };

    public enum Transmission
    {
        Auto_Transmission,
        Manual_Transmission,
    }
    // 플레이어의 Rigidbody 구성 요소
    private Rigidbody playerRB;
    public Vector3 _centerOfMass;



    // 스티어링 슬라이더
    public Slider steerSlider;



    // 자동차 이동을 위한 매개변수

    //핸들링
    public float maxSteerAngle;
    public float sensitivity;

    //엔진
    public Transmission transmission;
    public WheelWork wheelWork;
    public float minRPM; //최소RPM
    public float maxRPM; //최소RPM
    public float currentRPM; //모터 RPM
    public float wheelRPM; //바퀴 RPM
    public float finalDriveRatio; //최종 구동 비율 3~4 사이
    public float maxMotorTorque;  // 토크 커브의 피크에서의 토크
    public float maxBrakeTorque;// 최대 브레이크 토크
    public float totalMotorTorque; //최종 모터 토크
    public AnimationCurve gearRatiosCurve;//기어 비율
    public float[] gearRatiosArray;
    public AnimationCurve torqueCurve;//토크 커브


    //기어
    public AnimationCurve shiftUpCurve; //기어 올리는거
    public AnimationCurve shiftDownCurve; //기어 내리는거
    private float lastShift = 0.0f; //마지막 기어 시간
    public float shiftDelay = 1.0f; //기어 바꾸는 시간
    private int targetGear = 1; //목표로 하는 기어
    private int lastGear = 1; //마지막 기어
    private bool shifting = false; //기어 변속중?
    public float shiftTime = 0.4f;// 기어 변속이 완료되는 시간 (보간됨)

    //info
    public float motorWheelNum; //모터 힘을 받는 바퀴 수
    public float currentGear; //현재 기어
    public float KPH; // 속도
    [SerializeField] bool LockWheel;
    public float airDragCoeff;
    public float airDownForceCoeff;
    public float tireDragCoeff;


    //input
    [SerializeField] float mouseX;
    [SerializeField] float gasInput;
    [SerializeField] float brakeInput;


    //ui
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI gearratiostext;
    public TextMeshProUGUI torqueCurvetext;
    public TextMeshProUGUI wheelRPMtext;

    private void OnEnable()
    {
        InitializedSetting();
    }
    private void Update()
    {
        GetInput();
        UpdateUI();
    }
    private void FixedUpdate()
    {
        if (!LockWheel) Steering();
        ApplySteering();

        CalculateTorque();
        ApplyTorque();
        CheckingIsGrounded();
        DragAndDownForce();


        if (transmission == Transmission.Auto_Transmission)
        {
            AutoGear();
        }
        else if (transmission == Transmission.Manual_Transmission)
        {
            ManualGear();
        }
    }



    public void GetInput()
    {
        mouseX = Input.GetAxis("Mouse X");
        gasInput = Input.GetAxis("Vertical");

    }

    //초기화(기어비, 토크 등) 및 ui등
    void InitializedSetting()
    {
        RecalcDrivingWheels();
        MakingGearRatiotoCurve();
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
    }

    public void RecalcDrivingWheels()
    {
        //calculate how many wheels are driving
        motorWheelNum = pair.Where(a => a.IsMoterPower).Count() * 2;
    }
    public void MakingGearRatiotoCurve()
    {
        for (int i = 0; i < gearRatiosArray.Length; i++)
        {
            gearRatiosCurve.AddKey(i, gearRatiosArray[i]);
        }
    }

    public void UpdateUI()
    {
        KPH = Mathf.RoundToInt(playerRB.velocity.magnitude * 2.23693629f * 1.6f);
        speedText.text = "Km/h " + KPH;
        gearText.text = "GEAR " + (Mathf.RoundToInt(currentGear));
        torqueCurvetext.text = "Torque " + torqueCurve.Evaluate(currentRPM / maxRPM);
    }

    public void DragAndDownForce()
    {
        //air drag (quadratic)
        playerRB.AddForce(-airDragCoeff * playerRB.velocity * playerRB.velocity.magnitude);

        //downforce (quadratic)
        playerRB.AddForce(-airDownForceCoeff * playerRB.velocity.sqrMagnitude * transform.up);

        //tire drag (Linear)
        playerRB.AddForceAtPosition(-tireDragCoeff * playerRB.velocity, transform.position);
    }
    //스티어링 관련
    #region
    public void Steering()//fixedUpdate
    {

        if (mouseX != 0)
        {
            steerSlider.value += mouseX * sensitivity;
        }
    }

    public void ApplySteering()//fixedUpdate
    {
        float steeringAngle;
        //스티어 앵글 계산
        steeringAngle = steerSlider.value * maxSteerAngle / (1080.0f / 2);

        //스티어링 바퀴에만 적용
        foreach (var wheel in pair)
        {
            if (wheel.IsSteering)
            {
                wheel.left.steerAngle = steeringAngle;
                wheel.right.steerAngle = steeringAngle;
            }
        }
    }
    #endregion


    //accel and brake
    #region

    //토크 적용
    private void ApplyTorque()//fixedUpdate
    {

        if (gasInput >= 0)
        {
            //motor
            float torquePerWheel = gasInput * (totalMotorTorque / motorWheelNum);
            foreach (var wheel in pair)
            {
                if (wheel.IsMoterPower)
                {
                    if (wheel.isGroundedLeft)
                        wheel.left.motorTorque = torquePerWheel;
                    else
                        wheel.left.motorTorque = 0f;

                    if (wheel.isGroundedRight)
                        wheel.right.motorTorque = torquePerWheel;
                    else
                        wheel.left.motorTorque = 0f;
                }

                wheel.left.brakeTorque = 0f;
                wheel.right.brakeTorque = 0f;
            }

        }
        else
        {
            //brakes 
            foreach (var wheel in pair)
            {
                var brakeTorque = maxBrakeTorque * gasInput * -1 * wheel.brakeBias;
                wheel.left.brakeTorque = brakeTorque;
                wheel.right.brakeTorque = brakeTorque;
                wheel.left.motorTorque = 0f;
                wheel.right.motorTorque = 0f;
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in pair)
            {
                var brakeTorque = maxBrakeTorque * wheel.handbrakeBias;
                wheel.left.brakeTorque = brakeTorque;
                wheel.right.brakeTorque = brakeTorque;

            }


        }
    }

    public void CalculateTorque()//fixedUpdate
    {
        float gearRatio = gearRatiosCurve.Evaluate(currentGear); //이거 써보고 안되면 위에꺼로 써보기
        wheelRPM = (pair[1].right.rpm + pair[1].left.rpm) / 2f * gearRatio; //바퀴 RPM //나중에 Rear로 쓸 지 고민해보기
        if (wheelRPM < 0)
            wheelRPM = 0;
        currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * 2); //2있는 곳은 나중에 수치로 변환 시켜보기

        if (currentRPM > maxRPM)
        {
            currentRPM = maxRPM; //최대 RPM 제한
            totalMotorTorque = 0; //토크에 0을 줌으로써 일정 속도로 유지 되게
        }
        else
        {

            totalMotorTorque = torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio * finalDriveRatio * maxMotorTorque; // maxMotorTorque있는 곳은 나중에 tractionControlAdjustedMaxTorque생각해보기
        }


    }

    public void CheckingIsGrounded()//땅체크
    {
        foreach (var wheel in pair)
        {
            wheel.isGroundedLeft = wheel.left.GetGroundHit(out wheel.hitLeft);
            wheel.isGroundedRight = wheel.right.GetGroundHit(out wheel.hitRight);
        }
    }



    #endregion


    //기어 변환
    //auto
    private void AutoGear()
    {
        // 변속이 너무 빠르게 일어나지 않도록 지연 시간을 확인
        if (Time.time - lastShift > shiftDelay)
        {
            // 상승 변속
            if (currentRPM / maxRPM > shiftUpCurve.Evaluate(gasInput) && Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
            {
                // 1기어에서 단순히 회전 중인 경우 상승 변속하지 않음
                // 또는 1기어에서 15km/h 이상 이동 중인 경우에만 상승 변속합니다.
                if (Mathf.RoundToInt(currentGear) > 1 || KPH > 15f)
                {
                    // 마지막 변속 시간을 기록합니다.
                    lastGear = Mathf.RoundToInt(currentGear);
                    // 상승 변속할 기어를 설정합니다.
                    targetGear = lastGear + 1;
                    // 마지막 변속 시간을 기록하고, 변속 중임을 표시합니다.
                    lastShift = Time.time;
                    shifting = true;
                }
            }
            // 하락 변속
            else if (currentRPM / maxRPM < shiftDownCurve.Evaluate(gasInput) && Mathf.RoundToInt(currentGear) > 1)
            {
                // 마지막 변속 시간을 기록합니다.
                lastGear = Mathf.RoundToInt(currentGear);
                // 하락 변속할 기어를 설정합니다.
                targetGear = lastGear - 1;
                // 마지막 변속 시간을 기록하고, 변속 중임을 표시합니다.
                lastShift = Time.time;
                shifting = true;
            }
        }

        // 변속 중인 경우
        if (shifting)
        {
            // 시간에 따른 보간값을 계산하여 현재 기어를 조정합니다.
            float lerpVal = (Time.time - lastShift) / shiftTime;
            currentGear = Mathf.Lerp(lastGear, targetGear, lerpVal);
            // 보간이 완료되면 변속 중 상태를 해제합니다.
            if (lerpVal >= 1f)
                shifting = false;
        }

        // 기어 범위를 벗어나지 않도록 클램핑합니다.
        if (currentGear >= gearRatiosArray.Length)
        {
            currentGear = gearRatiosArray.Length;
        }
        else if (currentGear < 1)
        {
            currentGear = 1;
        }
    }
    //manual
    private void ManualGear()
    {
        // 변속이 너무 빠르게 일어나지 않도록 지연 시간을 확인
        if (Time.time - lastShift > shiftDelay)
        {
            // 상승 변속
            if (Input.GetKey(KeyCode.E))
            {
                // 1기어에서 단순히 회전 중인 경우 상승 변속하지 않음
                // 또는 1기어에서 15km/h 이상 이동 중인 경우에만 상승 변속합니다.
                if (Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
                {
                    // 마지막 변속 시간을 기록합니다.
                    lastGear = Mathf.RoundToInt(currentGear);
                    // 상승 변속할 기어를 설정합니다.
                    targetGear = lastGear + 1;
                    // 마지막 변속 시간을 기록하고, 변속 중임을 표시합니다.
                    lastShift = Time.time;
                    shifting = true;
                }
            }
            // 하락 변속
            else if (Input.GetKey(KeyCode.Q))
            {
                if (Mathf.RoundToInt(currentGear) > 1)
                {

                    // 마지막 변속 시간을 기록합니다.
                    lastGear = Mathf.RoundToInt(currentGear);
                    // 하락 변속할 기어를 설정합니다.
                    targetGear = lastGear - 1;
                    // 마지막 변속 시간을 기록하고, 변속 중임을 표시합니다.
                    lastShift = Time.time;
                    shifting = true;
                }

            }
        }

        // 변속 중인 경우
        if (shifting)
        {
            // 시간에 따른 보간값을 계산하여 현재 기어를 조정합니다.
            float lerpVal = (Time.time - lastShift) / shiftTime;
            currentGear = Mathf.Lerp(lastGear, targetGear, lerpVal);
            // 보간이 완료되면 변속 중 상태를 해제합니다.
            if (lerpVal >= 1f)
                shifting = false;
        }

        // 기어 범위를 벗어나지 않도록 클램핑합니다.
        if (currentGear >= gearRatiosArray.Length)
        {
            currentGear = gearRatiosArray.Length - 1;
        }
        else if (currentGear < 1)
        {
            currentGear = 1;
        }
    }



}

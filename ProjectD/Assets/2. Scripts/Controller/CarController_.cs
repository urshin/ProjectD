using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
public class CarController_ : MonoBehaviour
{
    // 차량 프리팹에 심어질 스크립트
    // 차량의 거동에 필요한 연산이 실행됨
    // PlayerController 또는 AIController 스크립트에 의해 제어될 것


    [System.Serializable]
    public class Wheel
    {
        public GameObject wheelGameObject; //휠 게임 오브젝트
        public WheelCollider wheelCollider; //휠 콜라이더
        public bool IsMoter; // 모터 힘 받는지
        public bool IsSteering; //스티어인지
        public float brakeBias = 0.5f; //브레이크 전달 크기
        public WheelHit hit;
        public bool isGrounded;
    }

    public float RigidSpeed
    {
        get { return playerRB.velocity.magnitude; }
    }

    //휠 리스트
    public List<Wheel> wheels;


    [Header("무게 및 리지드 바디")]
    // 플레이어의 Rigidbody 구성 요소
    private Rigidbody playerRB; //리지드 바디
    [SerializeField] GameObject centerofmassObject;
    [SerializeField] Vector3 centerofmass; //차 무게 중심 이 될 곳


    [Header("Handling")]
    //핸들링
    [Range(-540, 540)] public float steerValue;
    [Range(10, 90)] public float maxSteerAngle; //최대 각도
    public float sensitivity; //마우스 감도 
    public bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //스티어 앵글 각도 초기화 시간

    [Header("Engine")]
    public float accel;
    public Transmission transmission; // 수동 자동
    public WheelWork wheelWork; //전륜 등
    public float minRPM; //최소RPM
    public float maxRPM; //최소RPM
    public float currentRPM; //모터 RPM
    public float wheelRPM; //바퀴 RPM
    public bool reverse;//반대인지
    public float maxMotorTorque;  // 토크 커브의 피크에서의 토크
    public float maxBrakeTorque;// 최대 브레이크 토크
    public float finalDriveRatio; //최종 구동 비율 3~4 사이
    public float totalMotorTorque; //최종 모터 토크
    public AnimationCurve torqueCurve;//토크 커브
    public float RPMSmoothness;
    private float motorWheelNum; //모터 힘을 받는 바퀴 수

    [Header("Gear")]
    //기어
    public AnimationCurve gearRatiosCurve;//기어 비율
    public float[] gearRatiosArray;
    public float reverseRatio;
    public float shiftUp; //기어 올리는거
    public float shiftDown; //기어 내리는거
    private float lastShift = 0.0f; //마지막 기어 시간
    public float shiftDelay = 1.0f; //기어 바꾸는 시간
    private int targetGear = 1; //목표로 하는 기어
    private int lastGear = 1; //마지막 기어
    private bool shifting = false; //기어 변속중?
    private float shiftTime = 0.4f;// 기어 변속이 완료되는 시간 (보간됨)
    public float currentGear; //현재 기어


    [Header("Information")]

    public float KPH; // 속도

    [Header("Tire")]
    [SerializeField] bool LockWheel;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [Range(0f, 10f)] public float Friction;


    [Header("Environment Variable")]
    [Range(5, 20)] public float DownForceValue; //다운 포스
    public float downforce;
    public float airDragCoeff; // 공기저항 

    [Header("Effects")]
    [SerializeField] List<ParticleSystem> smoke;
    [SerializeField] GameObject smokePrefab;

    public bool initialized;

    private void OnEnable()
    {
        //InitializedSetting();
    }
    private void Update()
    {
        if (initialized)
        {
            AnimationUpdate();
            //ParticleEffect();
        }
    }
    private void FixedUpdate()
    {
        if (initialized)
        {
            addDownForce();
            ApplySteering();
            ApplyTorque();
            //friction();
            AdjustTraction();
            CheckingIsGrounded();//땅체크

            if (transmission == Transmission.Auto_Transmission)
            {
                AutoGear();
            }
            else if (transmission == Transmission.Manual_Transmission)
            {
                ManualGear();
            }

            Boosting();
        }
        //DriftControl();
    }



    public void InitializeSetting(int index, bool isAi)
    {
        
        //무게 중심 초기화
        //playerRB = gameObject.GetComponent<Rigidbody>();
        //playerRB.centerOfMass = centerofmassObject.transform.localPosition;
        //centerofmass = playerRB.centerOfMass;

        maxSteerAngle = DataManager.Instance.carDictionary[index].Handling.MaxSteerAngle;

        switch (DataManager.Instance.carDictionary[index].Engine.Transmission)
        {
            case "Manual":
                transmission = Transmission.Manual_Transmission;
                break;
            case "Auto":
                transmission = Transmission.Auto_Transmission;
                break;
        }
        if (isAi)
        {
            transmission = Transmission.Auto_Transmission;
        }

        switch (DataManager.Instance.carDictionary[index].Engine.WheelWork)
        {
            case "FRONT":
                wheelWork = WheelWork.FRONT;
                break;
            case "REAR":
                wheelWork = WheelWork.REAR;
                break;
            case "AWD":
                wheelWork = WheelWork.AWD;
                break;
        }

        minRPM = DataManager.Instance.carDictionary[index].Engine.MinRPM;
        maxRPM = DataManager.Instance.carDictionary[index].Engine.MaxRPM;
        maxMotorTorque = DataManager.Instance.carDictionary[index].Engine.MaxMotorTorque;
        maxBrakeTorque = DataManager.Instance.carDictionary[index].Engine.MaxBrakeTorque;
        RPMSmoothness = DataManager.Instance.carDictionary[index].Engine.RPMSmoothness;
        finalDriveRatio = DataManager.Instance.carDictionary[index].Gear.FinalGearRatio;
        gearRatiosArray = DataManager.Instance.carDictionary[index].Gear.GearRatios;
        reverseRatio = DataManager.Instance.carDictionary[index].Gear.ReverseRatio;
        shiftUp = DataManager.Instance.carDictionary[index].Gear.ShiftUp;
        shiftDown = DataManager.Instance.carDictionary[index].Gear.ShiftDown;
        shiftDelay = DataManager.Instance.carDictionary[index].Gear.ShiftDelay;
        DownForceValue = DataManager.Instance.carDictionary[index].Environment.DownForceValue;
        downforce = DataManager.Instance.carDictionary[index].Environment.DownForce;
        airDragCoeff = DataManager.Instance.carDictionary[index].Environment.AirDragCoeff;


    }

    public void InitializeIngame()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = centerofmassObject.transform.localPosition;
        centerofmass = playerRB.centerOfMass;
        ApplyMotorWork(); //전륜 후륜 정하기
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.ConfigureVehicleSubsteps(5.0f, 30, 10);
        }
        //모터 휠 카운트
        motorWheelNum = wheels.Where(a => a.IsMoter).Count();

        //기어비 커브로 만들어주기
        for (int i = 0; i < gearRatiosArray.Length; i++)
        {
            gearRatiosCurve.AddKey(i, gearRatiosArray[i]);
        }

        //for (int i = 0; i < wheels.Count; i++)
        //{
        //    smoke.Add(Instantiate(smokePrefab, wheels[i].wheelCollider.transform.position, Quaternion.identity, wheels[i].wheelCollider.transform).GetComponent<ParticleSystem>());
        //}
        initialized = true;
    }

    void ApplyMotorWork()
    {
        switch (wheelWork)
        {
            case WheelWork.AWD:
                foreach (var wheel in wheels)
                {
                    wheel.IsMoter = true;
                }
                break;
            case WheelWork.FRONT:
                foreach (var wheel in wheels)
                {
                    if (wheel.IsSteering)
                        wheel.IsMoter = true;
                }
                break;

            case WheelWork.REAR:
                foreach (var wheel in wheels)
                {
                    if (!wheel.IsSteering)
                        wheel.IsMoter = true;
                }
                break;
        }

    }
    public void CheckingIsGrounded()//땅체크
    {
        foreach (var wheel in wheels)
        {
            wheel.isGrounded = wheel.wheelCollider.GetGroundHit(out wheel.hit);
        }
    }


    private void addDownForce() //다운포스 값 계산하기
    {

        playerRB.angularDrag = (KPH > 100) ? KPH / 100 : 0;
        playerRB.drag = airDragCoeff + (KPH / 40000);

        KPH = playerRB.velocity.magnitude * 3.6f; //속도

        downforce = Mathf.Abs(DownForceValue * playerRB.velocity.magnitude); //다운포스값 계산
        downforce = KPH > 60 ? downforce : 0; //60 키로 이상이면 다운 포스 적용시키기
        playerRB.AddForce(-transform.up * downforce); //적용

    }


    [SerializeField] float slipAngle;
    [SerializeField] float steeringInput;
    [SerializeField] float speed;
    public void ApplySteering()//fixedUpdate
    {
        //float steeringAngle;
        // //스티어 앵글 계산


        //미끌어지는 각
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float steeringAngle = steerValue * maxSteerAngle / (1080.0f / 2);//steerSlider.value * maxSteerAngle / (1080.0f / 2);
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);



        //스티어링 바퀴에만 적용
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.steerAngle = wheel.IsSteering ? steeringAngle : 0; // 스티어링 값 적용
        }
    }

    //토크 적용
    private void ApplyTorque()//fixedUpdate
    {
        CalculateTorque();
        if (accel >= 0)
        {
            //motor
            float torquePerWheel = accel * (totalMotorTorque / motorWheelNum);
            foreach (var wheel in wheels)
            {
                if (wheel.IsMoter)
                {
                    if (wheel.isGrounded)
                        wheel.wheelCollider.motorTorque = torquePerWheel;
                    else
                        wheel.wheelCollider.motorTorque = 0f;
                }

                wheel.wheelCollider.brakeTorque = 0f;
            }
        }
        else if (accel <= -1 && wheelRPM <= 0)
        {
            float torquePerWheel = -torqueCurve.Evaluate(currentRPM / maxRPM) * reverseRatio * finalDriveRatio * maxMotorTorque;
            foreach (var wheel in wheels)
            {
                if (wheel.IsMoter)
                {
                    if (wheel.isGrounded)
                        wheel.wheelCollider.motorTorque = torquePerWheel;
                    else
                        wheel.wheelCollider.motorTorque = 0f;
                }

                wheel.wheelCollider.brakeTorque = 0f;
            }
        }
        else
        {
            //brakes 
            foreach (var wheel in wheels)
            {
                var brakeTorque = maxBrakeTorque * accel * -1 * wheel.brakeBias;
                wheel.wheelCollider.brakeTorque = brakeTorque;
                wheel.wheelCollider.motorTorque = 0f;
            }
        }

        // 핸드브레이크는 플레이어 차량 혹은 AI 차량에서 켜고 끄는 것을 제어해야 함
        if (handBrake) //핸드 브레이크
        {
            foreach (var wheel in wheels)
            {
                if (wheel.wheelCollider.name == "RR" || wheel.wheelCollider.name == "RL")
                {
                    // wheel.wheelCollider.brakeTorque = Mathf.Infinity;
                    //wheel.wheelCollider.motorTorque = 0f;

                    wheel.wheelCollider.brakeTorque = maxBrakeTorque;
                    wheel.wheelCollider.motorTorque = 0f;
                }
            }
        }
    }

    //토크 계산
    public void CalculateTorque()//fixedUpdate
    {
        // float gearRatio = gearRatiosCurve.Evaluate(currentGear); //이거 써보고 안되면 위에꺼로 써보기
        float gearRatio = gearRatiosArray[(int)currentGear]; //이거 써보고 안되면 위에꺼로 써보기
        WheelRPMCalculate(); //휠 RPM 계산
        //wheelRPM = wheelRPM < 0 ? 0 : wheelRPM; //휠 음수 가는거 막기
        currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * RPMSmoothness); //2있는 곳은 나중에 수치로 변환 시켜보기
        //currentRPM = minRPM + (wheelRPM * finalDriveRatio * gearRatio);
        if (currentRPM > maxRPM - 200)
        {
            currentRPM = maxRPM -UnityEngine.Random.Range(0, 200); //최대 RPM 제한
            totalMotorTorque = 0; //토크에 0을 줌으로써 일정 속도로 유지 되게
        }
        else
        {
            totalMotorTorque = torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio * finalDriveRatio * maxMotorTorque; // maxMotorTorque있는 곳은 나중에 tractionControlAdjustedMaxTorque생각해보기
        }
    }

    private void WheelRPMCalculate()
    {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            sum += wheels[i].wheelCollider.rpm;
            R++;
        }
        wheelRPM = (R != 0) ? sum / R : 0;

        if (wheelRPM < 0 && !reverse)
        {
            reverse = true;

        }
        else if (wheelRPM > 0 && reverse)
        {
            reverse = false;

        }
    }


    public float slipAllowance = 0.2f;
    public float driftTime;
    [Range(-90, 90)] public  float _driftAngle;

    void Boosting()
    {
        Vector3 driftValue = transform.InverseTransformVector(playerRB.velocity);
        _driftAngle = (Mathf.Atan2(driftValue.x, driftValue.z) * Mathf.Rad2Deg);

        if (Mathf.Abs(_driftAngle) > 10 && playerRB.velocity.magnitude > 5)
        {
            driftTime += Time.fixedDeltaTime;
        }

        if (driftTime > 0 && Mathf.Abs(_driftAngle) < 10) // 이전에 드리프트를 시작했었다면
        {
            if (GetComponent<CarAudio>() != null)
            {
                GetComponent<CarAudio>().TurboOn();

            }
            GetComponent<CinemachinController>().isTurbo = true;
            playerRB.AddForce(transform.forward * playerRB.velocity.magnitude * 2, ForceMode.Impulse);
            driftTime = 0;
        }
        else
        {
            GetComponent<CinemachinController>().isTurbo = false;
        }

    }

    private void AutoGear()
    {
        // 변속이 너무 빠르게 일어나지 않도록 지연 시간을 확인
        if (Time.time - lastShift > shiftDelay)
        {
            // 상승 변속
            if (currentRPM / maxRPM > shiftUp && Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
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
            else if (currentRPM / maxRPM < shiftDown && Mathf.RoundToInt(currentGear) > 0)
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
            currentGear = 0;
        }
    }


    //manual
    public bool upShift;
    public bool downShift;

    private void ManualGear()
    {
        // 변속이 너무 빠르게 일어나지 않도록 지연 시간을 확인
        if (Time.time - lastShift > shiftDelay)
        {
            // 상승 변속
            if (upShift)
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
                    upShift = false;
                }
            }
            // 하락 변속
            else if (downShift)
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
                    downShift = false;
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

    [SerializeField] float driftFactor;
    public float handBrakeFrictionMultiplier = 2f;
    private void AdjustTraction()
    {
        // 드리프트 상태로 전환하는 데 걸리는 시간
        float driftSmothFactor = .7f * Time.deltaTime;

        if (handBrake)
        {
            // 드리프트 중에는 바퀴의 마찰력을 조정하여 미끄러짐을 유도합니다.
            sidewaysFriction = wheels[0].wheelCollider.sidewaysFriction;
            forwardFriction = wheels[0].wheelCollider.forwardFriction;

            float velocity = 0;
            // 마찰력을 부드럽게 증가시킵니다.
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);

            for (int i = 0; i < 4; i++)
            {
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
            }

            // 전면 바퀴에 추가 그립을 제공하여 회전을 유지합니다.
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;
            for (int i = 0; i < 2; i++)
            {
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
            }
            // 드리프트 중에는 전진 방향으로 힘을 가합니다.
            playerRB.AddForce(transform.forward * (KPH / 400) * 40000);
        }
        else
        {
            // 핸드브레이크가 해제되었을 때, 일반적인 주행에서의 마찰력을 설정합니다.
            forwardFriction = wheels[0].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[0].wheelCollider.sidewaysFriction;

            // 주행 중에는 전방 마찰력을 증가시켜 슬립을 제어합니다.
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }

        // 슬립 정도를 체크하여 드리프트를 제어합니다.
        for (int i = 2; i < 4; i++)
        {
            WheelHit wheelHit;
            wheels[i].wheelCollider.GetGroundHit(out wheelHit);
            float x = 0;

            // 조향 슬라이더 값에 따라 x 값을 설정합니다.
            if (steerValue > 0)
            {
                x = 1;
            }
            else if (steerValue < 0)
            {
                x = -1;
            }
            // 슬립이 음수일 때는 우회전 중이므로 드리프트 팩터를 적용합니다.
            if (wheelHit.sidewaysSlip < 0) driftFactor = (1 + -x) * Mathf.Abs(wheelHit.sidewaysSlip);
            // 슬립이 양수일 때는 좌회전 중이므로 드리프트 팩터를 적용합니다.
            if (wheelHit.sidewaysSlip > 0) driftFactor = (1 + x) * Mathf.Abs(wheelHit.sidewaysSlip);
        }
    }

    private void friction() //마찰 계산
    {

        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheels.Count];
        for (int i = 0; i < wheels.Count; i++)
        {
            //앞바퀴
            if (wheels[i].wheelCollider.GetGroundHit(out hit) && i < 2)
            {


            }
            //뒷바퀴
            if (wheels[i].wheelCollider.GetGroundHit(out hit) && i >= 2)
            {
                forwardFriction = wheels[i].wheelCollider.forwardFriction;
                forwardFriction.stiffness = (handBrake) ? handBrakeSleepAmout : Mathf.Lerp(forwardFriction.stiffness, Friction, Time.fixedDeltaTime * 6);
                forwardFriction.extremumValue = (handBrake) ? 0.35f : Mathf.Lerp(forwardFriction.extremumValue, 2, Time.fixedDeltaTime * 3);
                wheels[i].wheelCollider.forwardFriction = forwardFriction;

                sidewaysFriction = wheels[i].wheelCollider.sidewaysFriction;
                sidewaysFriction.stiffness = (handBrake) ? handBrakeSleepAmout : Mathf.Lerp(forwardFriction.stiffness, Friction, Time.fixedDeltaTime * 6);
                sidewaysFriction.extremumValue = (handBrake) ? 0.35f : Mathf.Lerp(forwardFriction.extremumValue, 2, Time.fixedDeltaTime * 3);
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;

                sum += Mathf.Abs(hit.sidewaysSlip);
            }
            //wheelSlip[i] = Mathf.Abs(hit.forwardSlip) + Mathf.Abs(hit.sidewaysSlip);
            sidewaysSlip[i] = Mathf.Abs(hit.sidewaysSlip);
        }
        sum /= wheels.Count - 2;
    }

    //void ParticleEffect()           // Update()
    //{
    //    WheelHit[] wheelHits = new WheelHit[4];
    //    for (int i = 0; i < wheels.Count; i++)
    //    {
    //        wheels[i].wheelCollider.GetGroundHit(out wheelHits[i]);
    //        if (Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip) > slipAllowance)
    //        {
    //            //print(Mathf.Abs(wheelHits.sidewaysSlip) + Mathf.Abs(wheelHits.forwardSlip));
    //            smoke[i].Emit(1);
    //        }
    //    }
    //}


    [SerializeField] GameObject racingWheel;
    void AnimationUpdate()
    {
        foreach (var wheel in wheels)
        {
            Transform visualWheel = wheel.wheelGameObject.transform;

            Vector3 position;
            Quaternion rotation;
            wheel.wheelCollider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;

        }
        //racingWheel.transform.rotation = Quaternion.Euler(0, 0, steerSlider.value);
    }
}

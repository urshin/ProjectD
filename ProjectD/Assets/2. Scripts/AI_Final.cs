using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class AI_Final : MonoBehaviour
{
    #region
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



    //휠 리스트
    public List<Wheel> wheels;


    [Header("무게 및 리지드 바디")]
    // 플레이어의 Rigidbody 구성 요소
    private Rigidbody playerRB; //리지드 바디
    [SerializeField] GameObject centerofmassObject;
    [SerializeField] Vector3 centerofmass; //차 무게 중심 이 될 곳




    [Header("Handling")]
    //핸들링
    [Range(10, 90)] public float maxSteerAngle; //최대 각도
    public float sensitivity; //마우스 감도 
    [SerializeField] bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //스티어 앵글 각도 초기화 시간

    [Header("Engine")]
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


    [Header("Camera")]
    [SerializeField] Camera cam;


    [SerializeField] UnityEngine.UI.Slider steerSlider;

    //input
    private float mouseX;
    private float gasInput;
    #endregion

    private void OnEnable()
    {
        InitializedSetting();

    }
    private void Update()
    {
        GetInput();

        //AnimationUpdate();
    }
    private void FixedUpdate()
    {
        AIupdate();
        addDownForce();
        if (!LockWheel) Steering();
        ApplySteering();
        ApplyTorque();
        friction();
        CheckingIsGrounded();//땅체크

        if (transmission == Transmission.Auto_Transmission)
        {
            AutoGear();
        }




        //DriftControl();
    }
    [SerializeField] UnityEngine.UI.Slider gasslider;
    [Range(-540, 540)] public float autoHandle = 0;
    [SerializeField] Transform FrontMiddleRaySP;

    [SerializeField] float frontRaySensitive;
    [SerializeField] Transform leftFrontRaySP;
    [SerializeField] Transform rightFrontRaySP;
    Ray leftFront;
    Ray rightFront;
    [SerializeField] float currentRayLength;
    [SerializeField] float minFrontWallCheck;

    [SerializeField] float wallRaySensitive;
    [SerializeField] Transform leftWallRaySP;
    [SerializeField] Transform rightWallRaySP;



    [SerializeField] float gasRayLength;
    [SerializeField] AnimationCurve GasCurve;
    [SerializeField] AnimationCurve autoSteeringCurvel;


    [SerializeField] float brakeDistance = 5.0f; // 브레이크를 시작할 거리
    [SerializeField] float brakeSensitive;


    public void AIupdate()
    {
        gasslider.value = gasInput;

        AIGas();
        AISteering();
    }

    public void AIGas()
    {
        float gas = playerRB.velocity.magnitude * gasRayLength;
        //print(gas);
        RaycastHit leftHit;
        RaycastHit rightHit;
        leftFront = new Ray(leftFrontRaySP.position, leftFrontRaySP.forward);
        rightFront = new Ray(rightFrontRaySP.position, rightFrontRaySP.forward);
        Debug.DrawRay(leftFrontRaySP.position, leftFrontRaySP.forward * (gas + 2), Color.black);
        Debug.DrawRay(rightFrontRaySP.position, rightFrontRaySP.forward * (gas + 2), Color.black);
        float leftDistance = 0;
        float rightDistance = 0;
        if (Physics.Raycast(leftFront, out leftHit, (gas + 2)))
        {
            leftDistance = leftHit.distance;

        }
        if (Physics.Raycast(rightFront, out rightHit, (gas + 2)))
        {
            rightDistance = rightHit.distance;


        }

        Vector3 shortRayHitPoint;
        Vector3 longRayHitPoint;
        Transform frontRaySP;
        if (leftDistance < rightDistance)
        {
            shortRayHitPoint = leftFront.origin + leftFrontRaySP.forward * leftDistance;
            longRayHitPoint = rightFront.origin + rightFrontRaySP.forward * rightDistance;
            frontRaySP = leftFrontRaySP;
        }
        else if (rightDistance < leftDistance)
        {
            shortRayHitPoint = rightFront.origin + rightFrontRaySP.forward * rightDistance;
            longRayHitPoint = leftFront.origin + leftFrontRaySP.forward * leftDistance;
            frontRaySP = rightFrontRaySP;
        }
        else
        {
            shortRayHitPoint = Vector3.zero;
            longRayHitPoint = Vector3.zero;
            frontRaySP = leftFrontRaySP;
        }

        brakeDistance = playerRB.velocity.magnitude;
        RaycastHit brakeRayHit;

        Ray brakeRay = new Ray(frontRaySP.position, frontRaySP.forward);
        Debug.DrawRay(frontRaySP.position, frontRaySP.forward * (brakeDistance + brakeSensitive), Color.white);

        if (Physics.Raycast(brakeRay, out brakeRayHit))
        {
            // 벽까지의 남은 거리 계산
            float distanceToWall = brakeRayHit.distance;

            if (shortRayHitPoint != Vector3.zero || longRayHitPoint != Vector3.zero)
            {
                Vector3 directionVector = (longRayHitPoint - shortRayHitPoint).normalized;
                float angle = Vector3.Angle(directionVector, frontRaySP.forward);
                //  Debug.Log(GasCurve.Evaluate(angle));

                gasInput = GasCurve.Evaluate(angle);

            }
            // 브레이크 시작 거리보다 가까우면 브레이크 입력
            if (distanceToWall + brakeSensitive < brakeDistance && brakeDistance > 0)
            {
                gasInput = Mathf.Lerp(gasInput, -1, 0.4f);
            }
            else
            {

                float slopeAngle = (Vector3.SignedAngle(transform.forward.normalized, Vector3.up, transform.forward.normalized) - 90) * -1f;
                if (slopeAngle > 0f)
                {
                    gasInput = Mathf.Lerp(gasInput, 1f, slopeAngle / 5f);
                }
                else
                {

                gasInput = Mathf.Lerp(gasInput, 1, 0.1f);
                }
                //Debug.Log(slopeAngle);
            }


        }

        Debug.DrawLine(shortRayHitPoint, longRayHitPoint, Color.red);

    }


    public void AISteering()
    {

        currentRayLength = playerRB.velocity.magnitude * frontRaySensitive + minFrontWallCheck;
        RaycastHit leftFrontHit;
        RaycastHit rightFrontHit;


        Vector3 shortRayHitPoint;
        Vector3 longRayHitPoint;
        float leftFrontDis = 0;
        float rightFrontDis = 0;
        float tempFrontDis = 0;


        if (Physics.Raycast(leftFront, out leftFrontHit, currentRayLength))
        {
            leftFrontDis = leftFrontHit.distance;

        }
        else
        {
            leftFrontDis = 0;
        }

        if (Physics.Raycast(rightFront, out rightFrontHit, currentRayLength))
        {
            rightFrontDis = rightFrontHit.distance;
        }
        else
        {
            rightFrontDis = 0;
        }
        Debug.DrawRay(leftFrontRaySP.position, leftFrontRaySP.forward * currentRayLength, Color.blue);
        Debug.DrawRay(rightFrontRaySP.position, rightFrontRaySP.forward * currentRayLength, Color.blue);

        if (leftFrontDis != 0 || rightFrontDis!=0)
        {
            if (leftFrontDis > rightFrontDis)
            {
                if (Physics.Raycast(rightFront, out RaycastHit temphit))
                {
                    tempFrontDis = temphit.distance;
                    shortRayHitPoint = rightFront.origin + rightFrontRaySP.forward * tempFrontDis;
                    longRayHitPoint = leftFront.origin + leftFrontRaySP.forward * leftFrontDis;

                    // Calculate the vector from shortRayHitPoint to longRayHitPoint
                    Vector3 direction = (longRayHitPoint - shortRayHitPoint).normalized;

                    Debug.DrawRay(shortRayHitPoint, direction, Color.gray);
                    // Calculate the angle with respect to the ground
                    float angle = (Mathf.Acos(Vector3.Dot(rightFrontRaySP.right, direction)) * Mathf.Rad2Deg) - 90;
                    autoHandle = Mathf.Lerp(autoHandle, -1 * autoSteeringCurvel.Evaluate(Mathf.Abs(angle)), 0.08f);

                    //Debug.Log(angle);
                }
            }
            else if (rightFrontDis > leftFrontDis)
            {
                if (Physics.Raycast(leftFront, out RaycastHit temphit))
                {
                    tempFrontDis = temphit.distance;
                    shortRayHitPoint = leftFront.origin + leftFrontRaySP.forward * tempFrontDis;
                    longRayHitPoint = rightFront.origin + rightFrontRaySP.forward * rightFrontDis;

                    // Calculate the vector from shortRayHitPoint to longRayHitPoint
                    Vector3 direction = (longRayHitPoint - shortRayHitPoint).normalized;
                    Debug.DrawRay(shortRayHitPoint, direction, Color.gray);
                    // Calculate the angle with respect to the ground
                    float angle = (Mathf.Acos(Vector3.Dot(leftFrontRaySP.right, direction)) * Mathf.Rad2Deg) - 90;
                    autoHandle = Mathf.Lerp(autoHandle, autoSteeringCurvel.Evaluate(Mathf.Abs(angle)), 0.08f);

                    //Debug.Log(angle);
                }
            }
            else
            {
                return;
            }

        }
        
        else
        {

            Ray leftray = new Ray(leftWallRaySP.position, Quaternion.Euler(0, -45, 0) * leftWallRaySP.forward);
            Ray rightray = new Ray(rightWallRaySP.position, Quaternion.Euler(0, 45, 0) * rightWallRaySP.forward);

            RaycastHit leftwallhit;
            RaycastHit rightwallhit;

            float rayDis = wallRaySensitive;

            if (Physics.Raycast(leftray, out leftwallhit, rayDis))
            {
                if (leftwallhit.distance > 0)
                {

                    autoHandle = Mathf.Lerp(autoHandle, 180, 0.04f);
                    //print("왼");
                }
            }


            if (Physics.Raycast(rightray, out rightwallhit, rayDis))
            {
                if (rightwallhit.distance > 0)
                {
                    autoHandle = Mathf.Lerp(autoHandle, -180, 0.04f);
                    // print("오른");
                }
            }
            autoHandle = Mathf.Lerp(autoHandle, 0, 0.1f);

            // 레이를 시각적으로 보여주기 위한 디버깅용 코드
            Debug.DrawRay(leftWallRaySP.position, Quaternion.Euler(0, -45, 0) * leftWallRaySP.forward * (rayDis), Color.red);
            Debug.DrawRay(rightWallRaySP.position, Quaternion.Euler(0, 45, 0) * rightWallRaySP.forward * (rayDis), Color.red);
        }

        //float leftWallDistance = 0;
        //float rightWallDistance = 0;


        //if (Physics.Raycast(leftWallRaySP.position, Quaternion.Euler(0, -45, 0) * leftWallRaySP.forward, out RaycastHit leftInfo))
        //{

        //    leftWallDistance = leftInfo.distance;


        //}
        //else
        //{
        //    leftWallDistance = 80;
        //}

        //if (Physics.Raycast(rightWallRaySP.position, Quaternion.Euler(0, 45, 0) * rightWallRaySP.forward, out RaycastHit rightInfo))
        //{

        //    rightWallDistance = rightInfo.distance;

        //}
        //else
        //{
        //    rightWallDistance = 80;
        //}
        //if ((rightWallDistance - leftWallDistance > 0))
        //{
        //    autoHandle = Mathf.Lerp(autoHandle, rightWallDistance - leftWallDistance, 0.1f);
        //}
        //else if ((rightWallDistance - leftWallDistance < 0))
        //{
        //    autoHandle = Mathf.Lerp(autoHandle, rightWallDistance - leftWallDistance, 0.1f);
        //}
        //else
        //{
        //    autoHandle = Mathf.Lerp(autoHandle, rightWallDistance - leftWallDistance, 0.3f);
        //}
        //Debug.DrawRay(leftWallRaySP.position, Quaternion.Euler(0, -45, 0) * leftWallRaySP.forward * leftWallDistance, Color.blue);
        //Debug.DrawRay(rightWallRaySP.position, Quaternion.Euler(0, 45, 0) * rightWallRaySP.forward * rightWallDistance, Color.blue);


    }



    public void GetInput() //인풋 값 받기
    {
        // mouseX = Input.GetAxis("Mouse X");
        // gasInput = Input.GetAxis("Vertical");
        steerSlider.value = autoHandle;

    }

    void InitializedSetting()
    {





        //무게 중심 초기화
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

        cam = Camera.main;
        cam.GetComponent<CamController_June>().player = gameObject.transform;
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

    //스티어링 계산
    public void Steering()//fixedUpdate
    {
        //if (Input.GetMouseButton(0))
        //{
        //    if (mouseX != 0)
        //    {
        //        steerSlider.value += mouseX * sensitivity;
        //    }

        //}
        //else
        //{
        //    //steerSlider.value = Mathf.Lerp(steerSlider.value, 0, Time.fixedDeltaTime * resetSteerAngleSpeed);
        //}
    }
    [SerializeField] float slipAngle;
    [SerializeField] float steeringInput;
    [SerializeField] float speed;
    public void ApplySteering()//fixedUpdate
    {


        //미끌어지는 각
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float steeringAngle = autoHandle * maxSteerAngle / (1080.0f / 2);
        // if (slipAngle < 120f)
        // {
        //     steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        // }
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
        if (gasInput >= 0)
        {
            //motor
            float torquePerWheel = gasInput * (totalMotorTorque / motorWheelNum);
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
        else if (gasInput <= -0.5f && wheelRPM <= 0)
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
                var brakeTorque = maxBrakeTorque * gasInput * -1 * wheel.brakeBias;
                wheel.wheelCollider.brakeTorque = brakeTorque;
                wheel.wheelCollider.motorTorque = 0f;
            }
        }
        //Debug.Log(gasInput);


        if (Input.GetKey(KeyCode.Space)) //핸드 브레이크
        {
            handBrake = true;
            foreach (var wheel in wheels)
            {
                if (wheel.wheelCollider.name == "RR" || wheel.wheelCollider.name == "RL")
                {
                    wheel.wheelCollider.brakeTorque = Mathf.Infinity;
                    //wheel.wheelCollider.motorTorque = 0f;
                }
            }
        }
        else
        {
            handBrake = false;
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
            currentRPM = maxRPM - Random.Range(0, 200); //최대 RPM 제한
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
            else if (currentRPM / maxRPM < shiftDown && Mathf.RoundToInt(currentGear) > 1)
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

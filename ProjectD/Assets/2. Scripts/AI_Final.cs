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
        public GameObject wheelGameObject; //�� ���� ������Ʈ
        public WheelCollider wheelCollider; //�� �ݶ��̴�
        public bool IsMoter; // ���� �� �޴���
        public bool IsSteering; //��Ƽ������
        public float brakeBias = 0.5f; //�극��ũ ���� ũ��
        public WheelHit hit;
        public bool isGrounded;
    }



    //�� ����Ʈ
    public List<Wheel> wheels;


    [Header("���� �� ������ �ٵ�")]
    // �÷��̾��� Rigidbody ���� ���
    private Rigidbody playerRB; //������ �ٵ�
    [SerializeField] GameObject centerofmassObject;
    [SerializeField] Vector3 centerofmass; //�� ���� �߽� �� �� ��




    [Header("Handling")]
    //�ڵ鸵
    [Range(10, 90)] public float maxSteerAngle; //�ִ� ����
    public float sensitivity; //���콺 ���� 
    [SerializeField] bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //��Ƽ�� �ޱ� ���� �ʱ�ȭ �ð�

    [Header("Engine")]
    public Transmission transmission; // ���� �ڵ�
    public WheelWork wheelWork; //���� ��
    public float minRPM; //�ּ�RPM
    public float maxRPM; //�ּ�RPM
    public float currentRPM; //���� RPM
    public float wheelRPM; //���� RPM
    public bool reverse;//�ݴ�����
    public float maxMotorTorque;  // ��ũ Ŀ���� ��ũ������ ��ũ
    public float maxBrakeTorque;// �ִ� �극��ũ ��ũ
    public float finalDriveRatio; //���� ���� ���� 3~4 ����
    public float totalMotorTorque; //���� ���� ��ũ
    public AnimationCurve torqueCurve;//��ũ Ŀ��
    public float RPMSmoothness;
    private float motorWheelNum; //���� ���� �޴� ���� ��

    [Header("Gear")]
    //���
    public AnimationCurve gearRatiosCurve;//��� ����
    public float[] gearRatiosArray;
    public float reverseRatio;
    public float shiftUp; //��� �ø��°�
    public float shiftDown; //��� �����°�
    private float lastShift = 0.0f; //������ ��� �ð�
    public float shiftDelay = 1.0f; //��� �ٲٴ� �ð�
    private int targetGear = 1; //��ǥ�� �ϴ� ���
    private int lastGear = 1; //������ ���
    private bool shifting = false; //��� ������?
    private float shiftTime = 0.4f;// ��� ������ �Ϸ�Ǵ� �ð� (������)
    public float currentGear; //���� ���


    [Header("Information")]

    public float KPH; // �ӵ�

    [Header("Tire")]
    [SerializeField] bool LockWheel;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    [Range(0f, 10f)] public float Friction;


    [Header("Environment Variable")]
    [Range(5, 20)] public float DownForceValue; //�ٿ� ����
    public float downforce;
    public float airDragCoeff; // �������� 


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
        CheckingIsGrounded();//��üũ

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


    [SerializeField] float brakeDistance = 5.0f; // �극��ũ�� ������ �Ÿ�
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
            // �������� ���� �Ÿ� ���
            float distanceToWall = brakeRayHit.distance;

            if (shortRayHitPoint != Vector3.zero || longRayHitPoint != Vector3.zero)
            {
                Vector3 directionVector = (longRayHitPoint - shortRayHitPoint).normalized;
                float angle = Vector3.Angle(directionVector, frontRaySP.forward);
                //  Debug.Log(GasCurve.Evaluate(angle));

                gasInput = GasCurve.Evaluate(angle);

            }
            // �극��ũ ���� �Ÿ����� ������ �극��ũ �Է�
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
                    //print("��");
                }
            }


            if (Physics.Raycast(rightray, out rightwallhit, rayDis))
            {
                if (rightwallhit.distance > 0)
                {
                    autoHandle = Mathf.Lerp(autoHandle, -180, 0.04f);
                    // print("����");
                }
            }
            autoHandle = Mathf.Lerp(autoHandle, 0, 0.1f);

            // ���̸� �ð������� �����ֱ� ���� ������ �ڵ�
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



    public void GetInput() //��ǲ �� �ޱ�
    {
        // mouseX = Input.GetAxis("Mouse X");
        // gasInput = Input.GetAxis("Vertical");
        steerSlider.value = autoHandle;

    }

    void InitializedSetting()
    {





        //���� �߽� �ʱ�ȭ
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = centerofmassObject.transform.localPosition;
        centerofmass = playerRB.centerOfMass;

        ApplyMotorWork(); //���� �ķ� ���ϱ�
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.ConfigureVehicleSubsteps(5.0f, 30, 10);
        }
        //���� �� ī��Ʈ
        motorWheelNum = wheels.Where(a => a.IsMoter).Count();

        //���� Ŀ��� ������ֱ�
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
    public void CheckingIsGrounded()//��üũ
    {
        foreach (var wheel in wheels)
        {
            wheel.isGrounded = wheel.wheelCollider.GetGroundHit(out wheel.hit);
        }
    }


    private void addDownForce() //�ٿ����� �� ����ϱ�
    {

        playerRB.angularDrag = (KPH > 100) ? KPH / 100 : 0;
        playerRB.drag = airDragCoeff + (KPH / 40000);

        KPH = playerRB.velocity.magnitude * 3.6f; //�ӵ�

        downforce = Mathf.Abs(DownForceValue * playerRB.velocity.magnitude); //�ٿ������� ���
        downforce = KPH > 60 ? downforce : 0; //60 Ű�� �̻��̸� �ٿ� ���� �����Ű��
        playerRB.AddForce(-transform.up * downforce); //����

    }

    //��Ƽ� ���
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


        //�̲������� ��
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float steeringAngle = autoHandle * maxSteerAngle / (1080.0f / 2);
        // if (slipAngle < 120f)
        // {
        //     steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        // }
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);



        //��Ƽ� �������� ����
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.steerAngle = wheel.IsSteering ? steeringAngle : 0; // ��Ƽ� �� ����
        }
    }

    //��ũ ����
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


        if (Input.GetKey(KeyCode.Space)) //�ڵ� �극��ũ
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

    //��ũ ���
    public void CalculateTorque()//fixedUpdate
    {
        // float gearRatio = gearRatiosCurve.Evaluate(currentGear); //�̰� �Ẹ�� �ȵǸ� �������� �Ẹ��
        float gearRatio = gearRatiosArray[(int)currentGear]; //�̰� �Ẹ�� �ȵǸ� �������� �Ẹ��
        WheelRPMCalculate(); //�� RPM ���
        //wheelRPM = wheelRPM < 0 ? 0 : wheelRPM; //�� ���� ���°� ����
        currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * RPMSmoothness); //2�ִ� ���� ���߿� ��ġ�� ��ȯ ���Ѻ���
        //currentRPM = minRPM + (wheelRPM * finalDriveRatio * gearRatio);
        if (currentRPM > maxRPM - 200)
        {
            currentRPM = maxRPM - Random.Range(0, 200); //�ִ� RPM ����
            totalMotorTorque = 0; //��ũ�� 0�� �����ν� ���� �ӵ��� ���� �ǰ�
        }
        else
        {

            totalMotorTorque = torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio * finalDriveRatio * maxMotorTorque; // maxMotorTorque�ִ� ���� ���߿� tractionControlAdjustedMaxTorque�����غ���

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
        // ������ �ʹ� ������ �Ͼ�� �ʵ��� ���� �ð��� Ȯ��
        if (Time.time - lastShift > shiftDelay)
        {
            // ��� ����
            if (currentRPM / maxRPM > shiftUp && Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
            {
                // 1���� �ܼ��� ȸ�� ���� ��� ��� �������� ����
                // �Ǵ� 1���� 15km/h �̻� �̵� ���� ��쿡�� ��� �����մϴ�.
                if (Mathf.RoundToInt(currentGear) > 1 || KPH > 15f)
                {
                    // ������ ���� �ð��� ����մϴ�.
                    lastGear = Mathf.RoundToInt(currentGear);
                    // ��� ������ �� �����մϴ�.
                    targetGear = lastGear + 1;
                    // ������ ���� �ð��� ����ϰ�, ���� ������ ǥ���մϴ�.
                    lastShift = Time.time;
                    shifting = true;
                }
            }
            // �϶� ����
            else if (currentRPM / maxRPM < shiftDown && Mathf.RoundToInt(currentGear) > 1)
            {
                // ������ ���� �ð��� ����մϴ�.
                lastGear = Mathf.RoundToInt(currentGear);
                // �϶� ������ �� �����մϴ�.
                targetGear = lastGear - 1;
                // ������ ���� �ð��� ����ϰ�, ���� ������ ǥ���մϴ�.
                lastShift = Time.time;
                shifting = true;
            }
        }

        // ���� ���� ���
        if (shifting)
        {
            // �ð��� ���� �������� ����Ͽ� ���� �� �����մϴ�.
            float lerpVal = (Time.time - lastShift) / shiftTime;
            currentGear = Mathf.Lerp(lastGear, targetGear, lerpVal);
            // ������ �Ϸ�Ǹ� ���� �� ���¸� �����մϴ�.
            if (lerpVal >= 1f)
                shifting = false;
        }

        // ��� ������ ����� �ʵ��� Ŭ�����մϴ�.
        if (currentGear >= gearRatiosArray.Length)
        {
            currentGear = gearRatiosArray.Length;
        }
        else if (currentGear < 1)
        {
            currentGear = 1;
        }
    }


    private void friction() //���� ���
    {

        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheels.Count];
        for (int i = 0; i < wheels.Count; i++)
        {
            //�չ���
            if (wheels[i].wheelCollider.GetGroundHit(out hit) && i < 2)
            {


            }
            //�޹���
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

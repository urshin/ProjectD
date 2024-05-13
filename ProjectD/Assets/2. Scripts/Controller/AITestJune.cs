using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AITestJune : MonoBehaviour
{
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


    [Header("About UI")]
    // ��Ƽ� �����̴�
    public UnityEngine.UI.Slider steerSlider;


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




    //input
    private float mouseX;
    private float gasInput;



    private void OnEnable()
    {
        InitializedSetting();

    }
    private void Update()
    {
        GetInput();

        //AnimationUpdate();
    }



    [Range(-540, 540)] public float autoHandle = 0;
    [SerializeField] UnityEngine.UI.Slider gasslider;




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

    [SerializeField] float frontDetectedSensitive; // �ӷ¿� ���� �� �Ÿ� ����
    [SerializeField] float frontDetectedMinSensitive; // �ּ� ����
    [SerializeField] float steeringDetectedSensitive; // �ӷ¿� ���� �� �Ÿ� ����
    [SerializeField] float brakeSensitive;
    [SerializeField] float brakeMinDistance;

    //�� ���� ���� ����
    [SerializeField] Transform frontMiddleRaySP;
    [SerializeField] Transform frontLeftRaySP;
    [SerializeField] Transform frontRightleRaySP;
    [SerializeField] Transform wallLeftRaySP;
    [SerializeField] Transform WallRightRaySP;

    [SerializeField] AnimationCurve AISteerCurve;



    public void AIupdate()
    {
        AIGas();
        AISteering();
        //AIWallCheck();
    }
    [Range(-540, 540)] public float wheelAngle = 0;
    public void AIGas() //�Ķ���
    {

        float slopeAngle = (Vector3.SignedAngle(transform.forward.normalized, Vector3.up, transform.forward.normalized) - 90) * -1f;
        RaycastHit frontHit;

        float frontDistance = 0;

        Ray front = new Ray(frontMiddleRaySP.position, Quaternion.Euler(0, wheelAngle, 0) *frontMiddleRaySP.forward);


        Debug.DrawRay(frontMiddleRaySP.position - new Vector3(0, 1, 0), Quaternion.Euler(0, wheelAngle, 0) * frontMiddleRaySP.forward*1000, Color.blue);
        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.

        if (Physics.Raycast(front, out frontHit))
        {
            frontDistance = frontHit.distance;
        }
        float angle = 0;

        // ���� ����� ���� ���� ���͸� ����մϴ�.
        Vector3 wallNormal = frontHit.normal;
        Vector3 horizontalNormal = new Vector3(wallNormal.x, 0, wallNormal.z).normalized;


        // ���� ���� ���Ϳ� ���� ���� ���� ���� ������ ������ ����մϴ�.
        angle = Vector3.Angle(horizontalNormal, frontMiddleRaySP.forward);
      
        if (frontDistance*brakeSensitive +brakeMinDistance < playerRB.velocity.magnitude)
        {
            gasInput = Mathf.Lerp(gasInput, -1, 0.06f);
        }
        else if(frontDistance  < playerRB.velocity.magnitude)
        {
            gasInput = Mathf.Lerp(gasInput, 0, 0.1f);
        }    
        else
        {
            gasInput = Mathf.Lerp(gasInput,1,0.1f);
        }
    }


    public void AISteering()
    {

        RaycastHit leftHit;
        RaycastHit rightHit;
        float leftDistance = 0;
        float rightDistance = 0;
        Ray leftFront = new Ray(frontLeftRaySP.position, frontLeftRaySP.forward);
        Ray rightFront = new Ray(frontRightleRaySP.position, frontRightleRaySP.forward);
        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.
        if (Physics.Raycast(leftFront, out leftHit, (playerRB.velocity.magnitude*frontDetectedSensitive) + steeringDetectedSensitive))
        {
            leftDistance = leftHit.distance;
        }
        if (Physics.Raycast(rightFront, out rightHit, (playerRB.velocity.magnitude * frontDetectedSensitive) + steeringDetectedSensitive))
        {
            rightDistance = rightHit.distance;
        }



        RaycastHit frontHit;

        float frontDistance = 0;

        Ray front = new Ray(frontMiddleRaySP.position, frontMiddleRaySP.forward);
        if (Physics.Raycast(front, out frontHit, (playerRB.velocity.magnitude * frontDetectedSensitive) + steeringDetectedSensitive+2))
        {
            frontDistance = frontHit.distance;
        }
        else
        {
            AIWallCheck();
        }
        float angle = 0;

        // ���� ����� ���� ���� ���͸� ����մϴ�.
        Vector3 wallNormal = frontHit.normal;
        Vector3 horizontalNormal = new Vector3(wallNormal.x, 0, wallNormal.z).normalized;
        angle = Vector3.Angle(horizontalNormal, frontMiddleRaySP.forward);
        if (leftDistance != rightDistance)
        {
            float steerAmount = leftDistance < rightDistance ? AISteerCurve.Evaluate(angle) : -AISteerCurve.Evaluate(angle);
            autoHandle = Mathf.Lerp(autoHandle, steerAmount, 0.08f);
        }
        else
        {
            AIWallCheck();
        }

        
    }
    public void AIWallCheck() //���
    {
        float wallAngle =20;
        RaycastHit leftHit;
        RaycastHit rightHit;
        float leftDistance = 0;
        float rightDistance = 0;
        Ray leftFront = new Ray(wallLeftRaySP.position, Quaternion.Euler(0, -wallAngle + wheelAngle, 0) * wallLeftRaySP.forward);
        Ray rightFront = new Ray(WallRightRaySP.position, Quaternion.Euler(0, wallAngle + wheelAngle, 0) * WallRightRaySP.forward);
        Debug.DrawRay(wallLeftRaySP.position, Quaternion.Euler(0, -wallAngle + wheelAngle, 0) * wallLeftRaySP.forward*100, Color.white);
        Debug.DrawRay(WallRightRaySP.position, Quaternion.Euler(0, wallAngle + wheelAngle, 0) * wallLeftRaySP.forward * 100, Color.white);

        // ���ʰ� ������ ���̸� ��� �浹�� �����մϴ�.
        if (Physics.Raycast(leftFront, out leftHit))
        {
            leftDistance = leftHit.distance;
        }
        if (Physics.Raycast(rightFront, out rightHit))
        {
            rightDistance = rightHit.distance;
        }

        if(leftDistance> rightDistance)
        {
            autoHandle = Mathf.Lerp(autoHandle, -180, 0.08f);

        }
        else if(rightDistance> leftDistance)
        {
            autoHandle = Mathf.Lerp(autoHandle, 180, 0.08f);
        }
        else
        {
            autoHandle = Mathf.Lerp(autoHandle, 0, 0.08f);
        }

    }

    public void GetInput() //��ǲ �� �ޱ�
    {

        steerSlider.value = autoHandle;
        gasslider.value = gasInput;

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
        if (Input.GetMouseButton(0))
        {
            if (mouseX != 0)
            {
                steerSlider.value += mouseX * sensitivity;
            }

        }
        else
        {
            //steerSlider.value = Mathf.Lerp(steerSlider.value, 0, Time.fixedDeltaTime * resetSteerAngleSpeed);
        }
    }
    [SerializeField] float slipAngle;
    [SerializeField] float steeringInput;
    [SerializeField] float speed;
    public void ApplySteering()//fixedUpdate
    {
        //float steeringAngle;
        // //��Ƽ�� �ޱ� ���


        //�̲������� ��
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float steeringAngle = autoHandle * maxSteerAngle / (1080.0f / 2);
        // if (slipAngle < 120f)
        // {
        //     steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        // }
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);

        wheelAngle = autoHandle * maxSteerAngle / (1080.0f / 2);

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

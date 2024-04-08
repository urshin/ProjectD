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
    // WheelColliders Ŭ���� ����
    [System.Serializable]
    public class Pair
    {
        public WheelCollider left; // ���� ���� �ݶ��̴�
        public WheelCollider right; // ������ ���� �ݶ��̴�
        public bool IsMoterPower; // ���� �� �޴���
        public bool IsSteering; //��Ƽ�� �� �޴���
        public float brakeBias = 0.5f; //�극��ũ ���� ũ��
        public float handbrakeBias = 5f; //�ڵ�극��ũ ���� ũ��
        [System.NonSerialized] //public�̶� �ν����� â���� ������ ����
        public WheelHit hitLeft; // ���� �� ��Ʈ
        [System.NonSerialized]
        public WheelHit hitRight; // ������ �� ��Ʈ
        //[System.NonSerialized]
        public bool isGroundedLeft = false; // ������ ���� �پ��ִ��� ����
        //[System.NonSerialized]
        public bool isGroundedRight = false; // �������� ���� �پ��ִ��� ����
    }

    public List<Pair> pair;





    public enum WheelWork //��,��,4��
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
    // �÷��̾��� Rigidbody ���� ���
    private Rigidbody playerRB;
    public Vector3 _centerOfMass;



    // ��Ƽ� �����̴�
    public Slider steerSlider;



    // �ڵ��� �̵��� ���� �Ű�����

    //�ڵ鸵
    public float maxSteerAngle;
    public float sensitivity;

    //����
    public Transmission transmission;
    public WheelWork wheelWork;
    public float minRPM; //�ּ�RPM
    public float maxRPM; //�ּ�RPM
    public float currentRPM; //���� RPM
    public float wheelRPM; //���� RPM
    public float finalDriveRatio; //���� ���� ���� 3~4 ����
    public float maxMotorTorque;  // ��ũ Ŀ���� ��ũ������ ��ũ
    public float maxBrakeTorque;// �ִ� �극��ũ ��ũ
    public float totalMotorTorque; //���� ���� ��ũ
    public AnimationCurve gearRatiosCurve;//��� ����
    public float[] gearRatiosArray;
    public AnimationCurve torqueCurve;//��ũ Ŀ��


    //���
    public AnimationCurve shiftUpCurve; //��� �ø��°�
    public AnimationCurve shiftDownCurve; //��� �����°�
    private float lastShift = 0.0f; //������ ��� �ð�
    public float shiftDelay = 1.0f; //��� �ٲٴ� �ð�
    private int targetGear = 1; //��ǥ�� �ϴ� ���
    private int lastGear = 1; //������ ���
    private bool shifting = false; //��� ������?
    public float shiftTime = 0.4f;// ��� ������ �Ϸ�Ǵ� �ð� (������)

    //info
    public float motorWheelNum; //���� ���� �޴� ���� ��
    public float currentGear; //���� ���
    public float KPH; // �ӵ�
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

    //�ʱ�ȭ(����, ��ũ ��) �� ui��
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
    //��Ƽ� ����
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
        //��Ƽ�� �ޱ� ���
        steeringAngle = steerSlider.value * maxSteerAngle / (1080.0f / 2);

        //��Ƽ� �������� ����
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

    //��ũ ����
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
        float gearRatio = gearRatiosCurve.Evaluate(currentGear); //�̰� �Ẹ�� �ȵǸ� �������� �Ẹ��
        wheelRPM = (pair[1].right.rpm + pair[1].left.rpm) / 2f * gearRatio; //���� RPM //���߿� Rear�� �� �� ����غ���
        if (wheelRPM < 0)
            wheelRPM = 0;
        currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * 2); //2�ִ� ���� ���߿� ��ġ�� ��ȯ ���Ѻ���

        if (currentRPM > maxRPM)
        {
            currentRPM = maxRPM; //�ִ� RPM ����
            totalMotorTorque = 0; //��ũ�� 0�� �����ν� ���� �ӵ��� ���� �ǰ�
        }
        else
        {

            totalMotorTorque = torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio * finalDriveRatio * maxMotorTorque; // maxMotorTorque�ִ� ���� ���߿� tractionControlAdjustedMaxTorque�����غ���
        }


    }

    public void CheckingIsGrounded()//��üũ
    {
        foreach (var wheel in pair)
        {
            wheel.isGroundedLeft = wheel.left.GetGroundHit(out wheel.hitLeft);
            wheel.isGroundedRight = wheel.right.GetGroundHit(out wheel.hitRight);
        }
    }



    #endregion


    //��� ��ȯ
    //auto
    private void AutoGear()
    {
        // ������ �ʹ� ������ �Ͼ�� �ʵ��� ���� �ð��� Ȯ��
        if (Time.time - lastShift > shiftDelay)
        {
            // ��� ����
            if (currentRPM / maxRPM > shiftUpCurve.Evaluate(gasInput) && Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
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
            else if (currentRPM / maxRPM < shiftDownCurve.Evaluate(gasInput) && Mathf.RoundToInt(currentGear) > 1)
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
    //manual
    private void ManualGear()
    {
        // ������ �ʹ� ������ �Ͼ�� �ʵ��� ���� �ð��� Ȯ��
        if (Time.time - lastShift > shiftDelay)
        {
            // ��� ����
            if (Input.GetKey(KeyCode.E))
            {
                // 1���� �ܼ��� ȸ�� ���� ��� ��� �������� ����
                // �Ǵ� 1���� 15km/h �̻� �̵� ���� ��쿡�� ��� �����մϴ�.
                if (Mathf.RoundToInt(currentGear) < gearRatiosArray.Length)
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
            else if (Input.GetKey(KeyCode.Q))
            {
                if (Mathf.RoundToInt(currentGear) > 1)
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
            currentGear = gearRatiosArray.Length - 1;
        }
        else if (currentGear < 1)
        {
            currentGear = 1;
        }
    }



}

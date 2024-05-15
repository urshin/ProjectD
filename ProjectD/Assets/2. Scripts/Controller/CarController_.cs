using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
public class CarController_ : MonoBehaviour
{
    // ���� �����տ� �ɾ��� ��ũ��Ʈ
    // ������ �ŵ��� �ʿ��� ������ �����
    // PlayerController �Ǵ� AIController ��ũ��Ʈ�� ���� ����� ��


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

    public float RigidSpeed
    {
        get { return playerRB.velocity.magnitude; }
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
    [Range(-540, 540)] public float steerValue;
    [Range(10, 90)] public float maxSteerAngle; //�ִ� ����
    public float sensitivity; //���콺 ���� 
    public bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //��Ƽ�� �ޱ� ���� �ʱ�ȭ �ð�

    [Header("Engine")]
    public float accel;
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
            CheckingIsGrounded();//��üũ

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
        
        //���� �߽� �ʱ�ȭ
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


    [SerializeField] float slipAngle;
    [SerializeField] float steeringInput;
    [SerializeField] float speed;
    public void ApplySteering()//fixedUpdate
    {
        //float steeringAngle;
        // //��Ƽ�� �ޱ� ���


        //�̲������� ��
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float steeringAngle = steerValue * maxSteerAngle / (1080.0f / 2);//steerSlider.value * maxSteerAngle / (1080.0f / 2);
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        }
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

        // �ڵ�극��ũ�� �÷��̾� ���� Ȥ�� AI �������� �Ѱ� ���� ���� �����ؾ� ��
        if (handBrake) //�ڵ� �극��ũ
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
            currentRPM = maxRPM -UnityEngine.Random.Range(0, 200); //�ִ� RPM ����
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

        if (driftTime > 0 && Mathf.Abs(_driftAngle) < 10) // ������ �帮��Ʈ�� �����߾��ٸ�
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
            else if (currentRPM / maxRPM < shiftDown && Mathf.RoundToInt(currentGear) > 0)
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
            currentGear = 0;
        }
    }


    //manual
    public bool upShift;
    public bool downShift;

    private void ManualGear()
    {
        // ������ �ʹ� ������ �Ͼ�� �ʵ��� ���� �ð��� Ȯ��
        if (Time.time - lastShift > shiftDelay)
        {
            // ��� ����
            if (upShift)
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
                    upShift = false;
                }
            }
            // �϶� ����
            else if (downShift)
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
                    downShift = false;
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

    [SerializeField] float driftFactor;
    public float handBrakeFrictionMultiplier = 2f;
    private void AdjustTraction()
    {
        // �帮��Ʈ ���·� ��ȯ�ϴ� �� �ɸ��� �ð�
        float driftSmothFactor = .7f * Time.deltaTime;

        if (handBrake)
        {
            // �帮��Ʈ �߿��� ������ �������� �����Ͽ� �̲������� �����մϴ�.
            sidewaysFriction = wheels[0].wheelCollider.sidewaysFriction;
            forwardFriction = wheels[0].wheelCollider.forwardFriction;

            float velocity = 0;
            // �������� �ε巴�� ������ŵ�ϴ�.
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);

            for (int i = 0; i < 4; i++)
            {
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
            }

            // ���� ������ �߰� �׸��� �����Ͽ� ȸ���� �����մϴ�.
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.1f;
            for (int i = 0; i < 2; i++)
            {
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
            }
            // �帮��Ʈ �߿��� ���� �������� ���� ���մϴ�.
            playerRB.AddForce(transform.forward * (KPH / 400) * 40000);
        }
        else
        {
            // �ڵ�극��ũ�� �����Ǿ��� ��, �Ϲ����� ���࿡���� �������� �����մϴ�.
            forwardFriction = wheels[0].wheelCollider.forwardFriction;
            sidewaysFriction = wheels[0].wheelCollider.sidewaysFriction;

            // ���� �߿��� ���� �������� �������� ������ �����մϴ�.
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < 4; i++)
            {
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
            }
        }

        // ���� ������ üũ�Ͽ� �帮��Ʈ�� �����մϴ�.
        for (int i = 2; i < 4; i++)
        {
            WheelHit wheelHit;
            wheels[i].wheelCollider.GetGroundHit(out wheelHit);
            float x = 0;

            // ���� �����̴� ���� ���� x ���� �����մϴ�.
            if (steerValue > 0)
            {
                x = 1;
            }
            else if (steerValue < 0)
            {
                x = -1;
            }
            // ������ ������ ���� ��ȸ�� ���̹Ƿ� �帮��Ʈ ���͸� �����մϴ�.
            if (wheelHit.sidewaysSlip < 0) driftFactor = (1 + -x) * Mathf.Abs(wheelHit.sidewaysSlip);
            // ������ ����� ���� ��ȸ�� ���̹Ƿ� �帮��Ʈ ���͸� �����մϴ�.
            if (wheelHit.sidewaysSlip > 0) driftFactor = (1 + x) * Mathf.Abs(wheelHit.sidewaysSlip);
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

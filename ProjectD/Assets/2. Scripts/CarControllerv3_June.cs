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
    // WheelColliders Ŭ���� ����
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
        Neutral,        // �߸� ������ ��
        Running,        // ������ �۵� ���� ��
        CheckingChange, // ��� ������ Ȯ�� ���� ��
        Changing        // �� ���� ���� ��
    };

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

    // �ڵ����� ������ ���� WheelColliders
    public WheelColliders colliders;

    // ��Ƽ� �����̴�
    public Slider steerSlider;

    // �Է� ����
    private float gasInput;
    private float brakeInput;

    // �ڵ��� �̵��� ���� �Ű�����

    //�ڵ鸵
    public float maxSteerAngle;
    public float sensitivity;

    //����
    public Transmission transmission;
    public WheelWork wheelWork;
    public float minRPM; //�ּ�RPM
    public float maxRPM; //�ּ�RPM
    public float motorRPM; //���� RPM
    public float wheelRPM; //���� RPM
    public float finalDriveRatio; //���� ���� ���� 3~4 ����
    public float totalMotorTorque; //���� ���� ��ũ
    public AnimationCurve gearRatios;//��� ����
    public AnimationCurve torqueCurve;//��ũ Ŀ��
    public float gearindex;


    //info
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public float speed;
    public float wheelDiameter;

    //
    private bool handrbake = false; //�ڵ� �극��ũ
    public int isEngineRunning;// ������ �۵� �������� ��Ÿ���� ����
    public GearState gearState;    // ���� ��� ���¸� ��Ÿ���� ���� (�߸�, �۵� ��, ��� ���� Ȯ�� ��, ��� ���� ��)
    public float slipAngle;
    public float clutch; // Ŭ��ġ ���¸� ��Ÿ���� ���� (0���� 1 ������ ��)
    public float brakePower;
    public float increaseGearRPM;    // RPM�� ������ �� ��� ������ ���� �ʿ��� �߰� RPM
    public float decreaseGearRPM;    // RPM�� ������ �� ��� ������ ���� �ʿ��� �߰� RPM
    public float redLine;
    public TextMeshProUGUI gearratiostext;
    public TextMeshProUGUI torqueCurvetext;



    public AnimationCurve test_gearRatios;
    public AnimationCurve test_torqueCurve;
    public float[] test_gearRatiosArray;
    public float[] test_torqueCurveArray;

    [Header("Manager")]
    public float tempFloat;

    private void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
        wheelDiameter = colliders.RLWheel.radius;
        MakingGearCurve();
        MakingtorqueCurve();
        initializedminmaxRPM();

        gearRatios = test_gearRatios;
        torqueCurve = test_torqueCurve;

    }
    void initializedminmaxRPM()
    {

    }
    //���� �����
    void MakingGearCurve()
    {
        for (int i = -1; i < test_gearRatiosArray.Length - 1; i++)
        {
            test_gearRatios.AddKey(i, test_gearRatiosArray[i + 1]);
        }
    }
    void MakingtorqueCurve()
    {
        for (int i = 0; i < test_torqueCurveArray.Length; i++)
        {
            test_torqueCurve.AddKey(i * 100 + 1000, test_torqueCurveArray[i]);
        }
    }

    private void Update()
    {
        //wheelRPM = colliders.RLWheel.rpm;
       
        //���� (km/h) = 2pi * Ÿ�̾������ * ����RPM/(���ӱ� ���� x ������ ����)x60/1000
        float tireCircumference = 2 * Mathf.PI * (wheelDiameter / 2);
        speed = tireCircumference * (motorRPM / (gearRatios.Evaluate(gearindex) * finalDriveRatio)) * (60f / 1000f);
        gearratiostext.text = "gearRatios :" + test_gearRatios.Evaluate(gearindex).ToString();
        torqueCurvetext.text = "torqueCurve : " + test_torqueCurve.Evaluate(motorRPM).ToString();



        GetInput();
        gearText.text = "Current Gear : " + gearindex.ToString();
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
            //steerSlider.value += mouseX * sensitivity;
        }
        gasInput = Input.GetAxis("Vertical");
        //print(gasInput);
        handrbake = (Input.GetKey(KeyCode.Space));
        
        // ������ ���� �ְ� ���� �Է��� ���� �� ���� ����
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            if (transmission == Transmission.Auto_Transmission)
                gearindex++;
            gearState = GearState.Running;
            isEngineRunning = 1;
        }
        // �ӵ� ���⿡ ���� �극��ũ �Է� ����
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

        if (wheelWork == WheelWork.FRONT) //����
        {
            colliders.FLWheel.motorTorque = currentTorque / 2;
            colliders.FRWheel.motorTorque = currentTorque / 2;

        }
        if (wheelWork == WheelWork.REAR) //�ķ�
        {
            colliders.RLWheel.motorTorque = currentTorque / 2;
            colliders.RRWheel.motorTorque = currentTorque / 2;

        }
        if (wheelWork == WheelWork.AWD) //4��
        {
            colliders.FLWheel.motorTorque = currentTorque / 4;
            colliders.FRWheel.motorTorque = currentTorque / 4;
            colliders.RLWheel.motorTorque = currentTorque / 4;
            colliders.RRWheel.motorTorque = currentTorque / 4;
        }
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

            //if (transmission == Transmission.Manual_Transmission)
            //{

            //    if (Input.GetKeyDown(KeyCode.E))
            //    {
            //        StartCoroutine(ChangeGear(1));
            //    }
            //    else if (Input.GetKeyDown(KeyCode.Q))
            //    {
            //        StartCoroutine(ChangeGear(-1));
            //    }
            //}



        }
        if (isEngineRunning > 0)
        {
            if (clutch < 0.1f)
            {
                totalMotorTorque = Mathf.Lerp(motorRPM, Mathf.Max(minRPM, maxRPM * gasInput) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
            }
            else
            {
                wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f);
                if (wheelRPM < 0)
                {
                    wheelRPM = 0;
                }
                //currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelsRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * RPMSmoothness);

                //currentTorque = minRPM + (wheelRPM * finalDriveRatio * gearRatios.Evaluate(gearindex));
                motorRPM = Mathf.Lerp(motorRPM,minRPM + (wheelRPM * finalDriveRatio * gearRatios.Evaluate(gearindex)), Time.fixedDeltaTime *2);
                //totalMotorTorque = torqueCurve.Evaluate(currentTorque) * gearRatios.Evaluate(gearindex) * finalDriveRatio * gasInput;
                totalMotorTorque = torqueCurve.Evaluate(motorRPM/maxRPM) * gearRatios.Evaluate(gearindex) * finalDriveRatio * gasInput*tempFloat;
                torque = totalMotorTorque;
            }
            
        }
        return torque;
    }




    


    private void OnEnable()
    {
        colliders.RRWheel.ConfigureVehicleSubsteps(5.0f, 30, 10);
        colliders.RLWheel.ConfigureVehicleSubsteps(5.0f, 30, 10);
        colliders.FRWheel.ConfigureVehicleSubsteps(5.0f, 30, 10);
        colliders.FLWheel.ConfigureVehicleSubsteps(5.0f, 30, 10);
    }



















    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (gearindex + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                // ��� �ø���
                yield return new WaitForSeconds(0.3f);
                if (transmission == Transmission.Auto_Transmission)
                {
                    if (motorRPM < increaseGearRPM || gearindex >= finalDriveRatio)
                    {
                        gearState = GearState.Running;
                        yield break;
                    }

                    if (transmission == Transmission.Manual_Transmission)
                    {
                        gearState = GearState.Running;
                        yield break;
                    }
                }
            }
            if (gearChange < 0)
            {
                // ��� ������
                yield return new WaitForSeconds(0.1f);
                if (transmission == Transmission.Auto_Transmission)
                {
                    if (motorRPM > decreaseGearRPM || gearindex <= 0)
                    {
                        gearState = GearState.Running;
                        yield break;
                    }
                    if (transmission == Transmission.Manual_Transmission)
                    {
                        gearState = GearState.Running;
                        yield break;
                    }
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

            if (Input.GetKeyDown(KeyCode.E) && gearindex < test_gearRatiosArray.Length - 2)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (Input.GetKeyDown(KeyCode.Q) && gearindex > -1)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }
    }

}
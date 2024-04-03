using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CarControllerv2_June;

// �ڵ����� ��� ���¸� ��Ÿ���� ������
public enum GearState
{
    Neutral,        // �߸� ������ ��
    Running,        // ������ �۵� ���� ��
    CheckingChange, // ��� ������ Ȯ�� ���� ��
    Changing        // �� ���� ���� ��
};

// CarController_June Ŭ���� ����
public class CarController_June : MonoBehaviour
{
    // �÷��̾��� Rigidbody ���� ���
    private Rigidbody playerRB;
    public Vector3 _centerOfMass;

    // �ڵ����� ������ ���� WheelColliders
    public WheelColliders colliders;

    // �Է� ����
    private float gasInput;
    private float brakeInput;

    // �ڵ��� �̵��� ���� �Ű�����
    public float motorPower;
    public float brakePower;
    public float slipAngle;
    public float speed;
    private float speedClamped;
    public float maxSpeed;
    public AnimationCurve steeringCurve;
    public float sensitivity;

    //���� ����
    public int isEngineRunning;// ������ �۵� �������� ��Ÿ���� ����
    public float RPM;// ���� RPM(Revolutions Per Minute, �д� ȸ����)�� �����ϴ� ����
    public float redLine;// ������ �ִ� ȸ�� �ӵ��� ��Ÿ���� ����
    public float idleRPM;// ������ ��� ������ ���� RPM
    public TMP_Text rpmText;
    public int currentGear;// ���� �� ��Ÿ���� ����
    public float[] gearRatios; // ���� ���(RPM�� ���� ���ϴ� ���� ���)�� ���� ��� ������ �����ϴ� �迭
    public float differentialRatio; // ���ӱ�(differential) ����
    private float currentTorque; // ���� ��ũ ���� �����ϴ� ����
    private float clutch; // Ŭ��ġ ���¸� ��Ÿ���� ���� (0���� 1 ������ ��)
    private float wheelRPM;  // ���� RPM�� �����ϴ� ����
    public AnimationCurve hpToRPMCurve;    // ���� ��°� RPM ���� ���踦 �����ϴ� Ŀ��
    private GearState gearState;    // ���� ��� ���¸� ��Ÿ���� ���� (�߸�, �۵� ��, ��� ���� Ȯ�� ��, ��� ���� ��)
    public float increaseGearRPM;    // RPM�� ������ �� ��� ������ ���� �ʿ��� �߰� RPM
    public float decreaseGearRPM;    // RPM�� ������ �� ��� ������ ���� �ʿ��� �߰� RPM
    public float changeGearTime = 0.5f;    // �� �����ϴ� �� �ɸ��� �ð�
    private bool handrbake = false; //�ڵ� �극��ũ


    // Start is called before the first frame update
    void Start()
    {
        // Rigidbody ���� ��� ��������
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
    }

    // Update is called once per frame
    void Update()
    {
        // RPM �� �ӵ� ����
        rpmText.text = RPM.ToString("0,000") + "rpm";
        speed = colliders.RRWheel.rpm * colliders.RRWheel.radius * 2f * Mathf.PI / 10f;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);

        // �Է� Ȯ��
        CheckInput();
    }

    // ���� ������Ʈ���� ȣ��Ǵ� �Լ�
    private void FixedUpdate()
    {
        // �ڵ��� ���� �Լ� ȣ��
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
       
    }

    // ��Ƽ� �����̴�
    public Slider steerSlider;

    // �Է� Ȯ�� �Լ�
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

        // ���� �� �극��ũ �Է� Ȯ��
        gasInput = Input.GetAxis("Vertical");
        if (gasInput > 0)
        {
            gasInput += sensitivity * Time.deltaTime;
        }
        if (gasInput < 0)
        {
            gasInput -= sensitivity * Time.deltaTime;
        }

        // ������ ���� �ְ� ���� �Է��� ���� �� ���� ����
        if (Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            gearState = GearState.Running;
        }

        // ���콺 X �Է¿� ���� ��Ƽ� ����
        float mouseX = Input.GetAxis("Mouse X");
        if (mouseX != 0)
        {
            steerSlider.value += mouseX;
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



    public float originFriction = 1f;
    public float handbreakFriction = 0.8f;

    WheelFrictionCurve fFriction;
    WheelFrictionCurve sFriction;

    // �극��ũ ���� �Լ�
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

 


    // ���� ���� �Լ�
    void ApplyMotor()
    {
        currentTorque = CalculateTorque();
        colliders.RRWheel.motorTorque = currentTorque * gasInput;
        colliders.RLWheel.motorTorque = currentTorque * gasInput;
    }

    // ��ũ ��� �Լ�
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

    // ��Ƽ� ���� �Լ�
    void ApplySteering()
    {
        float steeringAngle;
        steeringAngle = steeringCurve.Evaluate(speed) *steerSlider.value * 30 / (1080.0f / 2);
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    // ��� ���� �ڷ�ƾ
    IEnumerator ChangeGear(int gearChange)
    {
        gearState = GearState.CheckingChange;
        if (currentGear + gearChange >= 0)
        {
            if (gearChange > 0)
            {
                // ��� �ø���
                yield return new WaitForSeconds(0.7f);
                if (RPM < increaseGearRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Running;
                    yield break;
                }
            }
            if (gearChange < 0)
            {
                // ��� ������
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

// WheelColliders Ŭ���� ����
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CarControllerv2_June;





public class Test : MonoBehaviour
{    // WheelColliders Ŭ���� ����
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

    // �ڵ��� �̵��� ���� �Ű�����

    //�ڵ鸵
    public float maxSteerAngle;
    public float sensitivity;

    //����
    public Transmission transmission;
    public float minRPM; //�ּ�RPM
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




    private void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        playerRB.centerOfMass = _centerOfMass;
        wheelDiameter = colliders.RLWheel.radius;
    }


    private void Update()
    {
        //wheelRPM = colliders.RLWheel.rpm;
        wheelRPM = Mathf.Abs((colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f);

        //���� (km/h) = 2pi * Ÿ�̾������ * ����RPM/(���ӱ� ���� x ������ ����)x60/1000
        float tireCircumference = 2 * Mathf.PI * (wheelDiameter / 2);
        speed = tireCircumference * (motorRPM / (gearRatios.Evaluate(gearindex) * finalDriveRatio)) * (60f / 1000f);




        GetInput();
        gearText.text = gearindex.ToString();
        //speedText.text = speed.ToString() ;
        speedText.text = playerRB.velocity.magnitude.ToString();

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
            //steerSlider.value += mouseX;
        }
        gasInput = Input.GetAxis("Vertical");
        print(gasInput);



    }


    public void ApplyMotor()
    {
        motorRPM = minRPM + (wheelRPM * finalDriveRatio * gearRatios.Evaluate(gearindex));
        totalMotorTorque = torqueCurve.Evaluate(motorRPM) * gearRatios.Evaluate(gearindex) * finalDriveRatio * gasInput;
        colliders.RLWheel.motorTorque = totalMotorTorque / 2; //�޹�����
        colliders.RRWheel.motorTorque = totalMotorTorque / 2;
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

    }
    public void ApplyGear()
    {
        if (transmission == Transmission.Manual_Transmission)
        {
            if (Input.GetKeyDown(KeyCode.E) && gearindex < finalDriveRatio)
            {
                gearindex++;
            }
            else if (Input.GetKeyDown(KeyCode.Q) && gearindex > 0)
            {
                gearindex--;
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PepperoniTest : MonoBehaviour
{
    [Header("Ȯ�ο� �ӽ� UI elements")]
    [SerializeField] TextMeshProUGUI rpmText;
    [SerializeField] TextMeshProUGUI accelText;
    [SerializeField] TextMeshProUGUI gearText;
    [SerializeField] TextMeshProUGUI speedText;

    [Space(10)]
    [SerializeField] List<AxleInfo> axleInfos;                            // ���� �� �ݶ��̴� �Ҵ�� ����Ʈ
    [SerializeField] float enginePower = 300f;                          // RPM �� �󸶳� ���� ����� ������ ���ϴ� multiplier
    [SerializeField] [Range(0.1f, 5f)] float accelSensitivity = 1f;      // �Ǽ� Ű�� ������ �� �󸶳� ��ġ�� ������ ���ϴ��� ���ϴ� ����
    [SerializeField] [Range(1, 10)] int maxGear = 6;                   // ������ ���� �� �ִ� �ִ� ���(���� ���� ���� �������� ����)
    [SerializeField][Range(4000f, 10000f)] float maxRPM = 7000f;      // �� ���� �� �� �ִ� �ִ� RPM
    [SerializeField] float maxSpeed = 30;                               // ������ �� �ִ� �ӵ�, ��� �� �ִ� �ӵ� ����� ���� �ʿ�
    [SerializeField] float baseRpm = 500f;                              // �Ǽ� 0�� �� �⺻���� �����Ǵ� rpm
    [SerializeField] float rpmReducer = 50f;                             // �ڿ������� �پ��� rpm ���Ⱚ

    [SerializeField] float breakingPower = 100f;

    [SerializeField] float steeringSensitivity = 20f;
    [SerializeField] float maxSteeringAngle = 30f;

    Rigidbody rigid;
    bool drifting;
    bool breaking;
    WheelFrictionCurve forwardCurve;
    WheelFrictionCurve sideCurve;
    WheelFrictionCurve driftForwardCurve;
    WheelFrictionCurve driftSideCurve;

    float acceleration;
    int currentGear;
    float currentRPM;
    float motor;
    float steering;
    float powerPerGear;

    private void Start()
    {
        forwardCurve = axleInfos[0].leftWheel.forwardFriction;
        sideCurve = axleInfos[0].leftWheel.sidewaysFriction;

        driftForwardCurve = forwardCurve;
        driftForwardCurve.stiffness = forwardCurve.stiffness / 2f;

        driftSideCurve = sideCurve;
        driftSideCurve.stiffness = sideCurve.stiffness / 2f;
        rigid = GetComponent<Rigidbody>();

        powerPerGear = maxSpeed / maxGear;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            drifting = true;
        }
        else
        {
            drifting = false;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            acceleration = Mathf.Clamp(acceleration + 1 * accelSensitivity, 0, 100);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            acceleration = Mathf.Clamp(acceleration - 1 * accelSensitivity, 0, 100);
        }

        if (Input.GetKey(KeyCode.S))
        {
            breaking = true;
        }
        else
        {
            breaking = false;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentGear--;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentGear++;
        }



        if (Input.GetKey(KeyCode.Mouse0))
        {
            steering += Input.GetAxis("Mouse X") * steeringSensitivity* Time.deltaTime;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            steering = Mathf.Lerp(steering, 0, steeringSensitivity * Time.deltaTime * 0.1f);
        }

        //if (Input.GetKeyUp(KeyCode.Mouse0))
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    steering = 0;
        //}
        //Debug.Log(steering);

        //Debug.Log(rigid.velocity.magnitude);
        rpmText.text = currentRPM.ToString();
        accelText.text = acceleration.ToString();
        gearText.text = currentGear.ToString();
        speedText.text = rigid.velocity.magnitude.ToString();
        //Debug.Log(axleInfos[0].rightWheel.for);
    }

    public void FixedUpdate()
    {
        //motor = maxMotorTorque * Input.GetAxis("Vertical");
        //steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        //steering *= steeringSensitivity;
        motor = MotorCalculation();

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (breaking)
            {
                axleInfo.leftWheel.brakeTorque = breakingPower * Time.deltaTime;
                axleInfo.rightWheel.brakeTorque = breakingPower * Time.deltaTime;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }

            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                // �ӽ�
                if(rigid.velocity.magnitude / maxGear < powerPerGear * currentGear)
                {
                    axleInfo.leftWheel.motorTorque = motor * Time.deltaTime;
                    axleInfo.rightWheel.motorTorque = motor * Time.deltaTime;
                }
                else
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.rightWheel.motorTorque = 0;
                    //axleInfo.rightWheel.rpm
                }


                if (drifting)
                {
                    //axleInfo.leftWheel.forwardFriction = driftForwardCurve;
                    axleInfo.leftWheel.sidewaysFriction = driftSideCurve;
                    //axleInfo.rightWheel.forwardFriction = driftForwardCurve;
                    axleInfo.rightWheel.sidewaysFriction = driftSideCurve;
                }
                else
                {
                    //axleInfo.leftWheel.forwardFriction = forwardCurve;
                    axleInfo.leftWheel.sidewaysFriction = sideCurve;
                    //axleInfo.rightWheel.forwardFriction = forwardCurve;
                    axleInfo.rightWheel.sidewaysFriction = sideCurve;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>������ ���� ��ũ</returns>
    float MotorCalculation()
    {
        // �Ǽ� �翡 ���� RPM�� ������Ų��
        // RPM�� ��� �ܼ��� ���� ���� ����� �����Ѵ�
        // �ִ� RPM�� �������� ���� �� �̻��� ����� ���� �ʴ´�
        // ������ ���� ��� �� ������ ����� �� ����� ���� ���Ѵ�
        
        if(currentRPM < maxRPM)
        {
            currentRPM += acceleration * enginePower * Time.deltaTime;
        }

        if(currentRPM > baseRpm)
        {
            currentRPM -= rpmReducer * Time.deltaTime;
        }
        else
        {
            currentRPM = baseRpm;
        }

        return currentRPM * currentGear;
    }


}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

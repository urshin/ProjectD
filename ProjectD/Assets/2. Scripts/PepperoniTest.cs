using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PepperoniTest : MonoBehaviour
{
    [Header("확인용 임시 UI elements")]
    [SerializeField] TextMeshProUGUI rpmText;
    [SerializeField] TextMeshProUGUI accelText;
    [SerializeField] TextMeshProUGUI gearText;
    [SerializeField] TextMeshProUGUI speedText;

    [Space(10)]
    [SerializeField] List<AxleInfo> axleInfos;                            // 차량 휠 콜라이더 할당용 리스트
    [SerializeField] float enginePower = 300f;                          // RPM 당 얼마나 강한 출력을 만들지 정하는 multiplier
    [SerializeField] [Range(0.1f, 5f)] float accelSensitivity = 1f;      // 악셀 키를 눌렀을 때 얼마나 수치가 빠르게 변하는지 정하는 변수
    [SerializeField] [Range(1, 10)] int maxGear = 6;                   // 차량이 가질 수 있는 최대 기어(현재 후진 기어는 상정하지 않음)
    [SerializeField][Range(4000f, 10000f)] float maxRPM = 7000f;      // 각 기어에서 낼 수 있는 최대 RPM
    [SerializeField] float maxSpeed = 30;                               // 차량이 낼 최대 속도, 기어 별 최대 속도 계산을 위해 필요
    [SerializeField] float baseRpm = 500f;                              // 악셀 0일 때 기본으로 유지되는 rpm
    [SerializeField] float rpmReducer = 50f;                             // 자연적으로 줄어드는 rpm 감쇄값

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
                // 임시
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
    /// <returns>바퀴에 가할 토크</returns>
    float MotorCalculation()
    {
        // 악셀 양에 따라서 RPM을 증가시킨다
        // RPM과 기어 단수에 따른 모터 출력을 리턴한다
        // 최대 RPM에 도달했을 때는 더 이상의 출력을 내지 않는다
        // 저속일 때에 고단 기어를 물리면 제대로 된 출력을 받지 못한다
        
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

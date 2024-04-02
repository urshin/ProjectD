using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepperoniTest : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    Rigidbody rigid;
    bool drifting;
    WheelFrictionCurve forwardCurve;
    WheelFrictionCurve sideCurve;
    WheelFrictionCurve driftForwardCurve;
    WheelFrictionCurve driftSideCurve;

    float motor;
    float steering;

    float steeringSensitivity = 10f;

    private void Start()
    {
        forwardCurve = axleInfos[0].leftWheel.forwardFriction;
        sideCurve = axleInfos[0].leftWheel.sidewaysFriction;

        driftForwardCurve = forwardCurve;
        //driftForwardCurve.extremumSlip = forwardCurve.extremumSlip / 2f;
        //driftForwardCurve.extremumValue = forwardCurve.extremumValue / 2f;
        //driftForwardCurve.asymptoteValue = forwardCurve.asymptoteValue / 2f;
        driftForwardCurve.stiffness = 0.3f;//forwardCurve.stiffness / 5f;

        driftSideCurve = sideCurve;
        //driftSideCurve.extremumSlip = sideCurve.extremumSlip / 2f;
        //driftSideCurve.extremumValue = sideCurve.extremumValue * 2f;
        //driftSideCurve.asymptoteValue = sideCurve.asymptoteValue * 2f;
        driftSideCurve.stiffness = 0.3f;// sideCurve.stiffness / 5f;
        rigid = GetComponent<Rigidbody>();
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

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Debug.Log(steering);
            steering += Input.GetAxis("Mouse X") * maxSteeringAngle * Time.deltaTime;
            Mathf.Clamp(steering, -30f, 30f);
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.None;
            steering = 0;
        }

        //Debug.Log(rigid.velocity.magnitude);

    }

    public void FixedUpdate()
    {
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        //steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        //steering *= steeringSensitivity;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;

                if (drifting)
                {
                    axleInfo.leftWheel.forwardFriction = driftForwardCurve;
                    axleInfo.leftWheel.sidewaysFriction = driftSideCurve;
                    axleInfo.rightWheel.forwardFriction = driftForwardCurve;
                    axleInfo.rightWheel.sidewaysFriction = driftSideCurve;
                }
                else
                {
                    axleInfo.leftWheel.forwardFriction = forwardCurve;
                    axleInfo.leftWheel.sidewaysFriction = sideCurve;
                    axleInfo.rightWheel.forwardFriction = forwardCurve;
                    axleInfo.rightWheel.sidewaysFriction = sideCurve;
                }
            }
        }
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

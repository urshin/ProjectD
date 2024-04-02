using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerv2_June : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    public Slider steerSlider;

    private Rigidbody carRb;

    //private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

        // carLights = GetComponent<CarLights>();
        foreach (Wheel wheel in wheels)
        {
            if(wheel.axel == Axel.Rear)
            {
                fFriction = wheel.wheelCollider.forwardFriction;
                sFriction = wheel.wheelCollider.sidewaysFriction;
            }
        }
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
        HandBrake();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
        // 마우스의 X축 움직임을 가져옵니다.
        float mouseX = Input.GetAxis("Mouse X");

        // 움직임이 감지되면 디버그 로그로 출력합니다.
        if (mouseX != 0)
        {
            steerSlider.value += mouseX;
        }
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                //var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                //wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);

                var _steerAngle = steerSlider.value * maxSteerAngle / (1080.0f / 2);
                wheel.wheelCollider.steerAngle = _steerAngle;
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space) || moveInput == 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }


    public float originFriction =1f;
    public float handbreakFriction = 0.3f;

    WheelFrictionCurve fFriction;
    WheelFrictionCurve sFriction;

    
   

    void HandBrake()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var wheel in wheels)
            {
                if(wheel.axel == Axel.Rear)
                {
                    fFriction.stiffness = handbreakFriction;
                    sFriction.stiffness = handbreakFriction;
                wheel.wheelCollider.forwardFriction = fFriction;
                wheel.wheelCollider.sidewaysFriction = sFriction;
                

                }
            }

            //carLights.isBackLightOn = true;
            //carLights.OperateBackLights();
        }
        else
        {
            foreach (var wheel in wheels)
            {
                if (wheel.axel == Axel.Rear)
                {
                    fFriction.stiffness = originFriction;
                    sFriction.stiffness = originFriction;
                    wheel.wheelCollider.forwardFriction = fFriction;
                    wheel.wheelCollider.sidewaysFriction = sFriction;
                }
            }

            //carLights.isBackLightOn = false;
            //carLights.OperateBackLights();
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}

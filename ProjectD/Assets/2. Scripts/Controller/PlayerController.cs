using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //input
    private float mouseX;
    private float gasInput;
    Camera cam;
    [Range(-540, 540)] public float steerSlider;
    [Range(10, 90)] public float maxSteerAngle; //최대 각도
    public float sensitivity; //마우스 감도 
    [SerializeField] bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //스티어 앵글 각도 초기화 시간

    CarController_ carController;

    private void Start()
    {
        InitializedSetting();
        
    }

    private void Update()
    {   
        if(carController != null)
        {
            GetInput();

        }
    }

    private void FixedUpdate()
    {
        if (carController != null)
        {
            Steering();

        }
    }

    public void GetInput() //인풋 값 받기
    {
        mouseX = Input.GetAxis("Mouse X");
        gasInput = Input.GetAxis("Vertical");
        carController.accel = gasInput;
        if(Input.GetKeyDown(KeyCode.Space))
        {
            carController.handBrake = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            carController.handBrake = false;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            carController.downShift = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            carController.upShift = true;
        }
    }
    void InitializedSetting()
    {
        cam = Camera.main;
        //cam.GetComponent<CamController_June>().player = gameObject.transform;

        // 자식으로 차량을 붙이고 그 차량의 CarController_ 클래스를 가져와야한다
        // 임시
        carController = GetComponentInChildren<CarController_>();
    }

    public void Steering()//fixedUpdate
    {
        //steerSlider.value 값을 차량에 전달하여 스티어링시켜야 함
        if (Input.GetMouseButton(0))
        {
            if (mouseX != 0)
            {
                steerSlider += mouseX * sensitivity;
            }

        }
        else
        {
            steerSlider = Mathf.Lerp(steerSlider, 0, Time.fixedDeltaTime * resetSteerAngleSpeed);
        }
        carController.steerValue = steerSlider;
    }
}

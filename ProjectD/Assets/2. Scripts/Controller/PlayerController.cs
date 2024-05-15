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
    [Range(10, 90)] public float maxSteerAngle; //�ִ� ����
    public float sensitivity; //���콺 ���� 
    [SerializeField] bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //��Ƽ�� �ޱ� ���� �ʱ�ȭ �ð�

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

    public void GetInput() //��ǲ �� �ޱ�
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

        // �ڽ����� ������ ���̰� �� ������ CarController_ Ŭ������ �����;��Ѵ�
        // �ӽ�
        carController = GetComponentInChildren<CarController_>();
    }

    public void Steering()//fixedUpdate
    {
        //steerSlider.value ���� ������ �����Ͽ� ��Ƽ����Ѿ� ��
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

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
    public Slider steerSlider;
    [Range(10, 90)] public float maxSteerAngle; //�ִ� ����
    public float sensitivity; //���콺 ���� 
    [SerializeField] bool handBrake = false;
    public float handBrakeSleepAmout = 0.55f;
    public float resetSteerAngleSpeed = 100f; //��Ƽ�� �ޱ� ���� �ʱ�ȭ �ð�

    CarController carController;

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
    }
    void InitializedSetting()
    {
        cam = Camera.main;
        //cam.GetComponent<CamController_June>().player = gameObject.transform;

        // �ڽ����� ������ ���̰� �� ������ CarController Ŭ������ �����;��Ѵ�
        // �ӽ�
        carController = GetComponentInChildren<CarController>();
    }

    public void Steering()//fixedUpdate
    {
        //steerSlider.value ���� ������ �����Ͽ� ��Ƽ����Ѿ� ��
        if (Input.GetMouseButton(0))
        {
            if (mouseX != 0)
            {
                steerSlider.value += mouseX * sensitivity;
            }

        }
        else
        {
            steerSlider.value = Mathf.Lerp(steerSlider.value, 0, Time.fixedDeltaTime * resetSteerAngleSpeed);
        }
        carController.steerValue = steerSlider.value;
    }
}

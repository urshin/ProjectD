using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerv4_june : MonoBehaviour
{

    // WheelColliders Ŭ���� ����
    [System.Serializable]
    public class Pair
    {
        public WheelCollider left; // ���� ���� �ݶ��̴�
        public WheelCollider right; // ������ ���� �ݶ��̴�
        public bool IsMoterPower; // ���� �� �޴���
        public bool IsSteering; //��Ƽ�� �� �޴���
        public float brakeBias = 0.5f; //�극��ũ ���� ũ��
        public float handbrakeBias = 5f; //�ڵ�극��ũ ���� ũ��
        [System.NonSerialized] //public�̶� �ν����� â���� ������ ����
        public WheelHit hitLeft; // ���� �� ��Ʈ
        [System.NonSerialized]
        public WheelHit hitRight; // ������ �� ��Ʈ
        //[System.NonSerialized]
        public bool isGroundedLeft = false; // ������ ���� �پ��ִ��� ����
        //[System.NonSerialized]
        public bool isGroundedRight = false; // �������� ���� �پ��ִ��� ����
    }

    public List<Pair> pair;





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



    private void OnEnable()
    {
        
    }

    private void Update()
    {
        WheelRPM();

    }

    public float totalPower;
    public float engineRPM;
    private void FixedUpdate()
    {
        
    }

    public float wheelsRPM;
    void WheelRPM()
    {
        float sum = 0;
        int R = 0;
        for(int i = 0; i<2; i++)
        {
            sum += pair[i].left.rpm;
            sum += pair[i].right.rpm;
            R++;
            R++;
        }
        wheelsRPM = (R!=0)? sum/R: 0;
    }


}








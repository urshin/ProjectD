using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerv4_june : MonoBehaviour
{

    // WheelColliders 클래스 선언
    [System.Serializable]
    public class Pair
    {
        public WheelCollider left; // 왼쪽 바퀴 콜라이더
        public WheelCollider right; // 오른쪽 바퀴 콜라이더
        public bool IsMoterPower; // 모터 힘 받는지
        public bool IsSteering; //스티어 힘 받는지
        public float brakeBias = 0.5f; //브레이크 전달 크기
        public float handbrakeBias = 5f; //핸드브레이크 전달 크기
        [System.NonSerialized] //public이라도 인스펙터 창에서 가리기 위함
        public WheelHit hitLeft; // 왼쪽 휠 히트
        [System.NonSerialized]
        public WheelHit hitRight; // 오른쪽 휠 히트
        //[System.NonSerialized]
        public bool isGroundedLeft = false; // 왼쪽이 땅에 붙어있는지 여부
        //[System.NonSerialized]
        public bool isGroundedRight = false; // 오른쪽이 땅에 붙어있는지 여부
    }

    public List<Pair> pair;





    public enum WheelWork //전,후,4륜
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
    // 플레이어의 Rigidbody 구성 요소
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








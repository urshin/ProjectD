using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarParticle : MonoBehaviour
{
    FinalCarController_June carController;
    [SerializeField] List< ParticleSystem> smoke;
    [SerializeField] GameObject smokePrefab1;
    [SerializeField] GameObject smokePrefab2;
    [SerializeField] GameObject collisionParticle;
    [SerializeField] Collider[] carBodyCollier;
    [SerializeField] float slipAllowance;

    private void OnEnable()
    {
        carController = gameObject.GetComponent<FinalCarController_June>();
        slipAllowance= carController.slipAllowance;
        for (int i = 0; i < carController.wheels.Count; i++)
        {
        smoke.Add( Instantiate(smokePrefab1, carController.wheels[i].wheelCollider.transform.position, Quaternion.identity, carController.wheels[i].wheelCollider.transform).GetComponent<ParticleSystem>()) ;

        }

    }
    private void Update()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        for(int i = 2;i < carController.wheels.Count;i++)
        {
            carController.wheels[i].wheelCollider.GetGroundHit(out wheelHits[i]);
            GetComponent<CarAudio>().tireSlipAmount = Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip);
            if (Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip) > slipAllowance)
            {
                //print(Mathf.Abs(wheelHits.sidewaysSlip) + Mathf.Abs(wheelHits.forwardSlip));
                smoke[i].Emit(1);
              
                GetComponent<CarAudio>().isSkid = true;
            }
            else
            {
                GetComponent<CarAudio>().isSkid = false;
            }
        }

        ScrollScreechEffect();




    }

    [SerializeField] GameObject Hitprefab;
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("웅라ㅡㅇㅁㄴ아ㅣ;럼ㄴㅇ;ㅣㅏ럼ㄴ;ㅣㅏㅇ");
        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        ParticleSystem particle = Instantiate(collisionParticle, contact.point, rotation).GetComponent<ParticleSystem>();
        // 파티클 크기를 설정할 변수
        float particleSize = carController.KPH / 10;

        // 파티클 시스템의 startSize 속성을 사용하여 크기 설정
        ParticleSystem.MainModule mainModule = particle.main;
        mainModule.startSize = particleSize;
       GameObject hitCanvas =  Instantiate(Hitprefab, contact.point, Quaternion.LookRotation(Camera.main.transform.forward));
        Destroy(hitCanvas, 1.0f);
        // particle.Emit(5);
        // 생성된 파티클을 일정 시간 후에 파괴
        Destroy(particle, 1.0f);

    }
    private void OnCollisionStay(Collision collision)
    {
       // Debug.Log("드르르르르륵");
    }

  
    [SerializeField] RectTransform[] images;
    [SerializeField] float scrollSpeed = 2f;
    [SerializeField] float scrollThreshold = 4f;
    float imageWidth = 4;
    private void ScrollScreechEffect()
    {
        foreach (RectTransform image in images)
        {
            // 현재 이미지의 위치를 증가시킴
            image.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;
            image.sizeDelta += new Vector2( 2 * Time.deltaTime * scrollSpeed, 1 * Time.deltaTime * scrollSpeed);
            // 이미지가 화면을 벗어나면 이동하여 뒤에 추가
            if (image.anchoredPosition.x >= scrollThreshold)
            {
                image.anchoredPosition -= Vector2.right * (scrollThreshold);
                image.sizeDelta = Vector2.zero;

            }
        }
    }

}

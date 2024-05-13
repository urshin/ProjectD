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
        //Debug.Log("枢虞ぱしけい焼び;軍いし;びた軍い;びたし");
        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        ParticleSystem particle = Instantiate(collisionParticle, contact.point, rotation).GetComponent<ParticleSystem>();
        // 督銅適 滴奄研 竺舛拝 痕呪
        float particleSize = carController.KPH / 10;

        // 督銅適 獣什奴税 startSize 紗失聖 紫遂馬食 滴奄 竺舛
        ParticleSystem.MainModule mainModule = particle.main;
        mainModule.startSize = particleSize;
       GameObject hitCanvas =  Instantiate(Hitprefab, contact.point, Quaternion.LookRotation(Camera.main.transform.forward));
        Destroy(hitCanvas, 1.0f);
        // particle.Emit(5);
        // 持失吉 督銅適聖 析舛 獣娃 板拭 督雨
        Destroy(particle, 1.0f);

    }
    private void OnCollisionStay(Collision collision)
    {
       // Debug.Log("球牽牽牽牽犬");
    }

  
    [SerializeField] RectTransform[] images;
    [SerializeField] float scrollSpeed = 2f;
    [SerializeField] float scrollThreshold = 4f;
    float imageWidth = 4;
    private void ScrollScreechEffect()
    {
        foreach (RectTransform image in images)
        {
            // 薄仙 戚耕走税 是帖研 装亜獣鉄
            image.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;
            image.sizeDelta += new Vector2( 2 * Time.deltaTime * scrollSpeed, 1 * Time.deltaTime * scrollSpeed);
            // 戚耕走亜 鉢檎聖 込嬢蟹檎 戚疑馬食 及拭 蓄亜
            if (image.anchoredPosition.x >= scrollThreshold)
            {
                image.anchoredPosition -= Vector2.right * (scrollThreshold);
                image.sizeDelta = Vector2.zero;

            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarParticle : MonoBehaviour
{
    CarController_ carController;
    [SerializeField] List< ParticleSystem> smoke;
    [SerializeField] GameObject smokePrefab1;
    [SerializeField] GameObject collisionParticle;
    [SerializeField] Collider[] carBodyCollier;
    [SerializeField] float slipAllowance;
    [SerializeField] GameObject ScreechObject;
    [SerializeField] Parabola[] screeches;
    private void OnEnable()
    {
        carController = gameObject.GetComponent<CarController_>();
        slipAllowance = carController.slipAllowance;

       
            screeches = ScreechObject.GetComponentsInChildren<Parabola>();
        
        for (int i = 0; i < carController.wheels.Count; i++)
        {
            smoke.Add(Instantiate(smokePrefab1, carController.wheels[i].wheelCollider.transform.position, Quaternion.identity, carController.wheels[i].wheelCollider.transform).GetComponent<ParticleSystem>());

        }

    }
    private void Update()
    {
        ScreechEffect();

        ScrollScreechEffect();




    }

    private void ScreechEffect()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        for (int i = 2; i < carController.wheels.Count; i++)
        {
            carController.wheels[i].wheelCollider.GetGroundHit(out wheelHits[i]);
            GetComponent<CarAudio>().tireSlipAmount = Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip);
            if (Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip) > slipAllowance)
            {
                //print(Mathf.Abs(wheelHits.sidewaysSlip) + Mathf.Abs(wheelHits.forwardSlip));
                smoke[i].Emit(1);
                for (int j = 0; j < screeches.Length; j++)
                {
                    var a = screeches[j].line.material;
                    a.color = new Color(a.color.r, a.color.g, a.color.b, Mathf.Lerp(a.color.a, 1, 0.09f)); // 알파값을 1로 설정 (완전 불투명)
                }
                GetComponent<CarAudio>().isSkid = true;
            }
            else
            {
                for (int j = 0; j < screeches.Length; j++)
                {
                   var a = screeches[j].line.material;
                    a.color = new Color(a.color.r, a.color.g, a.color.b, Mathf.Lerp(a.color.a, 0, 0.1f)); // 알파값을 1로 설정 (완전 불투명)
                }
                GetComponent<CarAudio>().isSkid = false;
            }
        }

           
        foreach (var a in screeches)
        {
            
            //Quaternion newRotation = Quaternion.Euler(0, carController._driftAngle/5-90, 0);
           // a.transform.rotation = newRotation;

            a.P2_GameObject.transform.localPosition = new Vector3(-1.5f, 0, Mathf.Clamp(carController._driftAngle / 90 * -2, -4, 4));
        }

    }


    [SerializeField] GameObject Hitprefab;
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("웅라ㅡㅇㅁㄴ아ㅣ;럼ㄴㅇ;ㅣㅏ럼ㄴ;ㅣㅏㅇ");
        ContactPoint contact = collision.contacts[0];
       GameObject hitCanvas =  Instantiate(Hitprefab, contact.point, Quaternion.LookRotation(Camera.main.transform.forward));
        Destroy(hitCanvas, 1.0f);

    }
    private void OnCollisionStay(Collision collision)
    {
       // Debug.Log("드르르르르륵");
    }


    [SerializeField] RectTransform[] images;

    [Range(1, 40)][SerializeField] float scrollSpeed = 15f;
    float screenWidth = 10;
    float totalWidth = 10f;
    private void ScrollScreechEffect()
    {
        // Move each image to the right
        foreach (RectTransform image in images)
        {
            image.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;

            // If an image is completely off screen, move it to the end
            if (image.anchoredPosition.x >= screenWidth)
            {
                float offsetX = screenWidth*2;
                image.anchoredPosition -= new Vector2(offsetX, 0);
            }
        }
    }



}

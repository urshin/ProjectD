//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class CarParticle : MonoBehaviour
//{
//    //FinalCarController_June carController;
//    [SerializeField] List< ParticleSystem> smoke;
//    [SerializeField] GameObject smokePrefab1;
//    [SerializeField] GameObject smokePrefab2;
//    [SerializeField] GameObject collisionParticle;
//    [SerializeField] Collider[] carBodyCollier;
//    [SerializeField] float slipAllowance;
// [SerializeField] RectTransform[] images;
//    [SerializeField] RawImage[] Screeches;
//    float imageWidth = 2f;
//    [Range(1,40)] [SerializeField] float scrollSpeed = 1f;
//    float screenWidth = 6;
//    private void OnEnable()
//    {
//       // carController = gameObject.GetComponent<FinalCarController_June>();
//       // slipAllowance= carController.slipAllowance;
//        //for (int i = 0; i < carController.wheels.Count; i++)
//       // {
//       // smoke.Add( Instantiate(smokePrefab1, carController.wheels[i].wheelCollider.transform.position, Quaternion.identity, carController.wheels[i].wheelCollider.transform).GetComponent<ParticleSystem>()) ;

//       // }

//    }
//    private void Update()
//    {
//        WheelHit[] wheelHits = new WheelHit[4];
//        //for(int i = 2;i < carController.wheels.Count;i++)
//        {
//          //  carController.wheels[i].wheelCollider.GetGroundHit(out wheelHits[i]);
//            GetComponent<CarAudio>().tireSlipAmount = Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip);
//            if (Mathf.Abs(wheelHits[i].sidewaysSlip) + Mathf.Abs(wheelHits[i].forwardSlip) > slipAllowance)
//            {
//                //print(Mathf.Abs(wheelHits.sidewaysSlip) + Mathf.Abs(wheelHits.forwardSlip));
//                smoke[i].Emit(1);
//                for (int j = 0;j < Screeches.Length;j++)
//                {
//                    var a = Screeches[j];
//                    a.color =  new Color(a.color.r, a.color.g, a.color.b, Mathf.Lerp(a.color.a, 1,0.09f)); // ���İ��� 1�� ���� (���� ������)
//                }
//                GetComponent<CarAudio>().isSkid = true;
//            }
//            else
//            {
//                for (int j = 0; j < Screeches.Length; j++)
//                {
//                    var a = Screeches[j];
//                    a.color = new Color(a.color.r, a.color.g, a.color.b, Mathf.Lerp(a.color.a,0, 0.1f)); // ���İ��� 1�� ���� (���� ������)
//                }
//                GetComponent<CarAudio>().isSkid = false;
//            }
//        }

//        ScrollScreechEffect();




//    }

//    [SerializeField] GameObject Hitprefab;
//    private void OnCollisionEnter(Collision collision)
//    {
//        //Debug.Log("����Ѥ������Ƥ�;������;�Ӥ�����;�Ӥ���");
//        ContactPoint contact = collision.contacts[0];
//        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
//        ParticleSystem particle = Instantiate(collisionParticle, contact.point, rotation).GetComponent<ParticleSystem>();
//        // ��ƼŬ ũ�⸦ ������ ����
//       // float particleSize = carController.KPH / 10;

//        // ��ƼŬ �ý����� startSize �Ӽ��� ����Ͽ� ũ�� ����
//        ParticleSystem.MainModule mainModule = particle.main;
//       // mainModule.startSize = particleSize;
//       GameObject hitCanvas =  Instantiate(Hitprefab, contact.point, Quaternion.LookRotation(Camera.main.transform.forward));
//        Destroy(hitCanvas, 1.0f);
//        // particle.Emit(5);
//        // ������ ��ƼŬ�� ���� �ð� �Ŀ� �ı�
//        Destroy(particle, 1.0f);

//    }
//    private void OnCollisionStay(Collision collision)
//    {
//       // Debug.Log("�帣��������");
//    }



   

//    private void ScrollScreechEffect()
//    {
//        // �̹������� �����ϰ� ��ũ��
//        foreach (RectTransform img in images)
//        {
//            // �̹����� �θ� ��ü�κ����� ���� �̵�
//            img.Translate(Vector2.right * scrollSpeed * Time.deltaTime, Space.Self);

//            // �̹����� ���������� �Ѿ�� ���� ������ �̵�
//            if (img.anchoredPosition.x >= screenWidth / 2)
//            {
//               // img.anchoredPosition -= Vector2.right * screenWidth;
//                img.Translate(Vector2.left * screenWidth*2, Space.Self);
//            }
//        }
//    }

//}

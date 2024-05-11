using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarParticle : MonoBehaviour
{
    FinalCarController_June carController;
    [SerializeField] List< ParticleSystem> smoke;
    [SerializeField] GameObject smokePrefab1;
    [SerializeField] GameObject smokePrefab2;

    private void OnEnable()
    {
        carController = gameObject.GetComponent<FinalCarController_June>();

        for (int i = 0; i < carController.wheels.Count; i++)
        {
        smoke.Add( Instantiate(smokePrefab1, carController.wheels[i].wheelCollider.transform.position, Quaternion.identity, carController.wheels[i].wheelCollider.transform).GetComponent<ParticleSystem>()) ;

        }

    }
       [SerializeField] float slipAllowance = 0.2f;
    private void Update()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        for(int i = 0;i < carController.wheels.Count;i++)
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
    }
}

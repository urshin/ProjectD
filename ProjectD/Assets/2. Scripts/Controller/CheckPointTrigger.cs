using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    public List<Collider> colliders;
    private int currentCheckpointIndex = 0;
    private float lowSpeedTime = 0f;
    private const float requiredLowSpeedDuration = 3f;
    CarController_ carController;
    public int lapCount;

    private void OnEnable()
    {
        currentCheckpointIndex = 0;
        lapCount = 0;

        carController = GetComponentInParent<CarController_>();
    }
    private void Update()
    {
        
        if (transform.root.tag == "Enemy")
        {
           
            if (carController != null)
            {
                if (carController.RigidSpeed < 1)
                {
                    lowSpeedTime += Time.deltaTime;
                }
                else
                {
                    lowSpeedTime = 0f; // 타이머 리셋
                }

                if (lowSpeedTime >= requiredLowSpeedDuration&& currentCheckpointIndex>0)
                {
                    carController.GetComponent<Rigidbody>().velocity = Vector3.zero;

                    Respawn();

                    lowSpeedTime = 0f; // 체크포인트 통과시 리셋
                }
            }
        }
        if (transform.root.tag == "Player")
        {

            if (carController != null)
            {
                if (carController.RigidSpeed < 1)
                {
                    lowSpeedTime += Time.deltaTime;
                }
                else
                {
                    lowSpeedTime = 0f; // 타이머 리셋
                }

                if (lowSpeedTime >= requiredLowSpeedDuration)
                {

                    print("Press R to Respawn");
                }


                if(Input.GetKeyDown(KeyCode.R))
                {
                    Respawn();
                    lowSpeedTime = 0f; // 체크포인트 통과시 리셋
                }
            }
        }
    }

    private void Respawn()
    {
        carController.GetComponent<Rigidbody>().velocity = Vector3.zero;

        Vector3 newPosition = colliders[currentCheckpointIndex - 1].transform.position;
        newPosition.x += 4;
        carController.transform.position = newPosition;
        carController.transform.rotation = colliders[currentCheckpointIndex - 1].transform.rotation * Quaternion.Euler(0, 90, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
            if (colliders.Contains(other))
            {
                int index = colliders.IndexOf(other);

                if (index == currentCheckpointIndex)
                {
                    // 정방향
                    currentCheckpointIndex++;
                    Debug.Log("체크포인트 " + currentCheckpointIndex + "통과 완료");

                    // 체크포인트 리셋
                    if (currentCheckpointIndex >= colliders.Count)
                    {
                        Debug.Log("모두 통과");
                        currentCheckpointIndex = 0; 
                    }
                }
                else
                {
                    if(currentCheckpointIndex>=1)
                    {
                        Respawn();


                    }

                    //다른 방향일때
                    Debug.Log("반대!");
                }
            }
        }
    }
}
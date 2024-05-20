using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    public List<Collider> colliders;
    [SerializeField] int currentCheckpointIndex = 0;
    public bool setTrigger;
    private float lowSpeedTime = 0f;
    private const float requiredLowSpeedDuration = 3f;
    CarController_ carController;
    [SerializeField] bool isReverse;
    public int currentLap;
    bool isIngame = false;
    private void OnEnable()
    {
        if (GameManager.Instance.gameState == GameState.InGame)
        {
            isIngame = true;
        }
        else
        {
            isIngame = false;
        }
        

            setTrigger = false;
            isReverse = GameManager.Instance.isReverse;
            if (!isReverse)
            {
                currentCheckpointIndex = 0;
            }
            else if (isReverse)
            {
                currentCheckpointIndex = colliders.Count - 1;
            }
            currentLap = 1;
            carController = GetComponentInParent<CarController_>();
        
    }
    private void Update()
    {
        if (isIngame)
        {
            if (setTrigger)
            {
                if (!isReverse)
                {
                    currentCheckpointIndex = 0;
                }
                else if (isReverse)
                {
                    currentCheckpointIndex = colliders.Count - 1;
                }
                setTrigger = false;
            }
            if (InGameManager.instance.currentState == InGameState.playing)
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

                        if (lowSpeedTime >= requiredLowSpeedDuration && currentCheckpointIndex > 0)
                        {


                            Respawn(isReverse);

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


                        if (Input.GetKeyDown(KeyCode.R))
                        {
                            Respawn(isReverse);
                            lowSpeedTime = 0f; // 체크포인트 통과시 리셋
                        }
                    }
                }
            }
        }

    }

    private void Respawn(bool revers)
    {
        carController.GetComponent<Rigidbody>().velocity = Vector3.zero;
        int look = 90;
        look = revers ? -90 : 90;
        int position = 2;
        position = revers ? -4 : 4;
        int index = 1;
        index = revers ? 1 : -1;

        carController.transform.rotation = colliders[currentCheckpointIndex + index].transform.rotation * Quaternion.Euler(0, look, 0);
        RaycastHit spawnPoint;
        Ray spawnRay = new Ray(colliders[currentCheckpointIndex + index].transform.position+new Vector3(0,2,0), -colliders[currentCheckpointIndex + index].transform.up);
        Physics.Raycast(spawnRay,out spawnPoint, 100, LayerMask.NameToLayer("Ground"));
        Debug.DrawRay(colliders[currentCheckpointIndex + index].transform.position + new Vector3(0, 2, 0), -colliders[currentCheckpointIndex + index].transform.up*100, Color.yellow);

        Vector3 newPosition = spawnPoint.point+new Vector3(0,1,0);
        Vector3 localPosition = carController.transform.InverseTransformPoint(newPosition);
        localPosition.z += position;
        Vector3 finalWorldPosition = carController.transform.TransformPoint(localPosition);
        carController.transform.position = finalWorldPosition;


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
            if (colliders.Contains(other))
            {
                int index = colliders.IndexOf(other);
                if (!isReverse)
                {


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
                            currentLap++;
                            if (transform.root.CompareTag("Player"))
                            {
                                InGameManager.instance.PlayerLap = currentLap;
                                if (currentLap > InGameManager.instance.maxLap)
                                {
                                    InGameManager.instance.OnGameEnd(true);
                                }
                            }
                            else if (transform.root.CompareTag("Enemy"))
                            {
                                InGameManager.instance.AILap = currentLap;
                                if (currentLap > InGameManager.instance.maxLap)
                                {
                                    InGameManager.instance.OnGameEnd(false);
                                }
                            }


                        }
                    }
                    else
                    {
                        if (currentCheckpointIndex >= 1)
                        {
                            Respawn(isReverse);


                        }

                        //다른 방향일때
                        Debug.Log("반대!");
                    }
                }
                else if (isReverse)
                {
                    if (index == currentCheckpointIndex)
                    {
                        // 정방향
                        currentCheckpointIndex--;
                        Debug.Log("체크포인트 " + currentCheckpointIndex + "통과 완료");

                        // 체크포인트 리셋
                        if (currentCheckpointIndex <= 0)
                        {
                            Debug.Log("모두 통과");
                            currentCheckpointIndex = colliders.Count - 1;
                            currentLap++;
                            if (transform.root.CompareTag("Player"))
                            {
                                InGameManager.instance.PlayerLap = currentLap;
                                if (currentLap > InGameManager.instance.maxLap)
                                {
                                    InGameManager.instance.OnGameEnd(true);
                                }
                            }
                            else if (transform.root.CompareTag("Enemy"))
                            {
                                InGameManager.instance.AILap = currentLap;
                                if (currentLap > InGameManager.instance.maxLap)
                                {
                                    InGameManager.instance.OnGameEnd(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentCheckpointIndex >= 1)
                        {
                            Respawn(isReverse);


                        }

                        //다른 방향일때
                        Debug.Log("반대!");
                    }
                }
            }
        }
    }
}
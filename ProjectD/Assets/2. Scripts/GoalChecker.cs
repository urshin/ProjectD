using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour
{
    [SerializeField] bool isReverse;

    private void OnTriggerEnter(Collider other)
    {
        if(isReverse == GameManager.Instance.isReverse)
        {
            if (other.transform.CompareTag("Player"))
            {
                Debug.Log("Player Win");
            }
            else if (other.transform.CompareTag("Enemy"))
            {
                Debug.Log("Enemy Win");
            }
        }
    }
}

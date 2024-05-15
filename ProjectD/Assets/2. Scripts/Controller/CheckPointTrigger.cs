using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [SerializeField] List<Collider> colliders;
    [SerializeField] Collider lastcolliders;
    [SerializeField] Transform lastPosition;
    [SerializeField] int currentCheckpointIndex = 0;

    private void OnEnable()
    {
        currentCheckpointIndex = 0;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
           

        }
    }


}
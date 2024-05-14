using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachinController : MonoBehaviour
{
    [SerializeField] CinemachineStateDrivenCamera cinemachineStateDrivenCamera;
   
    int cameraNum = 1;
    int MaxCamera;
    [SerializeField] Animator cinemachineAnimator;
    public bool isTurbo = false;

    // Start is called before the first frame update
    void Start()
    {
        cinemachineStateDrivenCamera = gameObject.GetComponent<CinemachineStateDrivenCamera>();

    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraNum++;
            if (cameraNum > 3)
            {
                cameraNum = 1;
            }
        }
        
        if (isTurbo)
        {
            if (cameraNum == 1)
            {
                cinemachineAnimator.Play("Turbo1");
            }
            if (cameraNum == 2)
            {
                cinemachineAnimator.Play("Turbo2");
            }
            if (cameraNum == 3)
            {
                cinemachineAnimator.Play("Turbo3");
            }

        }
        else
        {
            if (cameraNum == 1)
            {
                cinemachineAnimator.Play("1stView");
            }
            if (cameraNum == 2)
            {
                cinemachineAnimator.Play("3rdVeiw");
            }
            if (cameraNum == 3)
            {
                cinemachineAnimator.Play("GroundView");
            }

        }

    }

}

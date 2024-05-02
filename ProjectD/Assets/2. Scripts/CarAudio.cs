using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CarAudio : MonoBehaviour
{
    FinalCarController_June CC;
    [Range(1300, 8000)] public float engineValue;
    [SerializeField] float range;

    public bool isSkid = false;


    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource engineSourse;
    [SerializeField] AudioSource WheelSourse;
    [SerializeField] AudioClip hightOn;
    [SerializeField] AudioClip hightOff;
    [SerializeField] AudioClip MedOn;
    [SerializeField] AudioClip MedOff;
    [SerializeField] AudioClip LowOn;
    [SerializeField] AudioClip LowOff;
    [SerializeField] AudioClip maxRPM;
    [SerializeField] AudioClip Idle;
    [SerializeField] AudioClip skid;

    private float lowRpm;
    private float highRpm;

    private void Start()
    {
        CC = GetComponent<FinalCarController_June>();
        lowRpm = (CC.maxRPM / 3) * 1;
        highRpm = (CC.maxRPM / 3) * 2;
        engineSourse.clip = Idle;
        engineSourse.Play();
    }

    private void FixedUpdate()
    {
        // engineValue = CC.currentRPM;
        range = Mathf.Lerp(range, (CC.currentRPM * 2 / CC.maxRPM), 0.2f);

         EngineSound();
        TireSound();
    }


    void EngineSound()
    {
        engineSourse.pitch = 0.5f + 2.0f * (CC.currentRPM - CC.minRPM) / (CC.maxRPM -CC.minRPM);



    }
    public void TireSound()
    {
        if (WheelSourse.clip == null)
            WheelSourse.clip = skid;

        if (isSkid)
        {
            WheelSourse.enabled = true;
        }
        else
        {
            WheelSourse.enabled = false;

        }


    }




}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CarAudio : MonoBehaviour
{
    [Range(1300, 7000)] public float engineValue;
    FinalCarController_June CC;
    [SerializeField] AudioMixer audioMixer;
    
    [SerializeField] AudioSource audioSource;
    private void Start()
    {
        CC= GetComponentInParent<FinalCarController_June>();
        audioSource = GetComponent<AudioSource>();
    }
    private void FixedUpdate()
    {
        //audioSource.pitch = engineValue *3/ 7000;
        engineValue = CC.currentRPM;
        audioMixer.SetFloat("engine1Pitch", engineValue * 3 / 7000);
        audioMixer.SetFloat("engine2Pitch", engineValue * 3 / 7000);

    }
    
}

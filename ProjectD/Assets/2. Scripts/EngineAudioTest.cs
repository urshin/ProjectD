using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

public class EngineAudioTest : MonoBehaviour
{
    //eq
    public SEF_Equalizer eq;

    //

    private GameObject audioObject;
    public FinalCarController_June m_CarController;

    [Range(0, 1)] public float startOffValue = 0.35f;

    public float Load;
    public float loadLerpSpeed = 15;

    public AudioClip lowAccelClip;
    public AudioClip lowDecelClip;
    public AudioClip highAccelClip;
    public AudioClip highDecelClip;
    [Header("Tubro Sound")]
    public AudioClip Turbo;
    [Range(0, 2)] public float turboVolume;


    [Header("Pitch")]
    [Range(0.5f, 1)] public float Pitch = 1f;
    [Range(.5f, 3)] public float lowPitchMin = 1f;
    [Range(2, 7)] public float lowPitchMax = 6f;
    [Range(0, 1)] public float highPitchMultiplier = 0.25f;
    [Range(0, 1)] public float pitchMultiplier = 1f;


    private float accFade = 0;
    private float acceleration;
    private float maxRolloffDistance = 500;
    [SerializeField] AudioSource m_LowAccel;
    [SerializeField] AudioSource m_LowDecel;
    [SerializeField] AudioSource m_HighAccel;
    [SerializeField] AudioSource m_HighDecel;
    [SerializeField] AudioSource m_Turbo;


    private void Start()
    {


        m_HighAccel = SetUpEngineAudioSource(m_HighAccel,highAccelClip);
        m_LowAccel = SetUpEngineAudioSource(m_LowAccel, lowAccelClip);
        m_LowDecel = SetUpEngineAudioSource(m_LowDecel, lowDecelClip);
        m_HighDecel = SetUpEngineAudioSource(m_HighDecel, highDecelClip);
        if (Turbo != null) m_Turbo = SetUpEngineAudioSource(m_Turbo,Turbo);


        eq = gameObject.AddComponent<SEF_Equalizer>();
        m_CarController = transform.root.gameObject.GetComponent<FinalCarController_June>();


        lowPitchMax = (m_CarController.maxRPM / 1000) / 2;

    }

    void filter()
    {
        Load = accFade;
        //Load =  Mathf.Abs(IN.vertical) ; 
        eq.midFreq = Mathf.Lerp(eq.midFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
        eq.highFreq = Mathf.Lerp(eq.highFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
    }


    private void FixedUpdate()
    {

        accFade = Mathf.Lerp(accFade, Mathf.Abs(acceleration), 15 * Time.deltaTime);

        if (m_CarController.gasInput > 0 )
            acceleration = 1;
        else acceleration = 0;

        float pitch = ULerp(lowPitchMin, lowPitchMax, m_CarController.currentRPM / m_CarController.maxRPM);
        pitch = Mathf.Min(lowPitchMax, pitch);
        m_LowAccel.pitch = pitch * pitchMultiplier;
        m_LowDecel.pitch = pitch * pitchMultiplier;
        m_HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier;
        m_HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier;

        float decFade = 1 - accFade;
        float highFade = Mathf.InverseLerp(0.2f, 0.8f, m_CarController.currentRPM / m_CarController.maxRPM);
        float lowFade = 1 - highFade;

        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        //accFade = 1 - ((1 - accFade)*(1 - accFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));
        m_LowAccel.volume = lowFade * accFade;
        m_LowDecel.volume = lowFade * decFade;
        m_HighAccel.volume = highFade * accFade;
        m_HighDecel.volume = highFade * decFade;


        filter();
    }

    private AudioSource SetUpEngineAudioSource(AudioSource source, AudioClip clip)
    {
        //AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0;
        source.spatialBlend = 1;
        source.loop = true;
        source.dopplerLevel = 0;
        source.time = Random.Range(0f, clip.length);
        source.Play();
        source.minDistance = 5;
        source.maxDistance = maxRolloffDistance;
        return source;
    }

    private float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }



    ////테스트용입니다~~ 왜 안돼!!!!!!!!
    // FinalCarController_June cc;
    // private AudioSource audioSource;
    //// private Car car;
    // private int audioPosition;
    // //[Range(0, 1000)] public int engineValue;
    // float[] explosionData;
    // int sampleSize;
    // int explosionSampleSize;

    // public AudioClip explosionClip;
    // public float explosionVolume = 1f;
    // public float frequency = 1f;

    // void Awake()
    // {
    //     audioSource = GetComponent<AudioSource>();
    //    // car = GetComponent<Car>();
    //    cc = GetComponentInParent<FinalCarController_June>();
    //     explosionSampleSize = explosionClip.samples * explosionClip.channels;
    //     explosionData = new float[explosionSampleSize];
    //     explosionClip.GetData(explosionData, 0);

    //     sampleSize = explosionClip.samples * 2;

    //     AudioClip engine = AudioClip.Create(
    //         "Engine", explosionClip.samples * 2, explosionClip.channels, explosionClip.frequency, true
    //     );
    //     audioSource.clip = engine;
    // }

    // void Update()
    // {
    //     frequency = (int)(Mathf.Clamp01(cc.currentRPM / cc.maxRPM) * 100f)+1;

    //     if (!audioSource.isPlaying)
    //     {
    //         audioSource.Play();
    //     }
    // }

    // float SampleExplosion(int pos)
    // {
    //     float total = 0f;

    //     int start = pos % sampleSize;

    //     int spacing = Mathf.CeilToInt(sampleSize / frequency);

    //     for (int i = 1; i < sampleSize; i += spacing)
    //     {
    //         int j = (i + start) % explosionSampleSize;
    //         int idx = (i + start) / explosionSampleSize;

    //         total += explosionData[j] * explosionVolume;
    //     }

    //     return total;
    // }

    // void OnAudioFilterRead(float[] data, int channels)
    // {
    //     for (int i = 0; i < data.Length; i += 2)
    //     {
    //         data[i] = 0;
    //         data[i + 1] = 0;

    //         int off = audioPosition + i / 2;

    //         float e = SampleExplosion(off);

    //         data[i] += e;
    //         data[i + 1] += e;
    //     }

    //     audioPosition += data.Length / 2;
    // }
}
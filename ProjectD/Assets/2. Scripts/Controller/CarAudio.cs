using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CarAudio : MonoBehaviour
{

    // 타이어 슬립 관련 변수
    public bool isSkid = false;
    public float tireSlipAmount;

    [SerializeField] AudioMixer audioMixer; // AudioMixer에 대한 참조
    [SerializeField] AudioSource WheelSourse; // 타이어 사운드 AudioSource
    [SerializeField] AudioClip skid; // 슬립 사운드 클립
    //eq
    public SEF_Equalizer eq; // SEF_Equalizer 컴포넌트에 대한 참조

    // FinalCarController_June 및 cc 변수
    //public FinalCarController_June cc;
    public CarController cc;

    // 엔진 시작 및 로드 변수
    [Range(0, 1)] public float startOffValue = 0.35f;
    public float Load;
    public float loadLerpSpeed = 15;

    // 엔진 사운드 클립 변수
    public AudioClip lowAccelClip;
    public AudioClip lowDecelClip;
    public AudioClip highAccelClip;
    public AudioClip highDecelClip;

    [Header("Tubro Sound")]
    public AudioClip Turbo; // 터보 사운드 클립
    [Range(0, 2)] public float turboVolume; // 터보 볼륨 조절 변수

    // 피치 관련 변수
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
        // FinalCarController_June 및 AudioSource 설정
        m_HighAccel = SetUpEngineAudioSource(m_HighAccel, highAccelClip);
        m_LowAccel = SetUpEngineAudioSource(m_LowAccel, lowAccelClip);
        m_LowDecel = SetUpEngineAudioSource(m_LowDecel, lowDecelClip);
        m_HighDecel = SetUpEngineAudioSource(m_HighDecel, highDecelClip);
        //if (Turbo != null) m_Turbo = SetUpEngineAudioSource(m_Turbo, Turbo);

        // EQ 컴포넌트 설정
        eq = gameObject.AddComponent<SEF_Equalizer>();
        //cc = gameObject.GetComponent<FinalCarController_June>();
        cc = gameObject.GetComponent<CarController>();

        // lowPitchMax 설정
        lowPitchMax = (cc.maxRPM / 1000) / 2;
    }

    private void FixedUpdate()
    {
        // 엔진 및 타이어 사운드 재생
        if (cc.initialized)
        {
            EngineSound();
        }
        TireSound();
    }

    // EQ 필터링 함수
    void filter()
    {
        Load = accFade;
        eq.midFreq = Mathf.Lerp(eq.midFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
        eq.highFreq = Mathf.Lerp(eq.highFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
    }
    // 엔진 사운드 조절 함수

    void EngineSound()
    {
        // 가속도 및 피치 조절
        // accFade 값 보간: 현재 가속도에 따라 accFade 값을 조절하여 부드러운 효과를 줍니다.
        // 가속도가 크면 accFade는 1에 가깝고, 가속도가 작으면 0에 가까워집니다.
        accFade = Mathf.Lerp(accFade, Mathf.Abs(acceleration), 15 * Time.deltaTime);

        // 가속도 계산: 차량의 가스 입력(cc.gasInput)에 따라 가속도를 설정합니다.
        // 가스 입력이 있으면 가속도는 1, 없으면 0이 됩니다.
        //if (cc.gasInput > 0)
        //    acceleration = 1;
        //else
        //    acceleration = 0;

        acceleration = cc.currentRPM / cc.maxRPM;
        // 피치 계산: 엔진 RPM을 기반으로 저음 피치와 고음 피치를 설정합니다.
        float pitch = ULerp(lowPitchMin, lowPitchMax, cc.currentRPM / cc.maxRPM);
        pitch = Mathf.Min(lowPitchMax, pitch);
        m_LowAccel.pitch = pitch * pitchMultiplier; // 낮은 RPM 범위 내에서 엔진 소리 피치 설정
        m_LowDecel.pitch = pitch * pitchMultiplier; // 낮은 RPM 범위 내에서 엔진 소리 피치 설정
        m_HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier; // 높은 RPM 범위 내에서 엔진 소리 피치 설정
        m_HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier; // 높은 RPM 범위 내에서 엔진 소리 피치 설정

        // 각각의 Fade 값 계산: 엔진 소리의 볼륨을 조절하기 위한 Fade 값 계산
        float decFade = 1 - accFade;
        float highFade = Mathf.InverseLerp(0.2f, 0.8f, (cc.currentRPM / cc.maxRPM));
        float lowFade = 1 - highFade;

        // Fade 값 보간: 부드럽게 변하는 Fade 값을 위해 각 Fade 값을 제곱하여 보정합니다.
        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        // 각 AudioSource의 볼륨 설정: 각 상황에 따라 엔진 소리의 볼륨을 조절합니다.
        m_LowAccel.volume = (lowFade * accFade)/2+0.4f; // 낮은 RPM 범위 내에서 가속 중일 때의 볼륨
        m_LowDecel.volume = (lowFade * decFade)/2+0.4f; // 낮은 RPM 범위 내에서 감속 중일 때의 볼륨
        m_HighAccel.volume = Mathf.Lerp(m_HighAccel.volume, highFade * accFade, Time.deltaTime * 5); // 높은 RPM 범위 내에서 가속 중일 때의 볼륨
        m_HighDecel.volume = Mathf.Lerp(m_HighDecel.volume, highFade * decFade-0.3f, Time.deltaTime * 5); // 높은 RPM 범위 내에서 감속 중일 때의 볼륨
        // EQ 필터링
        filter();
    }

    // 엔진 사운드 AudioSource 설정 함수
    private AudioSource SetUpEngineAudioSource(AudioSource source, AudioClip clip)
    {
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

    // 선형 보간 함수
    private float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

  

    // 타이어 사운드 조절 함수
    public void TireSound()
    {
        if (WheelSourse.clip == null)
            WheelSourse.clip = skid;
        WheelSourse.volume = 0.5f + tireSlipAmount / 3;
        if (isSkid)
        {
            WheelSourse.enabled = true;
        }
        else
        {
            WheelSourse.enabled = false;
        }
    }
    
    public void TurboOn()
    {
        m_Turbo.volume = 0.5f;
        m_Turbo.Play();
    }
}
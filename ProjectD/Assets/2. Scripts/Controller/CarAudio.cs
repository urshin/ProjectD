using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class CarAudio : MonoBehaviour
{

    // Ÿ�̾� ���� ���� ����
    public bool isSkid = false;
    public float tireSlipAmount;

    [SerializeField] AudioMixer audioMixer; // AudioMixer�� ���� ����
    [SerializeField] AudioSource WheelSourse; // Ÿ�̾� ���� AudioSource
    [SerializeField] AudioClip skid; // ���� ���� Ŭ��
    //eq
    public SEF_Equalizer eq; // SEF_Equalizer ������Ʈ�� ���� ����

    // FinalCarController_June �� cc ����
    //public FinalCarController_June cc;
    public CarController cc;

    // ���� ���� �� �ε� ����
    [Range(0, 1)] public float startOffValue = 0.35f;
    public float Load;
    public float loadLerpSpeed = 15;

    // ���� ���� Ŭ�� ����
    public AudioClip lowAccelClip;
    public AudioClip lowDecelClip;
    public AudioClip highAccelClip;
    public AudioClip highDecelClip;

    [Header("Tubro Sound")]
    public AudioClip Turbo; // �ͺ� ���� Ŭ��
    [Range(0, 2)] public float turboVolume; // �ͺ� ���� ���� ����

    // ��ġ ���� ����
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
        // FinalCarController_June �� AudioSource ����
        m_HighAccel = SetUpEngineAudioSource(m_HighAccel, highAccelClip);
        m_LowAccel = SetUpEngineAudioSource(m_LowAccel, lowAccelClip);
        m_LowDecel = SetUpEngineAudioSource(m_LowDecel, lowDecelClip);
        m_HighDecel = SetUpEngineAudioSource(m_HighDecel, highDecelClip);
        //if (Turbo != null) m_Turbo = SetUpEngineAudioSource(m_Turbo, Turbo);

        // EQ ������Ʈ ����
        eq = gameObject.AddComponent<SEF_Equalizer>();
        //cc = gameObject.GetComponent<FinalCarController_June>();
        cc = gameObject.GetComponent<CarController>();

        // lowPitchMax ����
        lowPitchMax = (cc.maxRPM / 1000) / 2;
    }

    private void FixedUpdate()
    {
        // ���� �� Ÿ�̾� ���� ���
        if (cc.initialized)
        {
            EngineSound();
        }
        TireSound();
    }

    // EQ ���͸� �Լ�
    void filter()
    {
        Load = accFade;
        eq.midFreq = Mathf.Lerp(eq.midFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
        eq.highFreq = Mathf.Lerp(eq.highFreq, startOffValue + (Load / 1.5f), loadLerpSpeed * Time.deltaTime);
    }
    // ���� ���� ���� �Լ�

    void EngineSound()
    {
        // ���ӵ� �� ��ġ ����
        // accFade �� ����: ���� ���ӵ��� ���� accFade ���� �����Ͽ� �ε巯�� ȿ���� �ݴϴ�.
        // ���ӵ��� ũ�� accFade�� 1�� ������, ���ӵ��� ������ 0�� ��������ϴ�.
        accFade = Mathf.Lerp(accFade, Mathf.Abs(acceleration), 15 * Time.deltaTime);

        // ���ӵ� ���: ������ ���� �Է�(cc.gasInput)�� ���� ���ӵ��� �����մϴ�.
        // ���� �Է��� ������ ���ӵ��� 1, ������ 0�� �˴ϴ�.
        //if (cc.gasInput > 0)
        //    acceleration = 1;
        //else
        //    acceleration = 0;

        acceleration = cc.currentRPM / cc.maxRPM;
        // ��ġ ���: ���� RPM�� ������� ���� ��ġ�� ���� ��ġ�� �����մϴ�.
        float pitch = ULerp(lowPitchMin, lowPitchMax, cc.currentRPM / cc.maxRPM);
        pitch = Mathf.Min(lowPitchMax, pitch);
        m_LowAccel.pitch = pitch * pitchMultiplier; // ���� RPM ���� ������ ���� �Ҹ� ��ġ ����
        m_LowDecel.pitch = pitch * pitchMultiplier; // ���� RPM ���� ������ ���� �Ҹ� ��ġ ����
        m_HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier; // ���� RPM ���� ������ ���� �Ҹ� ��ġ ����
        m_HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier; // ���� RPM ���� ������ ���� �Ҹ� ��ġ ����

        // ������ Fade �� ���: ���� �Ҹ��� ������ �����ϱ� ���� Fade �� ���
        float decFade = 1 - accFade;
        float highFade = Mathf.InverseLerp(0.2f, 0.8f, (cc.currentRPM / cc.maxRPM));
        float lowFade = 1 - highFade;

        // Fade �� ����: �ε巴�� ���ϴ� Fade ���� ���� �� Fade ���� �����Ͽ� �����մϴ�.
        highFade = 1 - ((1 - highFade) * (1 - highFade));
        lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
        decFade = 1 - ((1 - decFade) * (1 - decFade));

        // �� AudioSource�� ���� ����: �� ��Ȳ�� ���� ���� �Ҹ��� ������ �����մϴ�.
        m_LowAccel.volume = (lowFade * accFade)/2+0.4f; // ���� RPM ���� ������ ���� ���� ���� ����
        m_LowDecel.volume = (lowFade * decFade)/2+0.4f; // ���� RPM ���� ������ ���� ���� ���� ����
        m_HighAccel.volume = Mathf.Lerp(m_HighAccel.volume, highFade * accFade, Time.deltaTime * 5); // ���� RPM ���� ������ ���� ���� ���� ����
        m_HighDecel.volume = Mathf.Lerp(m_HighDecel.volume, highFade * decFade-0.3f, Time.deltaTime * 5); // ���� RPM ���� ������ ���� ���� ���� ����
        // EQ ���͸�
        filter();
    }

    // ���� ���� AudioSource ���� �Լ�
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

    // ���� ���� �Լ�
    private float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

  

    // Ÿ�̾� ���� ���� �Լ�
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
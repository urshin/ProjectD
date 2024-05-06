using System.Collections;
using UnityEngine;

public class EngineAudioTest : MonoBehaviour
{




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
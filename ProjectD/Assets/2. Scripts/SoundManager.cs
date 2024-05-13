using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    static SoundManager uniqueInstance;
    public static SoundManager instance
    {
        get { return uniqueInstance; }
    }

    // true�� bgm, false�� sfx�� ����
    Dictionary<bool, Dictionary<int, AudioClip>> allClips;

    AudioSource bgmPlayer;
    AudioSource sfxPlayer;
    public float bgmVolume;
    public float sfxVolume;

    bool bgmFadeIn;
    bool bgmFadeOut;
    int currentBGM;

    public int CurrentBgm 
    {
        get { return currentBGM; }
    }

    // �� bgm ����� �ҽ��� �̸��� DataManager.Instance.bgmDictionary[] ���� �ε����� ������ �� �ִ�, �̸� Resources/Sound/BGM/ ���� �̸����� ã�� �������� �ȴ�


    private void Awake()
    {
        uniqueInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (bgmFadeOut)
        {
            if (bgmPlayer.volume > 0)
            {
                bgmPlayer.volume -= 0.1f * Time.deltaTime;
            }
            else
            {
                bgmPlayer.volume = 0;
                bgmFadeOut = false;
            }
        }

        if (bgmFadeIn && !bgmFadeOut)
        {
            if (bgmPlayer.volume < bgmVolume)
            {
                bgmPlayer.volume += 0.1f * Time.deltaTime;
            }
            else
            {
                bgmPlayer.volume = bgmVolume;
                bgmFadeIn = false;
            }
        }
    }

    public void InitSoundData()
    {
        allClips = new Dictionary<bool, Dictionary<int, AudioClip>>();

        {
            Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
            int count = DataManager.Instance.bgmDictionary.Count;
            for (int i = 0; i < count; i++)
            {
                AudioClip clip = Resources.Load<AudioClip>("Sound\\BGM\\" + DataManager.Instance.bgmDictionary[i]);
                clips.Add(i, clip);
            }
            allClips.Add(true, clips);  // BGM ����
        }

        {
            //Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
            //int count = DataManager.Instance.bgmDictionary.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    AudioClip clip = Resources.Load<AudioClip>("Sound\\SFX\\" + DataManager.Instance.bgmDictionary[i]);
            //    clips.Add(i, clip);
            //}
            //allClips.Add(false, clips);  // SFX ����
        }

        // �ڱ� �ڽ��� BGM �÷��̾ �ǰ� �ڽ����� SFX �÷��̾ �����Ͽ� �ٴ´�
        GameObject go = new GameObject("SFXPlayer", typeof(AudioSource));
        go.transform.SetParent(transform);
        sfxPlayer = go.GetComponent<AudioSource>();
        bgmPlayer = gameObject.AddComponent<AudioSource>();

        bgmPlayer.playOnAwake = false;
        sfxPlayer.playOnAwake = false;
        bgmPlayer.volume = 0.3f;        // ���� ������ ���߿� �ɼ����� �� �� ���� �Լ��� ���� �����޾ƾ� �Ѵ�
        sfxPlayer.volume = 0.3f;          // �߰������� �̴� ���Ͽ� ����Ǿ�� �ϰ� ���� ���ට ������ �ɼ����� ����Ǿ�� ��
        bgmVolume = 0.3f;
        sfxVolume = 0.3f;
        bgmPlayer.loop = true;
    }

    public void SetBGMVolume(float volume)
    {
        bgmPlayer.volume = volume;
        bgmVolume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        sfxPlayer.volume = volume;
        sfxVolume = volume;
    }

    public void BGMFadeIn()
    {
        bgmPlayer.volume = 0;
        bgmFadeIn = true;
    }
    public void BGMFadeOut()
    {
        bgmFadeOut = true;
    }

    public void PlayBGM(int index)
    {
        AudioClip clip = allClips[true][index];
        bgmPlayer.clip = clip;

        bgmPlayer.Play();
        currentBGM = index;
    }
    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void PlaySFX(int index)
    {
        AudioClip clip = allClips[false][index];
        sfxPlayer.PlayOneShot(clip);
    }


}
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

    // true는 bgm, false는 sfx를 저장
    Dictionary<bool, Dictionary<int, AudioClip>> allClips;

    AudioSource bgmPlayer;
    AudioSource sfxPlayer;

    bool bgmFadeIn;
    bool bgmFadeOut;

    // 각 bgm 오디오 소스의 이름은 DataManager.Instance.bgmDictionary[] 에서 인덱스로 가져올 수 있다, 이를 Resources/Sound/BGM/ 에서 이름으로 찾아 가져오면 된다


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
                bgmPlayer.volume -= 0.2f * Time.deltaTime;
            }
            else
            {
                bgmPlayer.volume = 0;
                bgmFadeOut = false;
            }
        }

        if (bgmFadeIn && !bgmFadeOut)
        {
            if (bgmPlayer.volume < 0.7f)
            {
                bgmPlayer.volume += 0.2f * Time.deltaTime;
            }
            else
            {
                bgmPlayer.volume = 0.7f;
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
            allClips.Add(true, clips);  // BGM 저장
        }

        {
            //Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
            //int count = DataManager.Instance.bgmDictionary.Count;
            //for (int i = 0; i < count; i++)
            //{
            //    AudioClip clip = Resources.Load<AudioClip>("Sound\\SFX\\" + DataManager.Instance.bgmDictionary[i]);
            //    clips.Add(i, clip);
            //}
            //allClips.Add(false, clips);  // SFX 저장
        }

        // 자기 자신이 BGM 플레이어가 되고 자식으로 SFX 플레이어를 생성하여 붙는다
        GameObject go = new GameObject("SFXPlayer", typeof(AudioSource));
        go.transform.SetParent(transform);
        sfxPlayer = go.GetComponent<AudioSource>();
        bgmPlayer = gameObject.AddComponent<AudioSource>();

        bgmPlayer.playOnAwake = false;
        sfxPlayer.playOnAwake = false;
        bgmPlayer.volume = 0.7f;        // 볼륨 조절은 나중에 옵션으로 뺀 후 따로 함수를 만들어서 조절받아야 한다
        sfxPlayer.volume = 0.7f;
        bgmPlayer.loop = true;
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

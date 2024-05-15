using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public struct GameOption
{
    public float sensitivity;
    public float mainVolume;
    public float bgmVolume;
    public float sfxVolume;
    public bool autoCounter;
}


public class UserInfoManager : MonoBehaviour
{
    static UserInfoManager uniqueInstance;
    public static UserInfoManager instance
    {
        get { return uniqueInstance; }
    }


    [Serializable]
    struct SaveData
    {
        public int cash;
        public int purchasedCar;
        public int ownMusic;
        public GameOption option;

        //1맵 3계절
        //Dictionary<int, float> data; key -> weather*100 + map ;  value -> LapTime;
        public Dictionary<int, float> lapTime;
    }

    // 보유한 차량, 음악은 정수로 저장하는데, 이는 비트 연산자로 소유 여부를 판가름하기 위함
    // 인덱스 0번의 차량을 소유했다면 0번째 비트가 1이 되는 식

    string fileName = "SaveData.json";

    SaveData data;
    GameOption option;
    int cash;
    int purchasedCar;
    int ownMusic;
    Dictionary<int, float> lapTime;

    // 수정이 불가능하고 참조만 할 수 있도록 프로퍼티
    public GameOption Option
    {
        get { return option; }
    }
    public int Cash
    {
        get { return cash; }
    }
    public int PurcashedCar
    {
        get { return purchasedCar; }
    }
    public int OwnMusic
    {
        get { return ownMusic; }
    }
    public Dictionary<int, float> LapTime
    {
        get { return lapTime; }
    }

    private void Awake()
    {
        uniqueInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitUserInfo()
    {
        // 세이브 파일 데이터를 읽어 유저 정보를 설정
        if (LoadGameData())
        {
            cash =  data.cash;
            purchasedCar = data.purchasedCar;
            ownMusic = data.ownMusic;
            option = data.option;
            lapTime = data.lapTime;
        }
        else
        {
            // LoadGameData()에서 파일을 찾지 못하면, 즉 저장 데이터가 없으면 아래와 같이 초기화
            cash = 1000;
            purchasedCar = 0;
            ownMusic = 15;
            option.sensitivity = 30;
            option.mainVolume = 0.5f;
            option.bgmVolume = 0.5f;
            option.sfxVolume = 0.5f;
            option.autoCounter = true;
            lapTime = new Dictionary<int, float>();
        }

        SceneControlManager.Instance.StartLobbyScene();
    }

    public void BuyObject(int carIndex, int price)
    {
        if ((purchasedCar & (1 << carIndex)) <= 0)
        {
            purchasedCar += 1 << carIndex;
        }

        cash -= price;
        SaveGameData();
    }

    public void GetMusic(int musicIndex)
    {
        if ((ownMusic & (1 << musicIndex)) <= 0)
        {
            ownMusic += 1 << musicIndex;
        }

        SaveGameData();
    }

    public void SaveOption(float sens, float mainVol, float bgmVol, float sfxVol, bool counter)
    {
        option.sensitivity = sens;
        option.mainVolume = mainVol;
        option.bgmVolume = bgmVol;
        option.sfxVolume = sfxVol;
        option.autoCounter = counter;

        SaveGameData();
    }

    public void AddCash(int earnedCash)
    {
        cash += earnedCash;
        SaveGameData();
    }

    public void OnGameEnd(int earnedCash, int mapIndex, int weatherIndex, float time = 0)
    {
        cash += earnedCash;
        if(lapTime.ContainsKey(weatherIndex * 100 + mapIndex))
        {
            lapTime[weatherIndex * 100 + mapIndex] = time;
        }
        else
        {
            lapTime.Add(weatherIndex * 100 + mapIndex, time);
        }

        SaveGameData();
    }

    public void SaveGameData()
    {
        data.cash = cash;
        data.purchasedCar = purchasedCar;
        data.ownMusic = ownMusic;
        data.option = option;
        data.lapTime = lapTime;

        string filePath = Application.persistentDataPath + "/" + fileName;
        string jsonData = JsonConvert.SerializeObject(data);
        //string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonData);
    }

    bool LoadGameData()
    {
        string filePath = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(filePath))
        {
            // 불러오기
            string jsonData = File.ReadAllText(filePath);
            data = (SaveData)JsonConvert.DeserializeObject(jsonData, typeof(SaveData));
            //data = JsonUtility.FromJson<SaveData>(jsonData);
            return true;
        }

        return false;
    }
}

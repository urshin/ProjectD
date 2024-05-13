using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserInfoManager : MonoBehaviour
{
    static UserInfoManager uniqueInstance;
    public UserInfoManager instance
    {
        get { return uniqueInstance; }
    }

    [Serializable]
    struct SaveData
    {
        public int cash;
        public int purchasedCar;
        public int ownMusic;
        

        //option
        //sensitive
        //main volume
        //bgm
        //effect
        //autoSteer?

        //1맵 3계절
        //Dictionary<int, float> data; key -> weather*100 + map ;  value -> LapTime;


    }

    // 보유한 차량, 음악은 정수로 저장하는데, 이는 비트 연산자로 소유 여부를 판가름하기 위함
    // 인덱스 0번의 차량을 소유했다면 0번째 비트가 1이 되는 식

    string fileName = "SaveData.json";

    SaveData data;
    int cash;
    int purchasedCar;
    int ownMusic;

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
        }
        else
        {
            // LoadGameData()에서 파일을 찾지 못하면, 즉 저장 데이터가 없으면 아래와 같이 초기화
            cash = 1000;
            purchasedCar = 0;
            ownMusic = 15;
        }
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


    public void AddCash(int earnedCash)
    {
        cash += earnedCash;
        SaveGameData();
    }

    public void OnGameEnd(int earnedCash, int mapIndex, float lapTime)
    {
        // ??
    }

    public void SaveGameData()
    {
        data.cash = cash;
        data.purchasedCar = purchasedCar;
        data.ownMusic = ownMusic;

        string filePath = Application.persistentDataPath + "/" + fileName;
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonData);
    }

    bool LoadGameData()
    {
        string filePath = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(filePath))
        {
            // 불러오기
            string jsonData = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SaveData>(jsonData);
            return true;
        }

        return false;
    }
}

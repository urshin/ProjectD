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

        //1�� 3����
        //Dictionary<int, float> data; key -> weather*100 + map ;  value -> LapTime;


    }

    // ������ ����, ������ ������ �����ϴµ�, �̴� ��Ʈ �����ڷ� ���� ���θ� �ǰ����ϱ� ����
    // �ε��� 0���� ������ �����ߴٸ� 0��° ��Ʈ�� 1�� �Ǵ� ��

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
        // ���̺� ���� �����͸� �о� ���� ������ ����
        if (LoadGameData())
        {
            cash =  data.cash;
            purchasedCar = data.purchasedCar;
            ownMusic = data.ownMusic;
        }
        else
        {
            // LoadGameData()���� ������ ã�� ���ϸ�, �� ���� �����Ͱ� ������ �Ʒ��� ���� �ʱ�ȭ
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
            // �ҷ�����
            string jsonData = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SaveData>(jsonData);
            return true;
        }

        return false;
    }
}

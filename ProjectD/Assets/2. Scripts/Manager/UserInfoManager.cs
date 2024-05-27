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
    public SpeeoMeterState speeoMeter;
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

        //1�� 3����
        //Dictionary<int, float> data; key -> weather*100 + map ;  value -> LapTime;
        // ����Ʈ���� �����ϴ� ���ÿ� �ֱ� 10���� ��Ÿ���� �����ؾ� �Ѵ�
        // ��� ������ List<int>�� �����ϰ� ����Ʈ�� ũ�⸦ 10���� �Ѵ�
        public Dictionary<int, List<float>> lapTime;
        public Dictionary<int, float> bestLapTime;
    }

    // ������ ����, ������ ������ �����ϴµ�, �̴� ��Ʈ �����ڷ� ���� ���θ� �ǰ����ϱ� ����
    // �ε��� 0���� ������ �����ߴٸ� 0��° ��Ʈ�� 1�� �Ǵ� ��

    string fileName = "SaveData.json";

    SaveData data;
    GameOption option;
    int cash;
    int purchasedCar;
    int ownMusic;
    Dictionary<int, List<float>> lapTime;
    Dictionary<int, float> bestLapTime;

    // ������ �Ұ����ϰ� ������ �� �� �ֵ��� ������Ƽ
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
    public Dictionary<int, List<float>> LapTime
    {
        get { return lapTime; }
    }
    public Dictionary<int, float> BestLapTime
    {
        get { return bestLapTime; }
    }

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
            cash = data.cash;
            purchasedCar = data.purchasedCar;
            ownMusic = data.ownMusic;
            option = data.option;
            lapTime = data.lapTime;
            bestLapTime = data.bestLapTime;
        }
        else
        {
            // LoadGameData()���� ������ ã�� ���ϸ�, �� ���� �����Ͱ� ������ �Ʒ��� ���� �ʱ�ȭ
            cash = 1000;
            purchasedCar = 0;
            ownMusic = 15;
            option.sensitivity = 30;
            option.mainVolume = 0.5f;
            option.bgmVolume = 0.5f;
            option.sfxVolume = 0.5f;
            option.autoCounter = true;
            option.speeoMeter = SpeeoMeterState.TacoMeter;
            lapTime = new Dictionary<int, List<float>>();
            bestLapTime = new Dictionary<int, float>();
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
    //�����ε�
    public void SaveOption(float sens, float mainVol, float bgmVol, float sfxVol, bool counter, SpeeoMeterState gage)
    {
        option.sensitivity = sens;
        option.mainVolume = mainVol;
        option.bgmVolume = bgmVol;
        option.sfxVolume = sfxVol;
        option.autoCounter = counter;
        option.speeoMeter = gage;
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
        if (lapTime.ContainsKey(weatherIndex * 100 + mapIndex))
        {
            int count = lapTime[weatherIndex * 100 + mapIndex].Count;
            if (count > 9)
                count = 9;
            else
            {
                // ī��Ʈ�� �ִ밪�� 10�� �ƴ϶�� ���� i+1���� ������ �߻��ϹǷ� �����Ⱚ�� �־� ó���ǵ��� �Ѵ�
                lapTime[weatherIndex * 100 + mapIndex].Add(0);
            }


            for (int i = count - 1; i >= 0; i--)
            {
                lapTime[weatherIndex * 100 + mapIndex][i + 1] = lapTime[weatherIndex * 100 + mapIndex][i];
            }

            lapTime[weatherIndex * 100 + mapIndex][0] = time;
            if (bestLapTime[weatherIndex * 100 + mapIndex] > time)
            {
                bestLapTime[weatherIndex * 100 + mapIndex] = time;
            }
        }
        else
        {
            List<float> temp = new List<float>();
            temp.Add(time);

            // ����� �����ٸ� ������ �ű���̹Ƿ� ����Ʈ������ �����Ѵ�
            bestLapTime[weatherIndex * 100 + mapIndex] = time;
            lapTime.Add(weatherIndex * 100 + mapIndex, temp);
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
        data.bestLapTime = bestLapTime;

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
            // �ҷ�����
            string jsonData = File.ReadAllText(filePath);
            data = (SaveData)JsonConvert.DeserializeObject(jsonData, typeof(SaveData));
            //data = JsonUtility.FromJson<SaveData>(jsonData);
            return true;
        }

        return false;
    }
}

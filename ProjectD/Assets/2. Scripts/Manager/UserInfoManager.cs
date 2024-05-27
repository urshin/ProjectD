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

        //1맵 3계절
        //Dictionary<int, float> data; key -> weather*100 + map ;  value -> LapTime;
        // 베스트랩을 보관하는 동시에 최근 10번의 랩타임을 저장해야 한다
        // 밸류 값으로 List<int>를 저장하고 리스트의 크기를 10으로 한다
        public Dictionary<int, List<float>> lapTime;
        public Dictionary<int, float> bestLapTime;
    }

    // 보유한 차량, 음악은 정수로 저장하는데, 이는 비트 연산자로 소유 여부를 판가름하기 위함
    // 인덱스 0번의 차량을 소유했다면 0번째 비트가 1이 되는 식

    string fileName = "SaveData.json";

    SaveData data;
    GameOption option;
    int cash;
    int purchasedCar;
    int ownMusic;
    Dictionary<int, List<float>> lapTime;
    Dictionary<int, float> bestLapTime;

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
        // 세이브 파일 데이터를 읽어 유저 정보를 설정
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
            // LoadGameData()에서 파일을 찾지 못하면, 즉 저장 데이터가 없으면 아래와 같이 초기화
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
    //오버로딩
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
                // 카운트가 최대값인 10이 아니라면 밑의 i+1에서 오류가 발생하므로 쓰레기값을 넣어 처리되도록 한다
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

            // 기록이 없었다면 무조건 신기록이므로 베스트랩으로 저장한다
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
            // 불러오기
            string jsonData = File.ReadAllText(filePath);
            data = (SaveData)JsonConvert.DeserializeObject(jsonData, typeof(SaveData));
            //data = JsonUtility.FromJson<SaveData>(jsonData);
            return true;
        }

        return false;
    }
}

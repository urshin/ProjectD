using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using AutoMoverPro;
using UnityEngine.SceneManagement;


public class GarageManager : MonoBehaviour
{
    static GarageManager uniqueInstance;
    public static GarageManager instace
    {
        get { return uniqueInstance; }
    }

    [SerializeField] GameObject startBlackPanel;
    [SerializeField] GameObject carSpawnPosition;
    [SerializeField] GameObject currentCar;
    [SerializeField] int carIndex = 0;
    [SerializeField] int carIndexMax;
    [SerializeField] TextMeshProUGUI carInfoTextLeft;
    [SerializeField] TextMeshProUGUI carInfoTextRight;
    //public TextAsset carTextAsset;

    [Space(10)]
    [SerializeField] GameObject musicSelectBox;
    [SerializeField] TextMeshProUGUI musicName;

    bool MusicBoxOn = false;
    int currentMusicIndex = 0;
    int currentMusic = 0;
    // �κ�, ���� ������ ������ �� ���� �����̶�� �������� ���ؼ� -1
    // ���� ���� ������ �����Ѵٸ� �� ���� �����ϰ��ִ� UserInfo�� ���� ���� ���� ���� �����;� �Ѵ�
    int maxMusicCount;

    private void Awake()
    {
        uniqueInstance = this;
    }

    void Start()
    {
        DOTween.Init(); // �� Ʈ�� �ʱ�ȭ
        StartFadeBlackPanel(); //���� �г� ġ���
        maxMusicCount = DataManager.Instance.bgmDictionary.Count - 1;
        carIndexMax = DataManager.Instance.garageCarPrefab.Count;
        GameManager.Instance.gameState = GameState.Garage;

        SpawnCar(1);
    }


    public void SpawnCar(int direction)
    {
        DespawnCar();
        carIndex += direction;
        // carIndex�� ������ �ʰ��ϸ� 0 �Ǵ� carIndexMax�� ����
        if (carIndex < 0)
        {
            carIndex = carIndexMax;
        }
        else if (carIndex > carIndexMax)
        {
            carIndex = 0;
        }
        currentCar = DataManager.Instance.garageCarPrefab[carIndex];
        GameObject car = Instantiate(currentCar, carSpawnPosition.transform);
        car.transform.SetParent(carSpawnPosition.transform);
        GameManager.Instance.carName = currentCar.name;
        UpdateCarInfoPanel(currentCar.name);
        //DataManager.Instance.UpdateCarData(carTextAsset);
    }

    public void DespawnCar()
    {
        if (carSpawnPosition.transform.childCount != 0)
        {
            Destroy(carSpawnPosition.transform.GetChild(0).gameObject);
        }
    }

      
    public void UpdateCarInfoPanel(string name)
    {
        //carInfoText.text =

        //foreach (var a in DataManager.Instance.textAsset)
        //{
        //    if(a.name == name)
        //    {
        //        carTextAsset = a;
        //    }

        //}

        //carInfoText.text = carTextAsset.ToString();


        carInfoTextLeft.text = "";
        carInfoTextLeft.text += "NAME" + "\n";
        carInfoTextLeft.text += "STEER ANGLE" + "\n";
        carInfoTextLeft.text += "TRANSMISSION" + "\n";
        carInfoTextLeft.text += "DRIVING SYSTEM" + "\n";
        carInfoTextLeft.text += "MAX RPM" + "\n";
        carInfoTextLeft.text += "MAX MOTOR POWER" + "\n";
        carInfoTextLeft.text += "GEAR RATIO" + "\n";

        carInfoTextRight.text = "";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].name + "\n";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].Handling.MaxSteerAngle + "\n";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].Engine.Transmission + "\n";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].Engine.WheelWork + "\n";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].Engine.MaxRPM + "\n";
        carInfoTextRight.text += DataManager.Instance.carDictionary[carIndex].Engine.MaxMotorTorque + "\n";
        foreach (var a in DataManager.Instance.carDictionary[carIndex].Gear.GearRatios)
        {
            carInfoTextRight.text += a + "\t";
        }

        

    }

    void StartFadeBlackPanel()
    {
        startBlackPanel.SetActive(true);
        startBlackPanel.GetComponent<Image>().DOFade(0, 3); //���� �г� �����
    }
    public void OnClickStartGame()
    {
        int carCount = DataManager.Instance.garageCarPrefab.Count;
        // ������ �ε����� ������ ��� �������� ����
        DataManager.Instance.playerCar = carIndex;
        DataManager.Instance.enemyCar = Random.Range(0, carCount);

        if(currentMusic == 0)
        {
            currentMusic = Random.Range(1, maxMusicCount);
        }
        SceneControlManager.Instance.StartIngameScene(carIndex, DataManager.Instance.enemyCar, currentMusic + 1);

        //StartCoroutine(StartGame(1));
    }

    private string weather;
    IEnumerator StartGame(float time)
    {
        yield return new WaitForSeconds(time);
        
        //switch(GameManager.Instance.mapWeatherState)
        //{
        //    case MapWeatherState.Autumn: weather = "Autumn"; break;
        //    case MapWeatherState.Summer: weather = "Summer"; break;
        //    case MapWeatherState.Winter: weather = "Winter"; break;

        //}


        // SceneManager.LoadScene(weather + "_"+GameManager.Instance.Map);
         SceneManager.LoadScene("InGame");
    }


    public void OnclickBackToMain()
    {
        SceneControlManager.Instance.StartLobbyScene();
        //SceneManager.LoadScene("Lobby");
    }

    
    public void OnClickMusicBox()
    {
        if (!MusicBoxOn)
        {
            musicSelectBox.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f);
            MusicBoxOn = true;
        }
        else
        {
            musicSelectBox.GetComponent<RectTransform>().DOAnchorPosX(400f, 0.3f);
            MusicBoxOn = false;
        }
    }

    public void ClickNextMusic()
    {
        currentMusic++;
        if(currentMusic >= maxMusicCount) 
        {
            currentMusic = 0;
        }

        if(currentMusic != 0)
        {
            musicName.text = DataManager.Instance.bgmDictionary[currentMusic + 1];
            SoundManager.instance.BGMFadeIn();
            SoundManager.instance.PlayBGM(currentMusic + 1);
        }
        else
        {
            // 0�϶��� ������ ���õ� ������ ����
            musicName.text = "Random";
            SoundManager.instance.BGMFadeIn();
            SoundManager.instance.PlayBGM(1);
        }

    }
    public void ClickPreviousMusic()
    {
        currentMusic--;
        if (currentMusic < 0)
        {
            currentMusic = maxMusicCount - 1;
        }

        if (currentMusic != 0)
        {
            musicName.text = DataManager.Instance.bgmDictionary[currentMusic + 1];
            SoundManager.instance.BGMFadeIn();
            SoundManager.instance.PlayBGM(currentMusic + 1);
        }
        else
        {
            // 0�϶��� ������ ���õ� ������ ����
            musicName.text = "Random";
            SoundManager.instance.BGMFadeIn();
            SoundManager.instance.PlayBGM(1);
        }
    }


    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    SpawnCar(1);
        //}
        //if (Input.GetMouseButtonDown(1))
        //{
        //    SpawnCar(-1);
        //}
    }
}

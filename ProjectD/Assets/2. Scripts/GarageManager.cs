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
    // Start is called before the first frame update
    private void Awake()
    {
        uniqueInstance = this;
    }

    void Start()
    {
        DOTween.Init(); // �� Ʈ�� �ʱ�ȭ
        StartFadeBlackPanel(); //���� �г� ġ���

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

        SceneControlManager.Instance.StartIngameScene(carIndex, DataManager.Instance.enemyCar);

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
        SceneManager.LoadScene("Lobby");
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

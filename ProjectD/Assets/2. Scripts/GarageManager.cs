using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;


public class GarageManager : MonoBehaviour
{

    [SerializeField] GameObject startBlackPanel;
    [SerializeField] GameObject carSpawnPosition;
    [SerializeField] GameObject currentCar;
    [SerializeField] int carIndex = 0;
    [SerializeField] int carIndexMax;
    [SerializeField] TextMeshProUGUI carInfoText;
    public TextAsset carTextAsset;
    // Start is called before the first frame update
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
        if (carIndex < 1)
        {
            carIndex = carIndexMax;
        }
        else if (carIndex > carIndexMax)
        {
            carIndex = 1;
        }
        currentCar = DataManager.Instance.garageCarPrefab[carIndex-1];
        GameObject car = Instantiate(currentCar, carSpawnPosition.transform);
        car.transform.SetParent(carSpawnPosition.transform);
        GameManager.Instance.carName = currentCar.name;
        UpdateCarInfoPanel(currentCar.name);
        DataManager.Instance.UpdateCarData(carTextAsset);
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

        foreach (var a in DataManager.Instance.textAsset)
        {
           if(a.name == name)
            {
                carTextAsset = a;
            }
            
        }

        carInfoText.text = carTextAsset.ToString();

    }

    void StartFadeBlackPanel()
    {
        startBlackPanel.SetActive(true);
        startBlackPanel.GetComponent<Image>().DOFade(0, 3); //���� �г� �����
    }

    // Update is called once per frame
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

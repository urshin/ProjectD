using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using AutoMoverPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;

public class LobbyUIHandler : MonoBehaviour
{
    public enum LobbyState
    {
        Logo,
        Main,
        SelectGameMode,
        StroyMode,
        VersusMode,
        MapSelect,
        Option,
        
    }
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject startGamePanel;
    [SerializeField] GameObject storyModePanel;
    [SerializeField] GameObject versusModePanel;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject mapSelectPanel;
    [SerializeField] GameObject backBTN;
    [SerializeField] GameObject logo;
    [SerializeField] GameObject logocar;
    [SerializeField] GameObject startCar;
    [SerializeField] GameObject[] maps;

    [SerializeField] TMP_Dropdown timeDropdown;
    [SerializeField] TMP_Dropdown weatherDropdown;

    [SerializeField] GameObject lapTimeTextBox;
    [SerializeField] GameObject lapTimeBoxPrefab;

    [SerializeField] GameObject isReverse;
    public LobbyState lobbystate;

    [SerializeField] TextMeshProUGUI sensitivityText;
    [SerializeField] Slider sensitivity;
    [SerializeField] Slider mainVolume;
    [SerializeField] Slider bgmVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] Toggle autoCounter;
    [SerializeField] Image autoCounterToggleImage;
    public SpeeoMeterState speeoMeter;

    void Start()
    {
        InitializedRestart();
    }

    void InitializedRestart()
    {
        lobbystate = LobbyState.Logo;
        GameManager.Instance.gameState = GameState.Lobby;
        GameManager.Instance.Map = 4;
        GameManager.Instance.mapWeatherState = MapWeatherState.Autumn;
        GameManager.Instance.mapTimeState = MapTimeState.Cold_Night;

        sensitivity.value = UserInfoManager.instance.Option.sensitivity;
        mainVolume.value = UserInfoManager.instance.Option.mainVolume;
        bgmVolume.value = UserInfoManager.instance.Option.bgmVolume;
        sfxVolume.value = UserInfoManager.instance.Option.sfxVolume;
        autoCounter.isOn = UserInfoManager.instance.Option.autoCounter;

        SetSensitivity();
        SetGlobalVolume();
        SetBGMVolume();
        SetSFXVolume();
        SetAutoCounter();

        HideAllPanel();

        ShowingLogo();
    }


    // Update is called once per frame
    void Update()
    {
        if(lobbystate == LobbyState.Logo || lobbystate == LobbyState.Main)
        {
            backBTN.SetActive(false);
        }
        else
        {
            backBTN.SetActive(true);
        }

        if(lobbystate == LobbyState.Logo)
        {
            if(Input.anyKeyDown)
            {
                MainPanel();
                logocar.GetComponent<AutoMover>().StartMoving();
            }
        }

        maps[0].transform.parent.Rotate(Vector3.up * 3 * Time.deltaTime); //맵 회전시키기

    }


    public void MainPanel() 
    {
        HideAllPanel();
        mainPanel.SetActive(true);
        lobbystate = LobbyState.Main;
    }

    public void HideAllPanel()
    {
        mainPanel.SetActive(false);
        startGamePanel.SetActive(false);
        storyModePanel.SetActive(false);
        versusModePanel.SetActive(false);
        optionPanel.SetActive(false);
        mapSelectPanel.SetActive(false);
        backBTN.SetActive(false);
    }

    public void ShowingLogo()
    {
        logo.GetComponent<TextMeshProUGUI>().DOFade(255, 500);
        logo.transform.GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(255, 500);
    }
    public void OnClickStartBTN()
    {
        HideAllPanel();
        startGamePanel.SetActive(true);
        lobbystate = LobbyState.SelectGameMode;


    }
    public void OnClickStoryBTN()
    {
        HideAllPanel();
        mapSelectPanel.SetActive(true);
        lobbystate = LobbyState.StroyMode;

        MapSelect();
    }
    public void OnClickVersusBTN()
    {
        HideAllPanel();
        mapSelectPanel.SetActive(true);
        lobbystate = LobbyState.VersusMode;
        MapSelect();
    }
    
    public void MapSelect()
    {
        HideAllPanel();
        logo.SetActive(false);

        mapSelectPanel.SetActive(true);
        lobbystate = LobbyState.MapSelect;
        OnClickMap(4);
    }

   
    public void OnClickMap(int num)
    {
        GameManager.Instance.Map = num;

        foreach(var a in maps)
        {
            a.gameObject.SetActive(false);
        }
        
        maps[num-4].gameObject.SetActive(true);
        timeDropdown.value = 0;
        weatherDropdown.value = 0;
        InitLapTimeBox();
    }

    public void TimeDropDownEvent(TMP_Dropdown select)
    {
        string op = select.options[select.value].text;
        switch (op)
        {
            case "Cold Night":
                GameManager.Instance.mapTimeState = MapTimeState.Cold_Night;
                break;
            case "Cold Sunset":
                GameManager.Instance.mapTimeState = MapTimeState.Cold_Sunset;
                break;
            case "Deep Dusk":
                GameManager.Instance.mapTimeState = MapTimeState.Deep_Dusk;
                break;
            case "BlueSunset":
                GameManager.Instance.mapTimeState = MapTimeState.BlueSunset;
                break;
            case "Night MoonBurst":
                GameManager.Instance.mapTimeState = MapTimeState.Night_MoonBurst;
                break;

        }
    }

    public void WeatherDropDownEvent(TMP_Dropdown select)
    {
        string op = select.options[select.value].text;

        switch (op)
        {
            case "Autumn":
                GameManager.Instance.mapWeatherState = MapWeatherState.Autumn;
                break;
            case "Summer":
                GameManager.Instance.mapWeatherState = MapWeatherState.Summer;
                break;
            case "Winter":
                GameManager.Instance.mapWeatherState = MapWeatherState.Winter;
                break;
        }

        InitLapTimeBox();
    }

    void InitLapTimeBox()
    {
        foreach (Transform child in lapTimeTextBox.transform)
        {
            Destroy(child.gameObject);
        }

        int currentLapTimeIndex = (int)GameManager.Instance.mapWeatherState * 100 + GameManager.Instance.Map;

        if (UserInfoManager.instance.BestLapTime.TryGetValue(currentLapTimeIndex, out float time))
        {
            GameObject go = Instantiate(lapTimeBoxPrefab, lapTimeTextBox.transform);
            go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Best Lap";
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = minutes.ToString() + " : " + seconds.ToString() + " : " + milliseconds.ToString();

            int lapTimeCounter = UserInfoManager.instance.LapTime[currentLapTimeIndex].Count;

            for (int i = 0; i < lapTimeCounter; i++)
            {
                go = Instantiate(lapTimeBoxPrefab, lapTimeTextBox.transform);
                go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Recent Lap " + (i + 1).ToString();
                float timer = UserInfoManager.instance.LapTime[currentLapTimeIndex][i];
                minutes = Mathf.FloorToInt(timer / 60f);
                seconds = Mathf.FloorToInt(timer % 60f);
                milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
                go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = minutes.ToString() + " : " + seconds.ToString() + " : " + milliseconds.ToString();
            }
        }

        //if (UserInfoManager.instance.LapTime.TryGetValue(currentLapTimeIndex, out List<float> value))
        //{
        //    int lapTimeCounter = UserInfoManager.instance.LapTime[currentLapTimeIndex].Count;

        //    for (int i = 0; i < lapTimeCounter; i++)
        //    {
        //        GameObject go = Instantiate(lapTimeBoxPrefab, lapTimeTextBox.transform);
        //        go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Recent Lap " + (i + 1).ToString();
        //        float timer = UserInfoManager.instance.LapTime[currentLapTimeIndex][i];
        //        int minutes = Mathf.FloorToInt(timer / 60f);
        //        int seconds = Mathf.FloorToInt(timer % 60f);
        //        int milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
        //        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = minutes.ToString() + " : " + seconds.ToString() + " : " + milliseconds.ToString();
        //    }
        //}
    }

    public void OnClickReverseButton()
    {
        GameManager.Instance.isReverse = !GameManager.Instance.isReverse;
        if (GameManager.Instance.isReverse)
        {
            isReverse.GetComponent<Image>().color = Color.white;
        }
        else
        {
            isReverse.GetComponent<Image>().color = Color.gray;
        }
        
    }

    public void OnClickSelectCar()
    {
        SceneControlManager.Instance.StartGarageScene();
        //StartCoroutine(  GoToGarage(3));
    }
    public void OnClickOptionBTN()
    {
        HideAllPanel();
        optionPanel.SetActive(true);
        lobbystate = LobbyState.Option;

    }
    public void OnClickExitBTN()
    {
        HideAllPanel();
        Application.Quit();

    }





 
    IEnumerator GoToGarage(float time)
    {
        startCar.GetComponent<AutoMover>().StartMoving();
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene("GarageScene1");
    }
    public void OnClickBackBTN()
    {
        switch(lobbystate)
        {
            case LobbyState.Logo:
                break;
            case LobbyState.Main:
                break;
            case LobbyState.SelectGameMode:
                HideAllPanel();
                mainPanel.SetActive(true);
                lobbystate = LobbyState.Main;

                break;
            case LobbyState.StroyMode:
                HideAllPanel();
                startGamePanel.SetActive(true);
                lobbystate = LobbyState.SelectGameMode;
                break;
            case LobbyState.VersusMode:
                HideAllPanel();
                startGamePanel.SetActive(true);
                lobbystate = LobbyState.SelectGameMode;
                break;
            case LobbyState.Option:
                HideAllPanel();
                mainPanel.SetActive(true);
                lobbystate = LobbyState.Main;

                break;
            case LobbyState.MapSelect:
                HideAllPanel();
                startGamePanel.SetActive(true);
                lobbystate = LobbyState.SelectGameMode;
                break;
        }
    }

    
    public void SetSensitivity()
    {
        sensitivityText.text = ((int)sensitivity.value).ToString();
    }
    public void SetGlobalVolume()
    {
        AudioListener.volume = mainVolume.value;
    }
    public void SetBGMVolume()
    {
        SoundManager.instance.SetBGMVolume(bgmVolume.value);
    }
    public void SetSFXVolume()
    {
        SoundManager.instance.SetSFXVolume(sfxVolume.value);
    }
    public void SetAutoCounter()
    {
        if (autoCounter.isOn)
        {
            autoCounterToggleImage.color = Color.blue;
            GameManager.Instance.isAutoCounter = true;
        }
        else
        {
            autoCounterToggleImage.color = Color.red;
            GameManager.Instance.isAutoCounter = false;
        }
    }
    public void GageDropDownEvent(TMP_Dropdown select)
    {
        string op = select.options[select.value].text;
        switch (op)
        {
            case "Horizontal":
               speeoMeter = SpeeoMeterState.Horizontal;
                break;
            case "TacoMeter":
                speeoMeter = SpeeoMeterState.TacoMeter;
                break;
           

        }
    }

    // 옵션 창에서 back 버튼을 눌러 나오는 시점에서 옵션 정보를 세이브한다
    public void SaveOptions()
    {
        UserInfoManager.instance.SaveOption(sensitivity.value, mainVolume.value, bgmVolume.value, sfxVolume.value, autoCounter.isOn, speeoMeter);
    }
}

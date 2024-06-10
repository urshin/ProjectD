using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public void Awake()
    {
        if (Instance == null) //�������� �ڽ��� üũ��, null����
        {
            Instance = this; //���� �ڱ� �ڽ��� ������.
        }
        DontDestroyOnLoad(gameObject);
    }

    public string carName;

    public GameMode gameMode;
    public GameState gameState;
    public MapWeatherState mapWeatherState;
    public MapTimeState mapTimeState;
    public int Map;
    public bool isReverse;

    public bool isAutoCounter = true;

    private void Start()
    {
        gameMode = GameMode.None;
        gameState = GameState.Lobby;
    }
    private void Update()
    {
     

    }
    public void LockMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
       
    }
    public void FreeMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;

    }
}
public enum GameMode
{
    AI,
    TimeAttack,
    Story,
    None,
}
public enum GameState
{
    Lobby,
    Garage,
    InGame,
}

public enum MapWeatherState
{
    Autumn,
    Summer,
    Winter,
}
public enum MapTimeState
{
    Cold_Night,
    Cold_Sunset,
    Deep_Dusk,
    BlueSunset,
    Night_MoonBurst,
}
public enum SpeeoMeterState
{
    Horizontal,
    TacoMeter,
}
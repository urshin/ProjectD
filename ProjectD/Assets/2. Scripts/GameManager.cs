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
    }
    
    public string carName;
    public GameMode gameMode;
    public GameState gameState;


    private void Start()
    {
        gameMode = GameMode.None;
        gameState = GameState.Lobby;
    }
    private void Update()
    {
        
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LobbyUIHandler : MonoBehaviour
{
    public enum LobbyState
    {
        Logo,
        Main,
        SelectGameMode,
        StroyMode,
        VersusMode,
        Option,
        
    }
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject startGamePanel;
    [SerializeField] GameObject storyModePanel;
    [SerializeField] GameObject versusModePanel;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject garagePanel;
    [SerializeField] GameObject backBTN;
    public LobbyState lobbystate;

    // Start is called before the first frame update
    void Start()
    {
        HideAllPanel();
        mainPanel.SetActive(true);
        lobbystate = LobbyState.Logo;
        lobbystate = LobbyState.Main;


    }

    // Update is called once per frame
    void Update()
    {
        if(lobbystate != LobbyState.Logo && lobbystate != LobbyState.Main)
        {
            backBTN.SetActive(true);
        }
    }

    


    public void HideAllPanel()
    {
        mainPanel.SetActive(false);
        startGamePanel.SetActive(false);
        storyModePanel.SetActive(false);
        versusModePanel.SetActive(false);
        optionPanel.SetActive(false);
        garagePanel.SetActive(false);
        backBTN.SetActive(false);
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
        storyModePanel.SetActive(true);
        lobbystate = LobbyState.StroyMode;


    }
    public void OnClickVersusBTN()
    {
        HideAllPanel();
        versusModePanel.SetActive(true);
        lobbystate = LobbyState.VersusMode;


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
        UnityEditor.EditorApplication.isPlaying = false;

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
                break;
        }
    }
}

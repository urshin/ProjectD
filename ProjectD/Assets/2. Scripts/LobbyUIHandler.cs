using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIHandler : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject startGamePanel;
    [SerializeField] GameObject storyModePanel;
    [SerializeField] GameObject versusModePanel;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject garagePanel;


    // Start is called before the first frame update
    void Start()
    {
        HideAllPanel();
        mainPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    


    void HideAllPanel()
    {
        mainPanel.SetActive(false);
        startGamePanel.SetActive(false);
        storyModePanel.SetActive(false);
        versusModePanel.SetActive(false);
        optionPanel.SetActive(false);
        garagePanel.SetActive(false);
    }


}

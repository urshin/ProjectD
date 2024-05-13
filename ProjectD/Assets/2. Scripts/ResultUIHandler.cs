using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winOrLoseText;

    public void DisableUI()
    {
        gameObject.SetActive(false);
    }

    public void ActiveUI(bool isWin)
    {
        gameObject.SetActive(true);
        if(isWin)
        {
            winOrLoseText.text = "You Win";
        }
        else 
        {
            winOrLoseText.text = "You Lose";
        }

    }


    public void OnRestartButtonClick()
    {
        //DataManager.Instance.
        SceneControlManager.Instance.StartIngameScene(DataManager.Instance.playerCar, DataManager.Instance.enemyCar, SoundManager.instance.CurrentBgm);
    }

    public void OnToGarageButtonClick()
    {
        SceneControlManager.Instance.StartGarageScene();
    }

    public void OnToMainButtonClick()
    {
        SceneControlManager.Instance.StartLobbyScene();
    }
}

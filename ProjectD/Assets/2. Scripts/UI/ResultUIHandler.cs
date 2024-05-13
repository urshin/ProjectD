using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winOrLoseText;
    [SerializeField] TextMeshProUGUI lapTimeText;

    public void DisableUI()
    {
        gameObject.SetActive(false);
    }

    public void ActiveUI(bool isWin, float lapTime)
    {
        gameObject.SetActive(true);
        if(isWin)
        {
            winOrLoseText.text = "You Win";
            float minutes = Mathf.FloorToInt(lapTime / 60f);
            float seconds = Mathf.FloorToInt(lapTime % 60f);
            float milliseconds = Mathf.FloorToInt((lapTime * 100f) % 100f);
            lapTimeText.text = minutes.ToString().PadLeft(2, '0') + " : " + seconds.ToString().PadLeft(2, '0') + " : " + milliseconds.ToString()[..2];

            // + 이 랩타임 정보를 유저 정보 매니저에 보내서 최고 기록과 비교해 저장해야 한다
        }
        else 
        {
            winOrLoseText.text = "You Lose";
            lapTimeText.text = "Fail";
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControlManager : MonoBehaviour
{
    static SceneControlManager uniqueInstance;

    public static SceneControlManager Instance
    {
        get { return uniqueInstance; }
    }

    private void Awake()
    {
        uniqueInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartLobbyScene()
    {
        StartCoroutine(LoadLobbyScene());
    }

    IEnumerator LoadLobbyScene()
    {
        SoundManager.instance.PlayBGM(0);
        AsyncOperation ao;
        ao = SceneManager.LoadSceneAsync("Lobby");
        while (!ao.isDone)
        {
            yield return null;
        }
    }


    public void StartGarageScene()
    {
        StartCoroutine(LoadGarageScene());
    }

    IEnumerator LoadGarageScene()
    {
        SoundManager.instance.PlayBGM(1);
        AsyncOperation ao;
        ao = SceneManager.LoadSceneAsync("GarageScene1");
        while(!ao.isDone)
        {
            yield return null;
        }

        // �ʱ�ȭ
    }


    public void StartIngameScene(int playerCarIndex, int enemyCarIndex)
    {
        StartCoroutine(LoadIngameScene(playerCarIndex, enemyCarIndex));
    }

    IEnumerator LoadIngameScene(int playerCarIndex, int enemyCarIndex)
    {
        SoundManager.instance.PlayBGM(Random.Range(2, DataManager.Instance.bgmDictionary.Count));

        AsyncOperation ao;

        ao = SceneManager.LoadSceneAsync("InGame");
        while (!ao.isDone)
        {
            yield return null;
        }

        // �ΰ��ӸŴ����� �̱������� ����� �ʱ�ȭ ȣ��
        InGameManager.instance.InitGame(playerCarIndex, enemyCarIndex);
    }
}

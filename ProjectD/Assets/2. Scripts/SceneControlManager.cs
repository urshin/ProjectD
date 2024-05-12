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

    private void Start()
    {
        StartLobbyScene();
    }


    public void StartLobbyScene()
    {
        StartCoroutine(LoadLobbyScene());
    }

    IEnumerator LoadLobbyScene()
    {
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

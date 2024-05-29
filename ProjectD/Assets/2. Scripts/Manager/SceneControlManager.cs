using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneControlManager : MonoBehaviour
{
    static SceneControlManager uniqueInstance;

    [SerializeField] Volume ppc;
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
       // ppc.weight = 1;
        while (!ao.isDone)
        {
            yield return null;
        }
        System.GC.Collect();
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
        //ppc.weight = 0;
        while(!ao.isDone)
        {
            yield return null;
        }
        System.GC.Collect();


        // 초기화
    }


    public void StartIngameScene(int playerCarIndex, int enemyCarIndex, int bgmIndex)
    {
        StartCoroutine(LoadIngameScene(playerCarIndex, enemyCarIndex, bgmIndex));
    }

    IEnumerator LoadIngameScene(int playerCarIndex, int enemyCarIndex, int bgmIndex)
    {
        //SoundManager.instance.PlayBGM(Random.Range(2, DataManager.Instance.bgmDictionary.Count));
        SoundManager.instance.PlayBGM(bgmIndex);
        AsyncOperation ao;

        ao = SceneManager.LoadSceneAsync("InGame");
        //ppc.weight = 1;
        GameManager.Instance.gameState = GameState.InGame;

        while (!ao.isDone)
        {
            yield return null;
        }
        System.GC.Collect();
        // 인게임매니저를 싱글톤으로 만들고 초기화 호출
        InGameManager.instance.InitGame(playerCarIndex, enemyCarIndex);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public void Awake()
    {
        if (Instance == null) //정적으로 자신을 체크함, null인진
        {
            Instance = this; //이후 자기 자신을 저장함.
        }
    }
    public GameObject player;

    public string Carname;
    public List<Transform> TrackPosition;
    int trackIndex = 0;

    private void Start()
    {
        
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            player.transform.position = TrackPosition[trackIndex].position;

            if(trackIndex >= TrackPosition.Count-1)
            {
                trackIndex = 0;
            }
            else
            {
                trackIndex++;
            }
            
        }
    }

}

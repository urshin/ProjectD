using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   public GameObject player;

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

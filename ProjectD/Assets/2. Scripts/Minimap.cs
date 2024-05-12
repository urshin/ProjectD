using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Minimap : MonoBehaviour
{
    [SerializeField] Transform playerTrans;
    [SerializeField] Transform MinimapPlayerTrans;
    [SerializeField] Transform enemyTrans;
    [SerializeField] Transform miniMapEnemyTrans;
    [SerializeField] Camera cam;
    [SerializeField] GameObject[] miniMaps;

    bool initialized = false;

    public void InitialzeMiniMap(int mapIndex, GameObject player, GameObject enemy)
    {
        foreach (var map in miniMaps)
        {
            map.SetActive(false);
        }
        miniMaps[mapIndex].SetActive(true);

        playerTrans = player.transform;
        enemyTrans = enemy.transform;

        initialized = true;
    }


    private void Update()
    {
        if(initialized)
        {
            MinimapPlayerTrans.rotation = Quaternion.Euler(90, playerTrans.rotation.eulerAngles.y, 0);
            MinimapPlayerTrans.position = new Vector3(playerTrans.position.x, transform.position.y + 0.2f, playerTrans.position.z);

            miniMapEnemyTrans.rotation = Quaternion.Euler(90, enemyTrans.rotation.eulerAngles.y, 0);
            miniMapEnemyTrans.position = new Vector3(enemyTrans.position.x, transform.position.y + 0.2f, enemyTrans.position.z);
            //cam.transform.position = MinimapPlayerTrans.position + new Vector3(-2,5,0);
        }
    }
}

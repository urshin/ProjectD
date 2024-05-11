using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Minimap : MonoBehaviour
{
    [SerializeField] Transform playerTrans;
    [SerializeField] Transform MinimapPlayerTrans;
    [SerializeField] Camera cam;
    [SerializeField] GameObject[] miniMaps;
    private void OnEnable()
    {
        //후에 minimaps에서 현재 맵에 맞는 미니맵 setactive;
    }
    private void Update()
    {
        Quaternion newRotation = Quaternion.Euler(90, playerTrans.rotation.eulerAngles.y, 0);
        MinimapPlayerTrans.rotation = newRotation; 
        MinimapPlayerTrans.position = new Vector3(playerTrans.position.x,transform.position.y+0.2f,playerTrans.position.z);
        //cam.transform.position = MinimapPlayerTrans.position + new Vector3(-2,5,0);
    }
}

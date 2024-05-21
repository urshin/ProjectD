using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetLight : MonoBehaviour
{
    GameObject player;
    Light streetLight;
    [SerializeField] float LimitDistance = 50;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject;
        streetLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        // player가 없다면 빠져나갑니다.
        if (player == null)
            return;

        // player와의 거리 계산
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // player와의 거리가 30 이하라면 빛을 켭니다.
        if (distance <  LimitDistance)
        {
            streetLight.enabled = true;
        }
        else
        {
            streetLight.enabled = false;
        }
    }
}
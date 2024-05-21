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
        // player�� ���ٸ� ���������ϴ�.
        if (player == null)
            return;

        // player���� �Ÿ� ���
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // player���� �Ÿ��� 30 ���϶�� ���� �մϴ�.
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour
{
    [SerializeField] bool isReverse;

    private void OnTriggerEnter(Collider other)
    {
        if(isReverse == GameManager.Instance.isReverse)
        {
            // 충돌 감지는 차량이 하지만 태그는 그 상위인 플레이어 컨트롤러 또는 AI 컨트롤러에 달려있기 때문에 부모에게서 검사
            if (other.transform.parent.CompareTag("Player"))
            {
                InGameManager.instance.OnGameEnd(true);
            }
            else if (other.transform.parent.CompareTag("Enemy"))
            {
                InGameManager.instance.OnGameEnd(false);
            }
        }
    }
}

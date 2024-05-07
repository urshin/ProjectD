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
            // �浹 ������ ������ ������ �±״� �� ������ �÷��̾� ��Ʈ�ѷ� �Ǵ� AI ��Ʈ�ѷ��� �޷��ֱ� ������ �θ𿡰Լ� �˻�
            if (other.transform.parent.CompareTag("Player"))
            {
                Debug.Log("Player Win");
            }
            else if (other.transform.parent.CompareTag("Enemy"))
            {
                Debug.Log("Enemy Win");
            }
        }
    }
}

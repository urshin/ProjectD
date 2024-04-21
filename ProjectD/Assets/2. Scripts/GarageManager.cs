using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class GarageManager : MonoBehaviour
{

    [SerializeField] GameObject startBlackPanel;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init(); // �� Ʈ�� �ʱ�ȭ
        StartFadeBlackPanel(); //���� �г� ġ���




    }

    void StartFadeBlackPanel()
    {
        startBlackPanel.SetActive(true);
        startBlackPanel.GetComponent<Image>().DOFade(0, 2); //���� �г� �����
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

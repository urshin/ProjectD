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
        DOTween.Init(); // 닷 트윈 초기화
        StartFadeBlackPanel(); //검은 패널 치우기




    }

    void StartFadeBlackPanel()
    {
        startBlackPanel.SetActive(true);
        startBlackPanel.GetComponent<Image>().DOFade(0, 2); //검은 패널 지우기
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

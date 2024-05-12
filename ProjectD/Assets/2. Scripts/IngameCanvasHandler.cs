using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameCanvasHandler : MonoBehaviour
{
    [SerializeField] FinalCarController_June cc;
    [SerializeField] TextMeshProUGUI lap;
    [SerializeField] TextMeshProUGUI timeSpen;
    [SerializeField] TextMeshProUGUI RPMTMP;
    [SerializeField] TextMeshProUGUI gearTMP;
    [SerializeField] Slider gas;
    [SerializeField] Slider brake;
    [SerializeField] Slider gage;
    [SerializeField] Image ABS;

    private float timer = 0f; // 타이머 변수 초기화
    private int minutes; // 분
    private int seconds; // 초
    private int milliseconds; // 밀리초
    private void OnEnable()
    {
        if (cc == null)
        {
            //나중에 인게임 매니저에서 정보 가져오기
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.fixedDeltaTime;
        // 시간을 분, 초, 밀리초로 변환
        minutes = Mathf.FloorToInt(timer / 60f);
        seconds = Mathf.FloorToInt(timer % 60f);
        milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
        timeSpen.text = "Time : " + minutes.ToString() + ":\t" + seconds.ToString() + ":\t" + milliseconds.ToString();
        RPMTMP.text = cc.currentRPM.ToString();
        gearTMP.text = ((int)cc.currentGear).ToString();
        gas.value = cc.gasInput;
        brake.value = -1 * cc.gasInput;
        gage.value = cc.currentRPM / cc.maxRPM;

        if (cc.handBrake)
        {
            ABS.color = Color.yellow;
        }
        else
        {
            ABS.color = Color.white;
        }
    }
}

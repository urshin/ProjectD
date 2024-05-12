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

    private float timer = 0f; // Ÿ�̸� ���� �ʱ�ȭ
    private int minutes; // ��
    private int seconds; // ��
    private int milliseconds; // �и���
    private void OnEnable()
    {
        if (cc == null)
        {
            //���߿� �ΰ��� �Ŵ������� ���� ��������
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.fixedDeltaTime;
        // �ð��� ��, ��, �и��ʷ� ��ȯ
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

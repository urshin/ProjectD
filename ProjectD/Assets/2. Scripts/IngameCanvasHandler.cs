using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameCanvasHandler : MonoBehaviour
{
    public static IngameCanvasHandler Instance;

    [SerializeField] FinalCarController_June cc;
    [SerializeField] TextMeshProUGUI lap;
    [SerializeField] TextMeshProUGUI timeSpen;
    [SerializeField] TextMeshProUGUI RPMTMP;
    [SerializeField] TextMeshProUGUI gearTMP;
    [SerializeField] Slider gas;
    [SerializeField] Slider brake;
    [SerializeField] Slider gage;
    [SerializeField] Image ABS;

    CarController playerCarController;

    bool initialized;
    private float timer = 0f; // 타이머 변수 초기화
    private int minutes; // 분
    private int seconds; // 초
    private int milliseconds; // 밀리초

    public float lapTime
    {
        get { return timer; }
    }

    private void Awake()
    {
        Instance = this;
        initialized = false;
    }

    public void InitUI()
    {
        playerCarController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CarController>();
        timer = 0f;
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            timer += Time.fixedDeltaTime;
            // 시간을 분, 초, 밀리초로 변환
            minutes = Mathf.FloorToInt(timer / 60f);
            seconds = Mathf.FloorToInt(timer % 60f);
            milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
            timeSpen.text = "Time : " + minutes.ToString() + ":\t" + seconds.ToString() + ":\t" + milliseconds.ToString();
            RPMTMP.text = playerCarController.currentRPM.ToString();
            gearTMP.text = ((int)playerCarController.currentGear).ToString();
            gas.value = playerCarController.accel;
            brake.value = -1 * playerCarController.accel;
            gage.value = playerCarController.currentRPM / playerCarController.maxRPM;

            if (playerCarController.handBrake)
            {
                ABS.color = Color.yellow;
            }
            else
            {
                ABS.color = Color.white;
            }
        }
    }
}

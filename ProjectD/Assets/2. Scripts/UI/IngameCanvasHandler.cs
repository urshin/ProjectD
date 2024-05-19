using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameCanvasHandler : MonoBehaviour
{
    public static IngameCanvasHandler Instance;

    //[SerializeField] FinalCarController_June cc;
    [SerializeField] TextMeshProUGUI lap;
    [SerializeField] TextMeshProUGUI timeSpen;
    [SerializeField] TextMeshProUGUI RPMTMP;
    [SerializeField] TextMeshProUGUI gearTMP;
    [SerializeField] Slider gas;
    [SerializeField] Slider brake;
    [SerializeField] Slider gage;
    [SerializeField] Image ABS;

    public GameObject pausedCanvas;
    [SerializeField] TextMeshProUGUI sensitivityText;
    [SerializeField] Slider sensitivity;
    [SerializeField] Slider mainVolume;
    [SerializeField] Slider bgmVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] Toggle autoCounter;
    [SerializeField] Image autoCounterToggleImage;

    CarController_ playerCarController;

    [SerializeField] TextMeshProUGUI countdownNum;

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
        transform.GetChild(0).gameObject.SetActive(false);
       
    }

    public void InitUI()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        playerCarController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<CarController_>();
        timer = 0f;
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            timer += Time.deltaTime;
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
        else
        {
            countdownNum.fontSize = countdownNum.rectTransform.rect.width;
        }
    }

    public void StartCountDown()
    {
        // UI에서 카운트다운 되는것 표현
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        Vector2 origin = countdownNum.rectTransform.sizeDelta;

        countdownNum.text = "3";
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        yield return new WaitForSeconds(1);

        countdownNum.text = "2";
        countdownNum.rectTransform.sizeDelta = origin;
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        yield return new WaitForSeconds(1);

        countdownNum.text = "1";
        countdownNum.rectTransform.sizeDelta = origin;
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        yield return new WaitForSeconds(1);

        countdownNum.gameObject.SetActive(false);
        InGameManager.instance.StartGame();
    }


    public void SetSensitivity()
    {
        sensitivityText.text = ((int)sensitivity.value).ToString();
    }
    public void SetGlobalVolume()
    {
        AudioListener.volume = mainVolume.value;
    }
    public void SetBGMVolume()
    {
        SoundManager.instance.SetBGMVolume(bgmVolume.value);
    }
    public void SetSFXVolume()
    {
        SoundManager.instance.SetSFXVolume(sfxVolume.value);
    }
    public void SetAutoCounter()
    {
        if (autoCounter.isOn)
        {
            //autoCounterToggleImage.color = Color.blue;
        }
        else
        {
            //autoCounterToggleImage.color = Color.red;
        }
    }

    // 옵션 창에서 back 버튼을 눌러 나오는 시점에서 옵션 정보를 세이브한다
    public void SaveOptions()
    {
        UserInfoManager.instance.SaveOption(sensitivity.value, mainVolume.value, bgmVolume.value, sfxVolume.value, autoCounter.isOn);
    }
}

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
    [SerializeField] Slider handleSlider;
    [SerializeField] Slider gas;
    [SerializeField] Slider brake;
    [SerializeField] Image ABS;
    [SerializeField] GameObject gages;
    [SerializeField] Slider gage1_Slider;


    [SerializeField] Image gage2_needle;
    [SerializeField] TextMeshProUGUI gage2_RPMTMP;
    [SerializeField] TextMeshProUGUI gage2_gearTMP;



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
    int maxLap;
    private float timer = 0f; // Ÿ�̸� ���� �ʱ�ȭ
    private int minutes; // ��
    private int seconds; // ��
    private int milliseconds; // �и���

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
        maxLap= InGameManager.instance.maxLap;
        for (int i = 0; i < gages.transform.childCount; i++)
        {
            gages.transform.GetChild(i).gameObject.SetActive(false);
        }
        if (UserInfoManager.instance.Option.speeoMeter == SpeeoMeterState.Horizontal)
        {
            gages.transform.GetChild(0).gameObject.SetActive(true);
        }
       else if (UserInfoManager.instance.Option.speeoMeter == SpeeoMeterState.TacoMeter)
        {
            gages.transform.GetChild(1).gameObject.SetActive(true);
        }
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            lap.text = InGameManager.instance.PlayerLap+ "/" + maxLap; 
            timer += Time.deltaTime;
            // �ð��� ��, ��, �и��ʷ� ��ȯ
            minutes = Mathf.FloorToInt(timer / 60f);
            seconds = Mathf.FloorToInt(timer % 60f);
            milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
            timeSpen.text = "Time : " + minutes.ToString() + ":\t" + seconds.ToString() + ":\t" + milliseconds.ToString();
            handleSlider.value = playerCarController.steerValue;
            gas.value = playerCarController.accel;
            brake.value = -1 * playerCarController.accel;


            //gage1
            gage1_Slider.value = playerCarController.currentRPM / playerCarController.maxRPM;
            gearTMP.text = ((int)playerCarController.currentGear).ToString();
            RPMTMP.text = playerCarController.currentRPM.ToString();




            //gage2
            //needle 
            float rpmRatio = Mathf.Clamp01(playerCarController.currentRPM / playerCarController.maxRPM);
            float rotationZ = Mathf.Lerp(-120, 120, rpmRatio);
            gage2_needle.rectTransform.localRotation = Quaternion.Euler(0, 180, rotationZ);
            //rpm, gear
            gage2_RPMTMP.text = (Mathf.FloorToInt(playerCarController.currentRPM / 1000f)).ToString() + " X 1000";
            gage2_gearTMP.text =  ((int)playerCarController.currentGear).ToString();


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
        // UI���� ī��Ʈ�ٿ� �Ǵ°� ǥ��
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

    // �ɼ� â���� back ��ư�� ���� ������ �������� �ɼ� ������ ���̺��Ѵ�
    public void SaveOptions()
    {
        UserInfoManager.instance.SaveOption(sensitivity.value, mainVolume.value, bgmVolume.value, sfxVolume.value, autoCounter.isOn);
    }
}

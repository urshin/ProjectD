using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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


    [SerializeField] CinemachineBlendListCamera blendListCamera;

    bool initialized;
    int maxLap;
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
    private void OnEnable()
    {
        ApplyOption();
    }

    public void ApplyOption()
    {
        sensitivity.value = UserInfoManager.instance.Option.sensitivity;
        mainVolume.value = UserInfoManager.instance.Option.mainVolume;
        bgmVolume.value = UserInfoManager.instance.Option.bgmVolume;
        sfxVolume.value = UserInfoManager.instance.Option.sfxVolume;
        autoCounter.isOn = UserInfoManager.instance.Option.autoCounter;
        SetSensitivity();
        SetGlobalVolume();
        SetBGMVolume();
        SetSFXVolume();
        SetAutoCounter();
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
            // 시간을 분, 초, 밀리초로 변환
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

    public void initialCamera()//시작 카메라
    {
        GameObject cam1start = blendListCamera.transform.GetChild(0).gameObject;
        GameObject cam1end = blendListCamera.transform.GetChild(1).gameObject;
        GameObject cam2start = blendListCamera.transform.GetChild(2).gameObject;
        GameObject cam2end = blendListCamera.transform.GetChild(3).gameObject;
        Transform playerTrans = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).transform.GetChild(0).transform;
        Transform enemyTrans = GameObject.FindGameObjectWithTag("Enemy").transform.GetChild(0).transform.GetChild(0).transform;
        Vector3 midpoint = (enemyTrans.position + playerTrans.position) / 2;
        float distance = Vector3.Distance(enemyTrans.position, playerTrans.position);
        cam1start.transform.position = midpoint + playerTrans.forward*4 + playerTrans.right*(distance/2); // 필요한 경우 위치 조정
        cam1start.transform.rotation = Quaternion.LookRotation(-enemyTrans.forward); // playerTrans의 반대 방향을 바라보게 설정
        cam1end.transform.rotation = Quaternion.LookRotation(-playerTrans.forward); // playerTrans의 반대 방향을 바라보게 설정
        cam1end.transform.position = midpoint + playerTrans.forward * 4 - playerTrans.right * (distance/2); // 필요한 경우 위치 조정
        cam2start.transform.position = midpoint - playerTrans.forward * 0.5f; // 필요한 경우 위치 조정
        cam2start.transform.rotation = playerTrans.rotation; 
        cam2end.transform.position = midpoint - playerTrans.forward * 2f; // 필요한 경우 위치 조정
        cam2end.transform.rotation = playerTrans.rotation;
    }
    

    public void StartCountDown()
    {
        // UI에서 카운트다운 되는것 표현
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        Vector2 origin = countdownNum.rectTransform.sizeDelta;
        initialCamera();
        
        countdownNum.text = "READY";
        yield return new WaitForSeconds(1);




        countdownNum.text = "3";
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        blendListCamera.enabled = true;
        yield return new WaitForSeconds(1);

        countdownNum.text = "2";
        countdownNum.rectTransform.sizeDelta = origin;
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        yield return new WaitForSeconds(1);

        blendListCamera.enabled = false;
        countdownNum.text = "1";
        countdownNum.rectTransform.sizeDelta = origin;
        countdownNum.rectTransform.DOSizeDelta(new Vector2(50, 50), 0.95f);
        yield return new WaitForSeconds(1);

        InGameManager.instance.StartGame();
        Color color = countdownNum.color;
        countdownNum.text = "GO!";
        for (int i = 0; i < 10; i++)
        {

        color.a -= 0.1f; // 50% 투명도로 설정
        yield return new WaitForSeconds(0.1f);
        countdownNum.color = color;
        }
        countdownNum.rectTransform.sizeDelta = origin;


        countdownNum.gameObject.SetActive(false);
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
            GameManager.Instance.isAutoCounter = true;
        }
        else
        {
            GameManager.Instance.isAutoCounter = false;
        }
    }

    // 옵션 창에서 back 버튼을 눌러 나오는 시점에서 옵션 정보를 세이브한다
    public void SaveOptions()
    {
        UserInfoManager.instance.SaveOption(sensitivity.value, mainVolume.value, bgmVolume.value, sfxVolume.value, autoCounter.isOn);
    }
}

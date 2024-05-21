using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 24f)] private float time; // 게임 시간 (0 - 24)

    [SerializeField] private float nightFogDensity; // 밤 상태의 Fog 밀도
    [SerializeField] private float dawnFogDensity; // 새벽 상태의 Fog 밀도
    [SerializeField] private float duskFogDensity; // 노을 상태의 Fog 밀도
    private float dayFogDensity; // 낮 상태의 Fog 밀도

    [SerializeField] private Color nightFogColor; // 밤 상태의 Fog 색
    [SerializeField] private Color dawnFogColor; // 새벽 상태의 Fog 색
    [SerializeField] private Color dayFogColor; // 낮 상태의 Fog 색
    [SerializeField] private Color duskFogColor; // 노을 상태의 Fog 색

    private float currentFogDensity;

    
    void Start()
    {
        SetTimeLight();
        
    }
 
    private void SetTimeLight()
    {
        if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Night) // 밤
        {
            GetComponent<Light>().intensity = 0.1f;
            SetLightColor(nightFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Sunset) // 새벽
        {
            GetComponent<Light>().intensity = 0.4f;
            SetLightColor(dawnFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.BlueSunset) // 낮
        {
            GetComponent<Light>().intensity = 1f;
            SetLightColor(dayFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Deep_Dusk) // 노을
        {
            GetComponent<Light>().intensity = 0.4f;
            SetLightColor(duskFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Night_MoonBurst) // 밤2
        {
            GetComponent<Light>().intensity = 0.1f;
            SetLightColor(nightFogColor);
        }
    }

    private void FogSetting()
    {
        dayFogDensity = RenderSettings.fogDensity;
        currentFogDensity = dayFogDensity;

        if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Night)// 밤
        {
            SetFogDensity(nightFogDensity);
            SetLightColor(nightFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Sunset) // 새벽
        {
            SetFogDensity(dawnFogDensity);
            SetLightColor(dawnFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.BlueSunset) // 낮
        {
            SetFogDensity(dayFogDensity);
            SetLightColor(dayFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Deep_Dusk) // 노을
        {
            SetFogDensity(duskFogDensity);
            SetLightColor(duskFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Night_MoonBurst) // 밤2
        {
            SetFogDensity(0.01f);
            SetLightColor(nightFogColor);
        }
    }

    

    void SetFogDensity(float targetFogDensity)
    {
        currentFogDensity = targetFogDensity;
        RenderSettings.fogDensity = currentFogDensity;
    }

    void SetLightColor(Color color)
    {
       // RenderSettings.fogColor = color;
        GetComponent<Light>().color = color;
    }
}
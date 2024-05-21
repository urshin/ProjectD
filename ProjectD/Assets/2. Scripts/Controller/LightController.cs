using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 24f)] private float time; // ���� �ð� (0 - 24)

    [SerializeField] private float nightFogDensity; // �� ������ Fog �е�
    [SerializeField] private float dawnFogDensity; // ���� ������ Fog �е�
    [SerializeField] private float duskFogDensity; // ���� ������ Fog �е�
    private float dayFogDensity; // �� ������ Fog �е�

    [SerializeField] private Color nightFogColor; // �� ������ Fog ��
    [SerializeField] private Color dawnFogColor; // ���� ������ Fog ��
    [SerializeField] private Color dayFogColor; // �� ������ Fog ��
    [SerializeField] private Color duskFogColor; // ���� ������ Fog ��

    private float currentFogDensity;

    
    void Start()
    {
        SetTimeLight();
        
    }
 
    private void SetTimeLight()
    {
        if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Night) // ��
        {
            GetComponent<Light>().intensity = 0.1f;
            SetLightColor(nightFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Sunset) // ����
        {
            GetComponent<Light>().intensity = 0.4f;
            SetLightColor(dawnFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.BlueSunset) // ��
        {
            GetComponent<Light>().intensity = 1f;
            SetLightColor(dayFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Deep_Dusk) // ����
        {
            GetComponent<Light>().intensity = 0.4f;
            SetLightColor(duskFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Night_MoonBurst) // ��2
        {
            GetComponent<Light>().intensity = 0.1f;
            SetLightColor(nightFogColor);
        }
    }

    private void FogSetting()
    {
        dayFogDensity = RenderSettings.fogDensity;
        currentFogDensity = dayFogDensity;

        if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Night)// ��
        {
            SetFogDensity(nightFogDensity);
            SetLightColor(nightFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Cold_Sunset) // ����
        {
            SetFogDensity(dawnFogDensity);
            SetLightColor(dawnFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.BlueSunset) // ��
        {
            SetFogDensity(dayFogDensity);
            SetLightColor(dayFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Deep_Dusk) // ����
        {
            SetFogDensity(duskFogDensity);
            SetLightColor(duskFogColor);
        }
        else if (GameManager.Instance.mapTimeState == MapTimeState.Night_MoonBurst) // ��2
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
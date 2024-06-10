using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown sizeDropdown;

    private List<Resolution> resolutions = new List<Resolution>();
    private int optimalResolutionIndex = 0;

    private void Start()
    {
        // Populate resolutions list
        resolutions.Add(new Resolution { width = 1280, height = 720 });
        resolutions.Add(new Resolution { width = 1280, height = 800 });
        resolutions.Add(new Resolution { width = 1440, height = 900 });
        resolutions.Add(new Resolution { width = 1600, height = 900 });
        resolutions.Add(new Resolution { width = 1680, height = 1050 });
        resolutions.Add(new Resolution { width = 1920, height = 1080 });
        resolutions.Add(new Resolution { width = 1920, height = 1200 });
        resolutions.Add(new Resolution { width = 2048, height = 1280 });
        resolutions.Add(new Resolution { width = 2560, height = 1440 });
        resolutions.Add(new Resolution { width = 2560, height = 1600 });
        resolutions.Add(new Resolution { width = 2880, height = 1800 });
        resolutions.Add(new Resolution { width = 3440, height = 1440 });

        // Populate resolution dropdown
        resolutionDropdown.ClearOptions();
        List<string> resolutionOptions = new List<string>();
        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                optimalResolutionIndex = i;
                option += " *";
            }
            resolutionOptions.Add(option);
        }
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = optimalResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Populate size dropdown
        sizeDropdown.ClearOptions();
        List<string> sizeOptions = new List<string> { "Windowed", "Full Screen", "Exclusive Full Screen" };
        sizeDropdown.AddOptions(sizeOptions);
        sizeDropdown.value = Screen.fullScreen ? (Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? 1 : 2) : 0;
        sizeDropdown.RefreshShownValue();

        // Set initial resolution
        SetResolution(optimalResolutionIndex);

        // Add listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        sizeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        bool isFullScreen = Screen.fullScreen;
        FullScreenMode screenMode = Screen.fullScreenMode;
        Screen.SetResolution(resolution.width, resolution.height, screenMode );
    }

    public void SetScreenMode(int modeIndex)
    {
        FullScreenMode screenMode = FullScreenMode.Windowed;
        switch (modeIndex)
        {
            case 0:
                screenMode = FullScreenMode.Windowed;
                break;
            case 1:
                screenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                screenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }
        Resolution resolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, screenMode);
    }
}
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;

public class UIModule : MonoBehaviour {

    protected AudioSource _source;
    [SerializeField]
    protected AudioMixer _audioMixer;
    protected List<Resolution> _resolutions = new List<Resolution>();
    [SerializeField]
    protected Dropdown _resolutionDropdown;
    [SerializeField]
    protected Dropdown _qualityDropdown;
    [SerializeField]
    protected Slider _volumeSlider;
    [SerializeField]
    protected Toggle _vSyncToggle;
    protected bool _awaked = false;

    public virtual void Awake()
    {
        _source = GameObject.Find("Click Source").GetComponent<AudioSource>();
        _resolutionDropdown.ClearOptions();
        int currentResolutionIndex = 0;
        int listIndex = -1;
        List<string> options = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].width != 1360 && Screen.resolutions[i].width >= 1280)
            {
                if ((listIndex > -1 && _resolutions[listIndex].width != Screen.resolutions[i].width) || listIndex == -1)
                {
                    _resolutions.Add(Screen.resolutions[i]);
                    options.Add(Screen.resolutions[i].width + " x " + Screen.resolutions[i].height);
                    listIndex++;
                    if (Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height) currentResolutionIndex = listIndex;
                }
            }
        }
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();
        _qualityDropdown.value = QualitySettings.GetQualityLevel();
        _qualityDropdown.RefreshShownValue();
        float volume;
        _audioMixer.GetFloat("volume", out volume);
        _volumeSlider.value = volume;
        if (QualitySettings.vSyncCount == 0) _vSyncToggle.isOn = false;
        else _vSyncToggle.isOn = true;
        _awaked = true;
    }


    public void SetQuality(int qualityIndex)
    {
        if (_awaked)
        {
            int vSync = QualitySettings.vSyncCount;
            QualitySettings.SetQualityLevel(qualityIndex);
            QualitySettings.vSyncCount = vSync;
        }
    }

    public void SetVSync(bool vSync)
    {
        if (_awaked)
        {
            if (vSync) QualitySettings.vSyncCount = 1;
            else QualitySettings.vSyncCount = 0;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (_awaked)
        {
            Resolution resolution = _resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }

    public void OnLoadGameButton()
    {
        _source.Play();
    }

    public virtual void OnOptionsButton()
    {
        _source.Play();
    }

    public virtual void OnQuitGameButton()
    {
        _source.Play();
    }

    public virtual void OnBackButton()
    {
        _source.Play();
    }

    public void SetVolume(float volume)
    {
        _audioMixer.SetFloat("volume", volume);
    }


}

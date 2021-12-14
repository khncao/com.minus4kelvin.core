// Adapted from PixelCrushers menu system

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace m4k {
public class Options : MonoBehaviour
{

    [Header("Video")]
    public UnityEngine.UI.Toggle fullScreenToggle;
    public string fullScreenPrefsKey = "options.fullScreen";
    public TMPro.TMP_Dropdown vSyncDropdown;
    public string vSyncPrefsKey = "options.vSync";
    public TMPro.TMP_Dropdown resolutionDropdown;
    public string resolutionPrefsKey = "options.resolution";
    public TMPro.TMP_Dropdown graphicsQualityDropdown;
    public string graphicsQualityPrefsKey = "options.quality";

    [Header("Audio")]
    public AudioMixer mainMixer;
    public string musicVolumeMixerParameter = "musicVol";
    public UnityEngine.UI.Slider musicVolumeSlider;
    public string sfxVolumeMixerParameter = "sfxVol";
    public UnityEngine.UI.Slider sfxVolumeSlider;
    public string masterVolumeMixerParameter = "masterVol";
    public UnityEngine.UI.Slider masterVolumeSlider;

    [Header("Gameplay")]
    public UnityEngine.UI.Slider lookSensSlider;
    public string lookSensPrefsKey = "options.lookSens";

    // [Header("Subtitles")]
    // public UnityEngine.UI.Toggle subtitles;
    // public bool setNPCSubtitlesDuringLine = true;
    // public bool setNPCSubtitlesWithResponseMenu = true;
    // public bool setPCSubtitlesDuringLine = false;
    // public string subtitlesPrefsKey = "options.subtitles";

    // [Header("Languages")]
    // public TMPro.TMP_Dropdown languages;

    private bool m_started = false;
    private List<string> resolutionDropdownItems = new List<string>();

    private void Start()
    {
        m_started = true;

        RefreshMenuElements();
        SetMenuElements();
    }

    private void OnEnable()
    {
        if (m_started) RefreshMenuElements();
    }

    private void OnDisable()
    {
        // m_started = false;
    }

    public void SetMenuElements() 
    {
        SetFullScreen(fullScreenToggle);
        SetVSyncIndex(vSyncDropdown.value);
        SetResolutionIndex(resolutionDropdown.value);
        SetGraphicsQualityIndex(graphicsQualityDropdown.value);
        SetMasterLevel(masterVolumeSlider.value);
        SetMusicLevel(musicVolumeSlider.value);
        SetSfxLevel(sfxVolumeSlider.value);
        // SetLookSens(lookSensSlider.value);
    }

    public void RefreshMenuElements()
    {
        RefreshResolutionDropdown();
        RefreshFullscreenToggle();
        RefreshVSyncDropdown();
        RefreshGraphicsQualityDropdown();
        RefreshMusicVolumeSlider();
        RefreshSfxVolumeSlider();
        RefreshMasterVolumeSlider();
        RefreshLookSensSlider();
/*             RefreshSubtitlesToggle();
        RefreshLanguagesDropdown(); */
    }

    private void RefreshFullscreenToggle()
    {
        fullScreenToggle.isOn = GetFullScreen();
    }

    private bool GetFullScreen()
    {
        return PlayerPrefs.HasKey(fullScreenPrefsKey) ? (PlayerPrefs.GetInt(fullScreenPrefsKey) == 1) : Screen.fullScreen;
    }

    public void SetFullScreen(bool on) {
        Screen.fullScreen = on;
        PlayerPrefs.SetInt(fullScreenPrefsKey, on ? 1 : 0);
        SetResolutionIndex(GetResolutionIndex());
        SelectNextFrame(fullScreenToggle);
    }

    void RefreshVSyncDropdown()
    {
        if (PlayerPrefs.HasKey(vSyncPrefsKey)) {
            SetVSyncIndex(PlayerPrefs.GetInt(vSyncPrefsKey));
        } else {
            SetVSyncIndex(QualitySettings.vSyncCount);
        }
        var list = new List<string>(){"Unlocked", "VSync1", "VSync2"};
        vSyncDropdown.ClearOptions();
        vSyncDropdown.AddOptions(list);
        var index = GetVSyncIndex();
        vSyncDropdown.value = index;
        vSyncDropdown.captionText.text = list[index];
        SelectNextFrame(vSyncDropdown);
    }
    private int GetVSyncIndex()
    {
        return PlayerPrefs.HasKey(vSyncPrefsKey) ? PlayerPrefs.GetInt(vSyncPrefsKey) : QualitySettings.vSyncCount;
    }
    public void SetVSyncIndex(int index)
    {
        QualitySettings.vSyncCount = index;
        PlayerPrefs.SetInt(vSyncPrefsKey, index);
        SelectNextFrame(vSyncDropdown);
        // Debug.Log(index);
    }

    // 2018-07-31: Saved resolution index is now index of cleaned-up dropdown list, 
    // not Screen.resolutions array which contains duplicates.

    private string GetResolutionString(Resolution resolution)
    {
        return (resolution.refreshRate > 0) ? (resolution.width + "x" + resolution.height + " " + resolution.refreshRate + "Hz")
            : (resolution.width + "x" + resolution.height);
    }

    private void RefreshResolutionDropdownItems()
    {
        resolutionDropdownItems.Clear();
        var uniqueResolutions = Screen.resolutions.Distinct();
        foreach (var resolution in uniqueResolutions)
        {
            resolutionDropdownItems.Add(GetResolutionString(resolution));
        }
    }

    private int GetCurrentResolutionDropdownIndex()
    {
        var currentString = GetResolutionString(Screen.currentResolution);
        for (int i = 0; i < resolutionDropdownItems.Count; i++)
        {
            if (string.Equals(resolutionDropdownItems[i], currentString)) return i;
        }
        return 0;
    }

    private int ResolutionDropdownIndexToScreenResolutionsIndex(int dropdownIndex)
    {
        if (0 <= dropdownIndex && dropdownIndex < resolutionDropdownItems.Count)
        {
            var dropdownString = resolutionDropdownItems[dropdownIndex];
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                if (string.Equals(GetResolutionString(Screen.resolutions[i]), dropdownString)) return i;
            }
        }
        // If we don't find a match, return current resolution's index:
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Equals(Screen.resolutions[i], Screen.currentResolution)) return i;
        }
        return 0;
    }

    private void RefreshResolutionDropdown()
    {
        if (PlayerPrefs.HasKey(resolutionPrefsKey)) 
            SetResolutionIndex(PlayerPrefs.GetInt(resolutionPrefsKey));
        
        RefreshResolutionDropdownItems();
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionDropdownItems);
        var dropdownIndex = GetResolutionIndex();
        resolutionDropdown.value = dropdownIndex;
        resolutionDropdown.captionText.text = resolutionDropdownItems[dropdownIndex];
    }

    private int GetResolutionIndex() // Returns dropdown list index.
    {
        return PlayerPrefs.HasKey(resolutionPrefsKey) ? PlayerPrefs.GetInt(resolutionPrefsKey) : GetCurrentResolutionDropdownIndex();
    }
    public void SetResolutionIndex(int dropdownIndex) // Dropdown list index.
    {
        if (0 <= dropdownIndex && dropdownIndex < resolutionDropdownItems.Count)
        {
            var resolutionsIndex = ResolutionDropdownIndexToScreenResolutionsIndex(dropdownIndex);
            if (0 <= resolutionsIndex && resolutionsIndex < Screen.resolutions.Length)
            {
                var resolution = Screen.resolutions[resolutionsIndex];
                //if (InputDeviceManager.instance != null) InputDeviceManager.instance.BrieflyIgnoreMouseMovement(); // Mouse "moves" (resets position) when resolution changes.
                Screen.SetResolution(resolution.width, resolution.height, GetFullScreen());
                PlayerPrefs.SetInt(resolutionPrefsKey, dropdownIndex);
            }
        }
        // Debug.Log(dropdownIndex);
        SelectNextFrame(resolutionDropdown);
    }

    private void RefreshGraphicsQualityDropdown()
    {
        if (PlayerPrefs.HasKey(graphicsQualityPrefsKey)) {
            SetGraphicsQualityIndex(PlayerPrefs.GetInt(graphicsQualityPrefsKey));
        } else {
            SetGraphicsQualityIndex(QualitySettings.GetQualityLevel());
        }
        var list = new List<string>(QualitySettings.names);
        graphicsQualityDropdown.ClearOptions();
        graphicsQualityDropdown.AddOptions(list);
        var index = GetGraphicsQualityIndex();
        graphicsQualityDropdown.value = index;
        graphicsQualityDropdown.captionText.text = list[index];
        SelectNextFrame(graphicsQualityDropdown);
    }

    private int GetGraphicsQualityIndex()
    {
        return PlayerPrefs.HasKey(graphicsQualityPrefsKey) ? PlayerPrefs.GetInt(graphicsQualityPrefsKey) : QualitySettings.GetQualityLevel();
    }
    public void SetGraphicsQualityIndex(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt(graphicsQualityPrefsKey, index);
        SelectNextFrame(graphicsQualityDropdown);
    }

    private void SelectNextFrame(UnityEngine.UI.Selectable selectable)
    {
        if (/* InputDeviceManager.autoFocus &&  */selectable != null && selectable.gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(SelectNextFrameCoroutine(selectable));
        }
    }

    private IEnumerator SelectNextFrameCoroutine(UnityEngine.UI.Selectable selectable)
    {
        yield return null;
        //UITools.Select(selectable);
    }

    private void RefreshMusicVolumeSlider()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.value = PlayerPrefs.GetFloat("options." + musicVolumeMixerParameter, 0);
    }
    public void SetMusicLevel(float musicLevel)
    {
        if (!m_started) return;
        mainMixer.SetFloat(musicVolumeMixerParameter, musicLevel);
        PlayerPrefs.SetFloat("options." + musicVolumeMixerParameter, musicLevel);
    }

    private void RefreshSfxVolumeSlider()
    {
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = PlayerPrefs.GetFloat("options." + sfxVolumeMixerParameter, 0);
    }
    public void SetSfxLevel(float sfxLevel)
    {
        if (!m_started) return;
        mainMixer.SetFloat(sfxVolumeMixerParameter, sfxLevel);
        PlayerPrefs.SetFloat("options." + sfxVolumeMixerParameter, sfxLevel);
    }

    private void RefreshMasterVolumeSlider()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat("options." + masterVolumeMixerParameter, 0);
    }
    public void SetMasterLevel(float masterLevel)
    {
        if (!m_started) return;
        mainMixer.SetFloat(masterVolumeMixerParameter, masterLevel);
        PlayerPrefs.SetFloat("options." + masterVolumeMixerParameter, masterLevel);
    }

    void RefreshLookSensSlider()
    {
        if(lookSensSlider != null) lookSensSlider.value = PlayerPrefs.GetFloat(lookSensPrefsKey, 5);
    }

    public void SetLookSens(float lookSens)
    {
        if(!m_started) return;
        // CamManager.PlayerCamRig.lookSpeed = lookSens;
        PlayerPrefs.SetFloat(lookSensPrefsKey, lookSens);
    }

/*         private void RefreshSubtitlesToggle()
    {
        subtitles.isOn = PlayerPrefs.GetInt(subtitlesPrefsKey, GetDefaultSubtitlesSetting() ? 1 : 0) == 1;
    }

    public void OnSubtitlesToggleChanged()
    {
        if (!m_started) return;
        SetSubtitles(subtitles.isOn);
    } */

/*         public void SetSubtitles(bool on)
    {
        var subtitleSettings = DialogueManager.DisplaySettings.subtitleSettings;
        subtitleSettings.showNPCSubtitlesDuringLine = subtitles.isOn && setNPCSubtitlesDuringLine;
        subtitleSettings.showNPCSubtitlesWithResponses = subtitles.isOn && setNPCSubtitlesWithResponseMenu;
        subtitleSettings.showPCSubtitlesDuringLine = subtitles.isOn && setPCSubtitlesDuringLine;
        PlayerPrefs.SetInt(subtitlesPrefsKey, on ? 1 : 0);
    }

    private bool GetDefaultSubtitlesSetting()
    {
        var subtitleSettings = DialogueManager.displaySettings.subtitleSettings;
        return subtitleSettings.showNPCSubtitlesDuringLine || subtitleSettings.showNPCSubtitlesWithResponses || subtitleSettings.showPCSubtitlesDuringLine;
    }

    private void RefreshLanguagesDropdown()
    {
        if (languages == null || DialogueManager.DisplaySettings.localizationSettings.textTable == null) return;
        var language = PlayerPrefs.GetString("Language");
        var languageList = new List<string>(DialogueManager.DisplaySettings.localizationSettings.textTable.languages.Keys);
        for (int i = 0; i < languageList.Count; i++)
        {
            if (languageList[i] == language)
            {
                languages.value = i;
                return;
            }
        }
    }

    public void SetLanguageByIndex(int index)
    {
        if (DialogueManager.DisplaySettings.localizationSettings.textTable == null) return;
        var language = string.Empty;
        var languageList = new List<string>(DialogueManager.DisplaySettings.localizationSettings.textTable.languages.Keys);
        if (0 <= index && index < languageList.Count)
        {
            language = languageList[index];
        }
        var uiLocalizationManager = FindObjectOfType<UILocalizationManager>();
        if (uiLocalizationManager == null) uiLocalizationManager = gameObject.AddComponent<UILocalizationManager>();
        uiLocalizationManager.currentLanguage = language;
        Localization.language = language;
        DialogueManager.SetLanguage(language);
    } */

}
}
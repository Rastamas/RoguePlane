using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Toggle vibrateToggle;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void Awake()
    {
        vibrateToggle.isOn = PlayerPrefExtensions.GetBool(Constants.VibratePlayerPrefKey, defaultValue: true);
        musicSlider.value = PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPrefKey, 1f);
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.value = PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
        sfxSlider.onValueChanged.AddListener(ChangeSfxVolume);
    }

    public void ToggleVibrate(bool turnOn)
    {
        PlayerPrefExtensions.SetBool(Constants.VibratePlayerPrefKey, turnOn);
    }

    public void ChangeMusicVolume(float value)
    {
        BackgroundMusicController.instance.UpdateVolume();
        PlayerPrefs.SetFloat(Constants.MusicVolumePlayerPrefKey, value);
    }

    public void ChangeSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(Constants.SfxVolumePlayerPrefKey, value);
    }
}

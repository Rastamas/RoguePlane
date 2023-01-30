using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public AudioClip buttonClickClip;
    public GameObject upgradeMenuContainer;
    public GameObject challengeButton;
    public GameObject muteButton;
    public GameObject unmuteButton;

    private AudioSource audioSource;

    public void Awake()
    {
        Time.timeScale = 1;
        var soundIsOn = PlayerPrefs.GetFloat(Constants.SoundPlayerPrefKey, defaultValue: 1) == 1;

        ToggleSound(soundIsOn);
        muteButton.SetActive(soundIsOn);
        unmuteButton.SetActive(!soundIsOn);

        audioSource = GetComponentInParent<AudioSource>();

        if (GlobalVariables.TryGet<bool>("GoToUpgrade", out var goToUpgrade) && goToUpgrade)
        {
            upgradeMenuContainer.SetActive(true);
            gameObject.SetActive(false);
            GlobalVariables.Set("GoToUpgrade", false);
            return;
        }

        if (GlobalVariables.TryGet<bool>("RestartGame", out var restartGame) && restartGame)
        {
            GlobalVariables.Set("RestartGame", false);
            SceneManager.LoadScene(1);
            return;
        }

        BackgroundMusicController.instance.SwitchToMusic(MusicType.Menu);
        var buttons = new List<Button>(transform.GetComponentsInChildren<Button>(includeInactive: true));

        buttons.ForEach(button => button.onClick.AddListener(PlayButtonClickAudio));

        if (PermanentProgressionManager.savedGame.beatBoss)
        {
            challengeButton.SetActive(true);
        }
    }

    public void OnEnable()
    {
        audioSource.volume = PlayerPrefs.GetFloat(Constants.SfxVolumePlayerPrefKey, 1f);
    }

    public void Start()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("PlayGamesPlatform.Instance.IsAuthenticated");
            PlayGamesPlatform.Activate();
            if (!Social.localUser.authenticated)
            {
                Debug.Log("!Social.localUser.authenticated");
                Social.localUser.Authenticate((success) =>
                {
                    Debug.Log($"Auth result: {success}");
                });
            }
        }
        else
        {
            Debug.Log("!PlayGamesPlatform.Instance.IsAuthenticated");
        }
    }

    public void ToggleSound(bool turnOn)
    {
        AudioListener.pause = !turnOn;
        AudioListener.volume = turnOn
            ? 1
            : 0;

        PlayerPrefs.SetFloat(Constants.SoundPlayerPrefKey, AudioListener.volume);
    }

    public void ShowAchievementsUI()
    {
        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
        Social.ShowAchievementsUI();
    }

    public void OnPlayClicked()
    {
        BackgroundMusicController.instance.SwitchToMusic(MusicType.Run);
        SceneManager.LoadScene(1);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void PlayButtonClickAudio()
    {
        audioSource.PlayOneShot(buttonClickClip);
    }
}
